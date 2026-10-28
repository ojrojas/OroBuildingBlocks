using BuildingBlocks.Tests.Helpers;
using BuildingBlocks.Kernel.Domain.Entities;
using BuildingBlocks.Kernel.Domain.Rules;

namespace BuildingBlocks.Tests.Kernel.Domain;

public class EntityTests
{
    [Fact]
    public void Equals_same_id_returns_true()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);

        Assert.Equal(a, b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_different_id_returns_false()
    {
        var a = new TestEntity(Guid.NewGuid());
        var b = new TestEntity(Guid.NewGuid());

        Assert.NotEqual(a, b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_null_returns_false()
    {
        var entity = new TestEntity(Guid.NewGuid());

        Assert.False(entity.Equals(null));
    }

    [Fact]
    public void Equals_different_type_returns_false()
    {
        var entity = new TestEntity(Guid.NewGuid());
        var other = new OtherEntity(Guid.NewGuid());

        Assert.False(entity.Equals(other));
    }

    [Fact]
    public void Equals_reference_same_returns_true()
    {
        var entity = new TestEntity(Guid.NewGuid());

        Assert.True(entity.Equals(entity));
    }

    [Fact]
    public void GetHashCode_same_id_same_hash()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_different_id_different_hash()
    {
        var a = new TestEntity(Guid.NewGuid());
        var b = new TestEntity(Guid.NewGuid());

        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void OperatorEquality_same_id_returns_true()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);

        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void OperatorEquality_different_id_returns_false()
    {
        var a = new TestEntity(Guid.NewGuid());
        var b = new TestEntity(Guid.NewGuid());

        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void OperatorEquality_null_left_returns_false()
    {
        var b = new TestEntity(Guid.NewGuid());

        Assert.False(null == b);
        Assert.True(null != b);
    }

    [Fact]
    public void OperatorEquality_null_right_returns_false()
    {
        var a = new TestEntity(Guid.NewGuid());

        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void OperatorEquality_both_null_returns_true()
    {
        TestEntity? a = null;
        TestEntity? b = null;

        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Id_is_set_correctly()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void CheckRule_does_not_throw_when_rule_not_broken()
    {
        var rule = new NeverBrokenRule();

        TestEntity.ExposeCheckRule(rule);
    }

    [Fact]
    public void CheckRule_throws_when_rule_is_broken()
    {
        var rule = new AlwaysBrokenRule();

        var ex = Assert.Throws<BusinessRuleValidationException>(() => TestEntity.ExposeCheckRule(rule));
        Assert.Equal(rule, ex.BrokenRule);
        Assert.Equal("Always broken", ex.Message);
    }

    private sealed class OtherEntity(Guid id) : Entity<Guid>(id);
}
