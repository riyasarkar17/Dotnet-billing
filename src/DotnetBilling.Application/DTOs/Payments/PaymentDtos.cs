using System.ComponentModel.DataAnnotations;

namespace DotnetBilling.Application.DTOs.Payments;

public class CreatePaymentRequest
{
    [Required]
    public Guid InvoiceId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal PaidAmount { get; set; }

    public DateTime? PaidDate { get; set; }

    [Required]
    [StringLength(100)]
    public string PaymentMethod { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ReferenceNumber { get; set; }
}

public class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime PaidDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
}
