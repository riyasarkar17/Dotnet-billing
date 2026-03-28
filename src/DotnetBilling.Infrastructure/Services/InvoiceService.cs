using DotnetBilling.Application.DTOs.Invoices;
using DotnetBilling.Application.DTOs.Payments;
using DotnetBilling.Application.Exceptions;
using DotnetBilling.Application.Interfaces;
using DotnetBilling.Domain.Entities;
using DotnetBilling.Domain.Enums;
using DotnetBilling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetBilling.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _dbContext;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;

    public InvoiceService(AppDbContext dbContext, IInvoiceNumberGenerator invoiceNumberGenerator)
    {
        _dbContext = dbContext;
        _invoiceNumberGenerator = invoiceNumberGenerator;
    }

    public async Task<IReadOnlyList<InvoiceListItemResponse>> GetAllAsync(
        string? search,
        InvoiceStatus? status,
        Guid? customerId,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var query = _dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.InvoiceNumber.Contains(term));
        }

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId.Value);
        }

        if (status.HasValue)
        {
            query = status.Value switch
            {
                InvoiceStatus.Overdue => query.Where(x => x.Status != InvoiceStatus.Paid && x.Status != InvoiceStatus.Draft && x.DueDate.Date < today),
                InvoiceStatus.Sent => query.Where(x => x.Status == InvoiceStatus.Sent && x.DueDate.Date >= today),
                _ => query.Where(x => x.Status == status.Value)
            };
        }

        var invoices = await query
            .OrderByDescending(x => x.IssueDate)
            .ThenByDescending(x => x.InvoiceNumber)
            .ToListAsync(cancellationToken);

        return invoices.Select(invoice =>
        {
            var totalPaid = invoice.Payments.Sum(x => x.PaidAmount);
            var displayStatus = ResolveDisplayStatus(invoice.Status, invoice.DueDate, totalPaid, invoice.TotalAmount);

            return new InvoiceListItemResponse
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                CustomerName = invoice.Customer?.Name ?? string.Empty,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                Status = displayStatus,
                TotalAmount = invoice.TotalAmount,
                TotalPaid = totalPaid,
                BalanceDue = Math.Max(invoice.TotalAmount - totalPaid, 0)
            };
        }).ToList();
    }

    public async Task<InvoiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.InvoiceItems)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Invoice with id '{id}' was not found.");

        return Map(invoice);
    }

    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCustomerExistsAsync(request.CustomerId, cancellationToken);
        ValidateInvoiceDates(request.IssueDate, request.DueDate);
        ValidateInvoiceStatus(request.Status);
        ValidateInvoiceItems(request.Items);

        var invoice = new Invoice
        {
            CustomerId = request.CustomerId,
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            Status = request.Status,
            InvoiceNumber = await _invoiceNumberGenerator.GenerateAsync(cancellationToken),
            InvoiceItems = request.Items.Select(MapToEntity).ToList()
        };

        invoice.RecalculateTotals();

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(invoice.Id, cancellationToken);
    }

    public async Task<InvoiceResponse> UpdateAsync(Guid id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCustomerExistsAsync(request.CustomerId, cancellationToken);
        ValidateInvoiceDates(request.IssueDate, request.DueDate);
        ValidateInvoiceStatus(request.Status);
        ValidateInvoiceItems(request.Items);

        var invoice = await _dbContext.Invoices
            .Include(x => x.InvoiceItems)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Invoice with id '{id}' was not found.");

        if (invoice.Payments.Any())
        {
            throw new BusinessRuleException("Invoices with recorded payments cannot be edited in this MVP.");
        }

        _dbContext.InvoiceItems.RemoveRange(invoice.InvoiceItems);
        invoice.InvoiceItems = request.Items.Select(MapToEntity).ToList();
        invoice.CustomerId = request.CustomerId;
        invoice.IssueDate = request.IssueDate;
        invoice.DueDate = request.DueDate;
        invoice.Status = request.Status;
        invoice.RecalculateTotals();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(invoice.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _dbContext.Invoices
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Invoice with id '{id}' was not found.");

        if (invoice.Payments.Any())
        {
            throw new BusinessRuleException("Invoices with recorded payments cannot be deleted in this MVP.");
        }

        _dbContext.Invoices.Remove(invoice);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateCustomerExistsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Customers.AnyAsync(x => x.Id == customerId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException($"Customer with id '{customerId}' was not found.");
        }
    }

    private static void ValidateInvoiceDates(DateTime issueDate, DateTime dueDate)
    {
        if (dueDate.Date < issueDate.Date)
        {
            throw new BusinessRuleException("Due date cannot be earlier than issue date.");
        }
    }

    private static void ValidateInvoiceStatus(InvoiceStatus status)
    {
        if (status == InvoiceStatus.Paid)
        {
            throw new BusinessRuleException("Use the payments endpoint to mark an invoice as paid.");
        }
    }

    private static void ValidateInvoiceItems(IEnumerable<InvoiceItemRequest> items)
    {
        if (!items.Any())
        {
            throw new BusinessRuleException("An invoice must contain at least one line item.");
        }
    }

    private static InvoiceItem MapToEntity(InvoiceItemRequest request)
    {
        var item = new InvoiceItem
        {
            ProductName = request.ProductName.Trim(),
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            TaxRate = request.TaxRate
        };

        item.Recalculate();
        return item;
    }

    private static InvoiceStatus ResolveDisplayStatus(InvoiceStatus storedStatus, DateTime dueDate, decimal totalPaid, decimal totalAmount)
    {
        if (totalPaid >= totalAmount && totalAmount > 0)
        {
            return InvoiceStatus.Paid;
        }

        if (storedStatus == InvoiceStatus.Draft)
        {
            return InvoiceStatus.Draft;
        }

        return dueDate.Date < DateTime.UtcNow.Date ? InvoiceStatus.Overdue : InvoiceStatus.Sent;
    }

    private static InvoiceResponse Map(Invoice invoice)
    {
        var totalPaid = invoice.Payments.Sum(x => x.PaidAmount);
        var displayStatus = ResolveDisplayStatus(invoice.Status, invoice.DueDate, totalPaid, invoice.TotalAmount);

        return new InvoiceResponse
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer?.Name ?? string.Empty,
            CustomerEmail = invoice.Customer?.Email,
            CustomerPhone = invoice.Customer?.Phone,
            CustomerAddress = invoice.Customer?.Address,
            CustomerTaxNumber = invoice.Customer?.TaxNumber,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Status = displayStatus,
            Subtotal = invoice.Subtotal,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            TotalPaid = totalPaid,
            BalanceDue = Math.Max(invoice.TotalAmount - totalPaid, 0),
            Items = invoice.InvoiceItems
                .OrderBy(x => x.ProductName)
                .Select(x => new InvoiceItemResponse
                {
                    Id = x.Id,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    TaxRate = x.TaxRate,
                    LineSubtotal = x.LineSubtotal,
                    LineTaxAmount = x.LineTaxAmount,
                    LineTotal = x.LineTotal
                })
                .ToList(),
            Payments = invoice.Payments
                .OrderByDescending(x => x.PaidDate)
                .Select(x => new PaymentResponse
                {
                    Id = x.Id,
                    InvoiceId = x.InvoiceId,
                    PaidAmount = x.PaidAmount,
                    PaidDate = x.PaidDate,
                    PaymentMethod = x.PaymentMethod,
                    ReferenceNumber = x.ReferenceNumber
                })
                .ToList()
        };
    }
}
