using DotnetBilling.Application.Interfaces;
using DotnetBilling.Domain.Entities;
using DotnetBilling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetBilling.Infrastructure.Services;

public class DataSeeder : IDataSeeder
{
    private readonly AppDbContext _dbContext;

    public DataSeeder(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var hasMigrations = (await _dbContext.Database.GetMigrationsAsync(cancellationToken)).Any();
        if (hasMigrations)
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (!await _dbContext.Customers.AnyAsync(cancellationToken))
        {
            _dbContext.Customers.AddRange(
                new Customer
                {
                    Name = "Wayne Enterprises",
                    Email = "billing@wayne.com",
                    Phone = "+1-555-0101",
                    Address = "1007 Mountain Drive, Gotham",
                    TaxNumber = "GSTIN-001"
                },
                new Customer
                {
                    Name = "Stark Industries",
                    Email = "accounts@starkindustries.com",
                    Phone = "+1-555-0102",
                    Address = "200 Park Ave, New York",
                    TaxNumber = "GSTIN-002"
                });
        }

        if (!await _dbContext.Products.AnyAsync(cancellationToken))
        {
            _dbContext.Products.AddRange(
                new Product
                {
                    Name = "Website Maintenance",
                    Description = "Monthly support retainer",
                    UnitPrice = 2500m,
                    TaxRate = 18m
                },
                new Product
                {
                    Name = "SEO Audit",
                    Description = "One-time SEO and performance review",
                    UnitPrice = 1800m,
                    TaxRate = 18m
                },
                new Product
                {
                    Name = "UI Design Sprint",
                    Description = "Five-day design workshop",
                    UnitPrice = 4500m,
                    TaxRate = 18m
                });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
