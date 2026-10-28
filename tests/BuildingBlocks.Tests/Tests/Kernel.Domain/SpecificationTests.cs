using BuildingBlocks.Tests.Helpers;
using BuildingBlocks.Kernel.Domain.Specifications;

namespace BuildingBlocks.Tests.Kernel.Domain;

public class SpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_returns_true_when_no_criteria()
    {
        var spec = new EmptySpecification<TestOrder>();
        var order = new TestOrder { Name = "Test", Price = 10 };

        Assert.True(spec.IsSatisfiedBy(order));
    }

    [Fact]
    public void IsSatisfiedBy_returns_true_when_criteria_matches()
    {
        var spec = new ActiveOrdersSpecification();
        var order = new TestOrder { IsActive = true };

        Assert.True(spec.IsSatisfiedBy(order));
    }

    [Fact]
    public void IsSatisfiedBy_returns_false_when_criteria_fails()
    {
        var spec = new ActiveOrdersSpecification();
        var order = new TestOrder { IsActive = false };

        Assert.False(spec.IsSatisfiedBy(order));
    }

    [Fact]
    public void Where_combines_criteria_with_and()
    {
        var spec = new ActiveAndExpensiveSpecification(5m);

        var activeExpensive = new TestOrder { IsActive = true, Price = 10 };
        var activeCheap = new TestOrder { IsActive = true, Price = 3 };
        var inactiveExpensive = new TestOrder { IsActive = false, Price = 10 };

        Assert.True(spec.IsSatisfiedBy(activeExpensive));
        Assert.False(spec.IsSatisfiedBy(activeCheap));
        Assert.False(spec.IsSatisfiedBy(inactiveExpensive));
    }

    [Fact]
    public void And_combines_two_specifications()
    {
        var activeSpec = new ActiveOrdersSpecification();
        var cheapSpec = new CheapOrdersSpecification(50);

        var combined = activeSpec.And(cheapSpec);

        var activeCheap = new TestOrder { IsActive = true, Price = 30 };
        var activeExpensive = new TestOrder { IsActive = true, Price = 100 };
        var inactiveCheap = new TestOrder { IsActive = false, Price = 30 };

        Assert.True(combined.IsSatisfiedBy(activeCheap));
        Assert.False(combined.IsSatisfiedBy(activeExpensive));
        Assert.False(combined.IsSatisfiedBy(inactiveCheap));
    }

    [Fact]
    public void Or_combines_two_specifications()
    {
        var activeSpec = new ActiveOrdersSpecification();
        var cheapSpec = new CheapOrdersSpecification(50);

        var combined = activeSpec.Or(cheapSpec);

        var activeExpensive = new TestOrder { IsActive = true, Price = 100 };
        var inactiveCheap = new TestOrder { IsActive = false, Price = 30 };
        var inactiveExpensive = new TestOrder { IsActive = false, Price = 100 };

        Assert.True(combined.IsSatisfiedBy(activeExpensive));
        Assert.True(combined.IsSatisfiedBy(inactiveCheap));
        Assert.False(combined.IsSatisfiedBy(inactiveExpensive));
    }

    [Fact]
    public void Not_inverts_criteria()
    {
        var spec = new ActiveOrdersSpecification();
        var inverted = spec.Not();

        var active = new TestOrder { IsActive = true };
        var inactive = new TestOrder { IsActive = false };

        Assert.False(inverted.IsSatisfiedBy(active));
        Assert.True(inverted.IsSatisfiedBy(inactive));
    }

    [Fact]
    public void Not_without_criteria_returns_false_for_all()
    {
        var spec = new EmptySpecification<TestOrder>();
        var inverted = spec.Not();

        var order = new TestOrder { IsActive = true };

        Assert.False(inverted.IsSatisfiedBy(order));
    }

    [Fact]
    public void Includes_list_is_empty_by_default()
    {
        var spec = new EmptySpecification<TestOrder>();

        Assert.Empty(spec.Includes);
    }

    [Fact]
    public void OrderBy_is_null_by_default()
    {
        var spec = new EmptySpecification<TestOrder>();

        Assert.Null(spec.OrderBy);
    }

    [Fact]
    public void OrderByDescending_is_null_by_default()
    {
        var spec = new EmptySpecification<TestOrder>();

        Assert.Null(spec.OrderByDescending);
    }

    [Fact]
    public void Skip_is_null_by_default()
    {
        var spec = new EmptySpecification<TestOrder>();

        Assert.Null(spec.Skip);
    }

    [Fact]
    public void Take_is_null_by_default()
    {
        var spec = new EmptySpecification<TestOrder>();

        Assert.Null(spec.Take);
    }

    [Fact]
    public void AsNoTracking_is_false_by_default()
    {
        var spec = new EmptySpecification<TestOrder>();

        Assert.False(spec.AsNoTracking);
    }

    private sealed class ActiveAndExpensiveSpecification : Specification<TestOrder>
    {
        public ActiveAndExpensiveSpecification(decimal minPrice)
        {
            Where(o => o.IsActive);
            Where(o => o.Price > minPrice);
        }
    }
}
