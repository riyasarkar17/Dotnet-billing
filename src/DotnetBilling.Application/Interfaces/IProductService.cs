using DotnetBilling.Application.DTOs.Products;

namespace DotnetBilling.Application.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateAsync(Guid id, ProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
