using System.ComponentModel.DataAnnotations;
using DotnetBilling.Application.DTOs.Payments;
using DotnetBilling.Domain.Enums;

namespace DotnetBilling.Application.DTOs.Invoices;

public class InvoiceItemRequest
{
    [Required]
    [StringLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999")]
    public decimal Quantity { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal UnitPrice { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal TaxRate { get; set; }
}

public class CreateInvoiceRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public DateTime IssueDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    [MinLength(1)]
    public List<InvoiceItemRequest> Items { get; set; } = new();
}

public class UpdateInvoiceRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public DateTime IssueDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    [MinLength(1)]
    public List<InvoiceItemRequest> Items { get; set; } = new();
}

public class InvoiceItemResponse
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineSubtotal { get; set; }
    public decimal LineTaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public class InvoiceListItemResponse
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal BalanceDue { get; set; }
}

public class InvoiceResponse
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerTaxNumber { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public List<InvoiceItemResponse> Items { get; set; } = new();
    public List<PaymentResponse> Payments { get; set; } = new();
}
