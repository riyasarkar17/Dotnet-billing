namespace DotnetBilling.Application.Interfaces;

public interface IInvoicePdfService
{
    Task<byte[]> GenerateAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
