using DotnetBilling.Application.DTOs.Payments;
using DotnetBilling.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DotnetBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Create([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.CreateAsync(request, cancellationToken);
        return Ok(payment);
    }
}
