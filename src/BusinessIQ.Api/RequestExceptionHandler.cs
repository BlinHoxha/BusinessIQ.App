using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Framework.Domain.Exceptions;

namespace BusinessIQ.Api;

internal sealed class RequestExceptionHandler(IProblemDetailsService problems, ILogger<RequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ArgumentException => (400, "Invalid request"),
            KeyNotFoundException => (404, "Resource not found"),
            DomainException domain => (domain.StatusCode, "Request rejected"),
            _ => (500, "Server error")
        };
        if (status >= 500) logger.LogError(exception, "BusinessIQ request failed");
        context.Response.StatusCode = status;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = title,
                Detail = status < 500 ? exception.Message : "An unexpected error occurred."
            }
        });
    }
}
