using System.ComponentModel.DataAnnotations;

namespace DotnetBilling.Application.DTOs.Customers;

public class CustomerRequest
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? TaxNumber { get; set; }
}

public class CustomerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
}
