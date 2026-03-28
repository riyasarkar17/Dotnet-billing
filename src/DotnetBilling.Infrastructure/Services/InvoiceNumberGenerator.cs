using DotnetBilling.Application.Interfaces;
using DotnetBilling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetBilling.Infrastructure.Services;

public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly AppDbContext _dbContext;

    public InvoiceNumberGenerator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        var latestInvoiceNumber = await _dbContext.Invoices
            .AsNoTracking()
            .Where(x => x.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(x => x.InvoiceNumber)
            .Select(x => x.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var nextSequence = 1;
        if (!string.IsNullOrWhiteSpace(latestInvoiceNumber) && latestInvoiceNumber.Length > prefix.Length)
        {
            var suffix = latestInvoiceNumber[prefix.Length..];
            if (int.TryParse(suffix, out var parsed))
            {
                nextSequence = parsed + 1;
            }
        }

        return $"{prefix}{nextSequence:0000}";
    }
}
