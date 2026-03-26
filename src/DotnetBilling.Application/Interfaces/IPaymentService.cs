using DotnetBilling.Application.DTOs.Payments;

namespace DotnetBilling.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
}
