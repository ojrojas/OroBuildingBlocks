// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServicesDefaults;

/// <summary>
/// Global exception handler that converts unhandled exceptions to structured Problem Details responses.
/// <see cref="ApplicationException"/> maps to 400 Bad Request; all others map to 500 Internal Server Error.
/// </summary>
public sealed class ApiExceptionsHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = exception switch
        {
            ApplicationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        Activity? activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = exception.GetType().Name,
                Title = "An error occurred while processing your request.",
                Detail = exception.Message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = activity?.Id,
                    ["requestId"] = httpContext.TraceIdentifier,
                }
            }
        });
    }
}
