using BuildingBlocks.Kernel.Domain.Results;

namespace BuildingBlocks.Tests.Kernel.Domain;

public class ErrorTests
{
    [Fact]
    public void None_has_empty_code_and_description()
    {
        var none = Error.None;

        Assert.Equal(string.Empty, none.Code);
        Assert.Equal(string.Empty, none.Description);
        Assert.Equal(ErrorType.Failure, none.Type);
    }

    [Fact]
    public void NullValue_has_correct_values()
    {
        var nullValue = Error.NullValue;

        Assert.Equal("General.NullValue", nullValue.Code);
        Assert.Equal("A null value was provided.", nullValue.Description);
        Assert.Equal(ErrorType.Failure, nullValue.Type);
    }

    [Fact]
    public void Failure_factory_creates_failure_error()
    {
        var error = Error.Failure("code", "desc");

        Assert.Equal("code", error.Code);
        Assert.Equal("desc", error.Description);
        Assert.Equal(ErrorType.Failure, error.Type);
    }

    [Fact]
    public void Validation_factory_creates_validation_error()
    {
        var error = Error.Validation("code", "desc");

        Assert.Equal(ErrorType.Validation, error.Type);
    }

    [Fact]
    public void NotFound_factory_creates_not_found_error()
    {
        var error = Error.NotFound("code", "desc");

        Assert.Equal(ErrorType.NotFound, error.Type);
    }

    [Fact]
    public void Conflict_factory_creates_conflict_error()
    {
        var error = Error.Conflict("code", "desc");

        Assert.Equal(ErrorType.Conflict, error.Type);
    }

    [Fact]
    public void Unauthorized_factory_creates_unauthorized_error()
    {
        var error = Error.Unauthorized("code", "desc");

        Assert.Equal(ErrorType.Unauthorized, error.Type);
    }

    [Fact]
    public void Forbidden_factory_creates_forbidden_error()
    {
        var error = Error.Forbidden("code", "desc");

        Assert.Equal(ErrorType.Forbidden, error.Type);
    }

    [Fact]
    public void Equality_same_values_returns_true()
    {
        var a = new Error("code", "desc", ErrorType.Failure);
        var b = new Error("code", "desc", ErrorType.Failure);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_different_values_returns_false()
    {
        var a = new Error("code1", "desc", ErrorType.Failure);
        var b = new Error("code2", "desc", ErrorType.Failure);

        Assert.NotEqual(a, b);
    }
}

public class ResultTests
{
    [Fact]
    public void Success_returns_is_success_true()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_returns_is_success_false()
    {
        var error = Error.Failure("code", "desc");
        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Success_with_value()
    {
        var result = Result.Success("hello");

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Failure_with_value_throws_on_access()
    {
        var result = Result.Failure<string>(Error.Failure("code", "desc"));

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Implicit_conversion_from_value()
    {
        Result<string> result = "hello";

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Implicit_conversion_from_error()
    {
        Result<string> result = Error.Failure("code", "desc");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Match_on_success()
    {
        var result = Result.Success(42);

        var output = result.Match(
            val => val * 2,
            _ => 0);

        Assert.Equal(84, output);
    }

    [Fact]
    public void Match_on_failure()
    {
        var result = Result.Failure<int>(Error.Failure("code", "desc"));

        var output = result.Match(
            val => val,
            _ => -1);

        Assert.Equal(-1, output);
    }

    [Fact]
    public void Success_with_error_throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Result.Failure(Error.None));
    }

    [Fact]
    public void Success_result_with_value_is_success()
    {
        var result = Result.Success<int>(0);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void Failure_without_error_throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Result.Failure(Error.None));
    }
}
