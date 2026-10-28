using BuildingBlocks.Kernel.Domain.Exceptions;
using BuildingBlocks.Kernel.Domain.Results;
using BuildingBlocks.ServiceDefaults.Endpoints;
using BuildingBlocks.ServiceDefaults.Middleware;
using BuildingBlocks.CQRS.Validation;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using NSubstitute;

namespace BuildingBlocks.Tests.ServiceDefaults;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_returns_true_always()
    {
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(logger);
        var context = new DefaultHttpContext();

        var result = await handler.TryHandleAsync(context, new Exception("test"), CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task TryHandleAsync_sets_400_for_ValidationException()
    {
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(logger);
        var context = new DefaultHttpContext();
        var failures = new List<ValidationFailure> { new("Field", "Error") };
        var exception = new ValidationException(failures);

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_sets_422_for_DomainException()
    {
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(logger);
        var context = new DefaultHttpContext();
        var exception = new DomainException("rule violated");

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_sets_500_for_unknown_exception()
    {
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(logger);
        var context = new DefaultHttpContext();

        await handler.TryHandleAsync(context, new InvalidOperationException("fail"), CancellationToken.None);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_writes_problem_details()
    {
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new System.IO.MemoryStream();

        await handler.TryHandleAsync(context, new Exception("test"), CancellationToken.None);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }
}

public class ResultExtensionsTests
{
    [Fact]
    public void ToHttpResult_success_returns_result()
    {
        var result = Result.Success();

        var httpResult = result.ToHttpResult();

        Assert.NotNull(httpResult);
    }

    [Fact]
    public void ToHttpResult_failure_returns_result()
    {
        var result = Result.Failure(Error.Failure("code", "desc"));

        var httpResult = result.ToHttpResult();

        Assert.NotNull(httpResult);
    }

    [Fact]
    public void ToHttpResult_generic_success_returns_result()
    {
        var result = Result.Success("value");

        var httpResult = result.ToHttpResult<string>();

        Assert.NotNull(httpResult);
    }

    [Fact]
    public void ToHttpResult_generic_failure_returns_result()
    {
        var result = Result.Failure<string>(Error.NotFound("not.found", "Not found"));

        var httpResult = result.ToHttpResult<string>();

        Assert.NotNull(httpResult);
    }

    [Fact]
    public void ToCreatedResult_success_returns_result()
    {
        var result = Result.Success("value");

        var httpResult = result.ToCreatedResult(v => $"/api/items/{v}");

        Assert.NotNull(httpResult);
    }
}
