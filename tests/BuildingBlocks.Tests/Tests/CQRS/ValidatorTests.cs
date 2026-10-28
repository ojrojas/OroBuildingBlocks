using BuildingBlocks.CQRS.DependencyInjection;
using BuildingBlocks.CQRS.Validation;
using BuildingBlocks.Tests.Helpers;

namespace BuildingBlocks.Tests.CQRS;

public class ValidatorTests
{
    [Fact]
    public async Task ValidateAsync_returns_empty_when_no_rules()
    {
        var validator = new ConcreteValidator();

        var failures = await validator.ValidateAsync(new TestRequest(), CancellationToken.None);

        Assert.Empty(failures);
    }

    [Fact]
    public async Task ValidateAsync_returns_empty_when_rule_passes()
    {
        var validator = new ConcreteValidator();
        validator.AddRule(x => true, "Prop", "Error");

        var failures = await validator.ValidateAsync(new TestRequest(), CancellationToken.None);

        Assert.Empty(failures);
    }

    [Fact]
    public async Task ValidateAsync_returns_failure_when_rule_fails()
    {
        var validator = new ConcreteValidator();
        validator.AddRule(x => false, "Prop", "Error message");

        var failures = await validator.ValidateAsync(new TestRequest(), CancellationToken.None);

        Assert.Single(failures);
        Assert.Equal("Prop", failures.First().PropertyName);
        Assert.Equal("Error message", failures.First().ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_accumulates_multiple_failures()
    {
        var validator = new ConcreteValidator();
        validator.AddRule(x => false, "P1", "E1");
        validator.AddRule(x => false, "P2", "E2");
        validator.AddRule(x => true, "P3", "E3");

        var failures = await validator.ValidateAsync(new TestRequest(), CancellationToken.None);

        Assert.Equal(2, failures.Count);
    }

    private sealed class ConcreteValidator : Validator<TestRequest>
    {
        public void AddRule(Func<TestRequest, bool> isValid, string propertyName, string errorMessage)
            => RuleFor(isValid, propertyName, errorMessage);
    }
}

public class ValidationExceptionTests
{
    [Fact]
    public void Contains_failures()
    {
        var failures = new List<ValidationFailure>
        {
            new("P1", "E1"),
            new("P2", "E2")
        };

        var ex = new ValidationException(failures);

        Assert.Equal(2, ex.Failures.Count);
    }

    [Fact]
    public void Message_is_descriptive()
    {
        var ex = new ValidationException([]);

        Assert.Equal("One or more validation failures occurred.", ex.Message);
    }

    [Fact]
    public void Inherits_from_Exception()
    {
        var ex = new ValidationException([]);

        Assert.IsAssignableFrom<Exception>(ex);
    }
}

public class CqrsConfigurationTests
{
    [Fact]
    public void RegisterHandlersFromAssembly_returns_self()
    {
        var config = new CqrsConfiguration();

        var result = config.RegisterHandlersFromAssembly(typeof(CqrsConfigurationTests).Assembly);

        Assert.Same(config, result);
    }

    [Fact]
    public void RegisterHandlersFromAssemblyContaining_returns_self()
    {
        var config = new CqrsConfiguration();

        var result = config.RegisterHandlersFromAssemblyContaining<CqrsConfiguration>();

        Assert.Same(config, result);
    }

    [Fact]
    public void AddOpenBehavior_returns_self()
    {
        var config = new CqrsConfiguration();

        var result = config.AddOpenBehavior(typeof(BuildingBlocks.CQRS.Behaviors.LoggingBehavior<,>));

        Assert.Same(config, result);
    }

    [Fact]
    public void Can_register_multiple_assemblies()
    {
        var config = new CqrsConfiguration();

        config.RegisterHandlersFromAssembly(typeof(CqrsConfigurationTests).Assembly);
        config.RegisterHandlersFromAssembly(typeof(CqrsConfiguration).Assembly);

        // Verify fluent chaining doesn't throw
        var result = config.AddOpenBehavior(typeof(BuildingBlocks.CQRS.Behaviors.LoggingBehavior<,>));
        Assert.Same(config, result);
    }
}
