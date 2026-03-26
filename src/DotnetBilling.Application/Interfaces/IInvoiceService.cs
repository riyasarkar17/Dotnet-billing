using DotnetBilling.Application.DTOs.Invoices;
using DotnetBilling.Domain.Enums;

namespace DotnetBilling.Application.Interfaces;

public interface IInvoiceService
{
    Task<IReadOnlyList<InvoiceListItemResponse>> GetAllAsync(string? search, InvoiceStatus? status, Guid? customerId, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> UpdateAsync(Guid id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
