namespace DotnetBilling.Domain.Entities;

public class InvoiceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineSubtotal { get; set; }
    public decimal LineTaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    public Invoice? Invoice { get; set; }

    public void Recalculate()
    {
        LineSubtotal = Math.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);
        LineTaxAmount = Math.Round(LineSubtotal * (TaxRate / 100m), 2, MidpointRounding.AwayFromZero);
        LineTotal = Math.Round(LineSubtotal + LineTaxAmount, 2, MidpointRounding.AwayFromZero);
    }
}
