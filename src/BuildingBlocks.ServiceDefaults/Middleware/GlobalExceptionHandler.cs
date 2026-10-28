using System.Diagnostics;

namespace BuildingBlocks.ServiceDefaults.Middleware;

/// <summary>
/// Translates unhandled exceptions to RFC 7807 ProblemDetails:
/// validation failures → 400 with per-field errors, domain rule violations → 422,
/// anything else → 500 without leaking internals.
/// Register with services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;() + app.UseExceptionHandler().
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            UnauthorizedAccessException => new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden"
            },
            ValidationException validationException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Extensions =
                {
                    ["errors"] = validationException.Failures
                        .GroupBy(failure => failure.PropertyName)
                        .ToDictionary(group => group.Key, group => group.Select(f => f.ErrorMessage).ToArray())
                }
            },
            DomainException domainException => new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Domain rule violated",
                Detail = domainException.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred"
            }
        };

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            // Exception messages, inner exceptions and paths may contain request data or secrets.
            // Keep the log useful for correlation without serializing exception details.
            logger.LogError(
                "Unhandled exception type {ExceptionType}; trace {TraceId}",
                exception.GetType().Name,
                Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
