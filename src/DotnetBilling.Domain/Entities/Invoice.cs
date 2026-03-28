using DotnetBilling.Domain.Enums;

namespace DotnetBilling.Domain.Entities;

public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public Customer? Customer { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public void RecalculateTotals()
    {
        foreach (var item in InvoiceItems)
        {
            item.Recalculate();
        }

        Subtotal = Math.Round(InvoiceItems.Sum(x => x.LineSubtotal), 2, MidpointRounding.AwayFromZero);
        TaxAmount = Math.Round(InvoiceItems.Sum(x => x.LineTaxAmount), 2, MidpointRounding.AwayFromZero);
        TotalAmount = Math.Round(InvoiceItems.Sum(x => x.LineTotal), 2, MidpointRounding.AwayFromZero);
    }
}
