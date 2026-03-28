using DotnetBilling.Application.DTOs.Products;
using DotnetBilling.Application.Exceptions;
using DotnetBilling.Application.Interfaces;
using DotnetBilling.Domain.Entities;
using DotnetBilling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetBilling.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;

    public ProductService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Product with id '{id}' was not found.");

        return Map(product);
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            UnitPrice = request.UnitPrice,
            TaxRate = request.TaxRate
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, ProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{id}' was not found.");

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.UnitPrice = request.UnitPrice;
        product.TaxRate = request.TaxRate;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{id}' was not found.");

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ProductResponse Map(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        UnitPrice = product.UnitPrice,
        TaxRate = product.TaxRate
    };
}
