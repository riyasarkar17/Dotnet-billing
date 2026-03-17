using System.Net;
using System.Text.Json;
using DotnetBilling.Application.Exceptions;

namespace DotnetBilling.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            BusinessRuleException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        if ((int)statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception while processing request.");
        }
        else
        {
            _logger.LogWarning(exception, "Request failed with a handled exception.");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            message = exception.Message,
            statusCode = context.Response.StatusCode
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
