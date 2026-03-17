using DotnetBilling.Application.DTOs.Invoices;
using DotnetBilling.Application.Interfaces;
using DotnetBilling.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DotnetBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly IInvoicePdfService _invoicePdfService;

    public InvoicesController(IInvoiceService invoiceService, IInvoicePdfService invoicePdfService)
    {
        _invoiceService = invoiceService;
        _invoicePdfService = invoicePdfService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceListItemResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] InvoiceStatus? status,
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        var invoices = await _invoiceService.GetAllAsync(search, status, customerId, cancellationToken);
        return Ok(invoices);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceService.GetByIdAsync(id, cancellationToken);
        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceResponse>> Create([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InvoiceResponse>> Update(Guid id, [FromBody] UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceService.UpdateAsync(id, request, cancellationToken);
        return Ok(invoice);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _invoiceService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken cancellationToken)
    {
        var pdfBytes = await _invoicePdfService.GenerateAsync(id, cancellationToken);
        return File(pdfBytes, "application/pdf", $"invoice-{id}.pdf");
    }
}
