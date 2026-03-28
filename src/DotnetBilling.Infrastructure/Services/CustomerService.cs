using DotnetBilling.Application.DTOs.Customers;
using DotnetBilling.Application.Exceptions;
using DotnetBilling.Application.Interfaces;
using DotnetBilling.Domain.Entities;
using DotnetBilling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetBilling.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _dbContext;

    public CustomerService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Customer with id '{id}' was not found.");

        return Map(customer);
    }

    public async Task<CustomerResponse> CreateAsync(CustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            Name = request.Name.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            TaxNumber = request.TaxNumber?.Trim()
        };

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, CustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                       ?? throw new NotFoundException($"Customer with id '{id}' was not found.");

        customer.Name = request.Name.Trim();
        customer.Email = request.Email?.Trim();
        customer.Phone = request.Phone?.Trim();
        customer.Address = request.Address?.Trim();
        customer.TaxNumber = request.TaxNumber?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .Include(x => x.Invoices)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Customer with id '{id}' was not found.");

        if (customer.Invoices.Any())
        {
            throw new BusinessRuleException("Cannot delete a customer that already has invoices.");
        }

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static CustomerResponse Map(Customer customer) => new()
    {
        Id = customer.Id,
        Name = customer.Name,
        Email = customer.Email,
        Phone = customer.Phone,
        Address = customer.Address,
        TaxNumber = customer.TaxNumber
    };
}
