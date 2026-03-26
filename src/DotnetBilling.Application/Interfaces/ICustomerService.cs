using DotnetBilling.Application.DTOs.Customers;

namespace DotnetBilling.Application.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerResponse> CreateAsync(CustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerResponse> UpdateAsync(Guid id, CustomerRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
