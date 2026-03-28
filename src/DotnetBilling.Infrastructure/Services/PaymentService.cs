using DotnetBilling.Application.DTOs.Payments;
using DotnetBilling.Application.Exceptions;
using DotnetBilling.Application.Interfaces;
using DotnetBilling.Domain.Entities;
using DotnetBilling.Domain.Enums;
using DotnetBilling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetBilling.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _dbContext;

    public PaymentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await _dbContext.Invoices
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException($"Invoice with id '{request.InvoiceId}' was not found.");

        var totalPaid = invoice.Payments.Sum(x => x.PaidAmount);
        var balanceDue = Math.Max(invoice.TotalAmount - totalPaid, 0);

        if (balanceDue <= 0)
        {
            throw new BusinessRuleException("This invoice is already fully paid.");
        }

        if (request.PaidAmount > balanceDue)
        {
            throw new BusinessRuleException("Payment amount cannot exceed the remaining balance in this MVP.");
        }

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            PaidAmount = request.PaidAmount,
            PaidDate = request.PaidDate ?? DateTime.UtcNow,
            PaymentMethod = request.PaymentMethod.Trim(),
            ReferenceNumber = request.ReferenceNumber?.Trim()
        };

        _dbContext.Payments.Add(payment);

        var projectedTotalPaid = totalPaid + payment.PaidAmount;
        if (projectedTotalPaid >= invoice.TotalAmount)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.Status != InvoiceStatus.Draft)
        {
            invoice.Status = invoice.DueDate.Date < DateTime.UtcNow.Date ? InvoiceStatus.Overdue : InvoiceStatus.Sent;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PaymentResponse
        {
            Id = payment.Id,
            InvoiceId = payment.InvoiceId,
            PaidAmount = payment.PaidAmount,
            PaidDate = payment.PaidDate,
            PaymentMethod = payment.PaymentMethod,
            ReferenceNumber = payment.ReferenceNumber
        };
    }
}
