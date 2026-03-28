using DotnetBilling.Application.Exceptions;
using DotnetBilling.Application.Interfaces;
using DotnetBilling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DotnetBilling.Infrastructure.Services;

public class InvoicePdfService : IInvoicePdfService
{
    private readonly AppDbContext _dbContext;

    public InvoicePdfService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<byte[]> GenerateAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.InvoiceItems)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken)
            ?? throw new NotFoundException($"Invoice with id '{invoiceId}' was not found.");

        var totalPaid = invoice.Payments.Sum(x => x.PaidAmount);
        var balanceDue = Math.Max(invoice.TotalAmount - totalPaid, 0);
        const string brand = "#1D4ED8";
        const string light = "#F3F4F6";
        const string dark = "#111827";
        const string muted = "#6B7280";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(dark));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("INVOICE")
                            .FontSize(24)
                            .SemiBold()
                            .FontColor(brand);
                        col.Item().Text($"Invoice No: {invoice.InvoiceNumber}").SemiBold();
                        col.Item().Text($"Issue Date: {invoice.IssueDate:dd MMM yyyy}");
                        col.Item().Text($"Due Date: {invoice.DueDate:dd MMM yyyy}");
                    });

                    row.ConstantItem(200).AlignRight().Column(col =>
                    {
                        col.Item().Text("DotnetBilling")
                            .FontSize(16)
                            .SemiBold();
                        col.Item().Text("Small business billing API").FontColor(muted);
                        col.Item().Text("support@dotnetbilling.local").FontColor(muted);
                    });
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    column.Spacing(18);

                    column.Item().Border(1).BorderColor(light).Padding(14).Column(col =>
                    {
                        col.Item().Text("Bill To")
                            .SemiBold()
                            .FontColor(brand);
                        col.Item().Text(invoice.Customer?.Name ?? "");
                        if (!string.IsNullOrWhiteSpace(invoice.Customer?.Email))
                            col.Item().Text(invoice.Customer.Email);
                        if (!string.IsNullOrWhiteSpace(invoice.Customer?.Phone))
                            col.Item().Text(invoice.Customer.Phone);
                        if (!string.IsNullOrWhiteSpace(invoice.Customer?.Address))
                            col.Item().Text(invoice.Customer.Address);
                        if (!string.IsNullOrWhiteSpace(invoice.Customer?.TaxNumber))
                            col.Item().Text($"Tax No: {invoice.Customer.TaxNumber}");
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            IContainer HeaderCell(IContainer container) => container
                                .Background(brand)
                                .PaddingVertical(8)
                                .PaddingHorizontal(10);

                            header.Cell().Element(HeaderCell).Text("Item").FontColor(Colors.White).SemiBold();
                            header.Cell().Element(HeaderCell).AlignRight().Text("Qty").FontColor(Colors.White).SemiBold();
                            header.Cell().Element(HeaderCell).AlignRight().Text("Price").FontColor(Colors.White).SemiBold();
                            header.Cell().Element(HeaderCell).AlignRight().Text("Tax").FontColor(Colors.White).SemiBold();
                            header.Cell().Element(HeaderCell).AlignRight().Text("Total").FontColor(Colors.White).SemiBold();
                        });

                        foreach (var item in invoice.InvoiceItems.OrderBy(x => x.ProductName))
                        {
                            IContainer BodyCell(IContainer container) => container
                                .BorderBottom(1)
                                .BorderColor(light)
                                .PaddingVertical(8)
                                .PaddingHorizontal(10);

                            table.Cell().Element(BodyCell).Text(item.ProductName);
                            table.Cell().Element(BodyCell).AlignRight().Text(item.Quantity.ToString("0.##"));
                            table.Cell().Element(BodyCell).AlignRight().Text(item.UnitPrice.ToString("0.00"));
                            table.Cell().Element(BodyCell).AlignRight().Text($"{item.TaxRate:0.##}%");
                            table.Cell().Element(BodyCell).AlignRight().Text(item.LineTotal.ToString("0.00"));
                        }
                    });

                    column.Item().AlignRight().Width(220).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Subtotal");
                            row.ConstantItem(90).AlignRight().Text(invoice.Subtotal.ToString("0.00"));
                        });
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Tax");
                            row.ConstantItem(90).AlignRight().Text(invoice.TaxAmount.ToString("0.00"));
                        });
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Paid");
                            row.ConstantItem(90).AlignRight().Text(totalPaid.ToString("0.00"));
                        });
                        col.Item().PaddingVertical(6).LineHorizontal(1).LineColor(light);
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Balance Due").SemiBold();
                            row.ConstantItem(90).AlignRight().Text(balanceDue.ToString("0.00")).SemiBold();
                        });
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Grand Total").SemiBold().FontColor(brand);
                            row.ConstantItem(90).AlignRight().Text(invoice.TotalAmount.ToString("0.00")).SemiBold().FontColor(brand);
                        });
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated by DotnetBilling • ").FontColor(muted);
                    text.Span(DateTime.UtcNow.ToString("dd MMM yyyy HH:mm 'UTC'"))
                        .FontColor(muted);
                });
            });
        }).GeneratePdf();
    }
}
