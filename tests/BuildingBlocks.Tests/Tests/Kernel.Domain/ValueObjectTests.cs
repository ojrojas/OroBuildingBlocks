using BuildingBlocks.Tests.Helpers;

namespace BuildingBlocks.Tests.Kernel.Domain;

public class ValueObjectTests
{
    [Fact]
    public void Equals_same_components_returns_true()
    {
        var a = new TestValueObject("John", 30);
        var b = new TestValueObject("John", 30);

        Assert.Equal(a, b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_different_components_returns_false()
    {
        var a = new TestValueObject("John", 30);
        var b = new TestValueObject("Jane", 25);

        Assert.NotEqual(a, b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_null_returns_false()
    {
        var vo = new TestValueObject("John", 30);

        Assert.False(vo.Equals(null));
    }

    [Fact]
    public void Equals_different_type_returns_false()
    {
        var vo = new TestValueObject("John", 30);

        Assert.False(vo.Equals("not a value object"));
    }

    [Fact]
    public void GetHashCode_same_components_same_hash()
    {
        var a = new TestValueObject("John", 30);
        var b = new TestValueObject("John", 30);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_different_components_different_hash()
    {
        var a = new TestValueObject("John", 30);
        var b = new TestValueObject("Jane", 25);

        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void OperatorEquality_same_returns_true()
    {
        var a = new TestValueObject("John", 30);
        var b = new TestValueObject("John", 30);

        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void OperatorEquality_different_returns_false()
    {
        var a = new TestValueObject("John", 30);
        var b = new TestValueObject("Jane", 25);

        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void OperatorEquality_null_left()
    {
        TestValueObject? a = null;
        var b = new TestValueObject("John", 30);

        Assert.True(a == null);
        Assert.False(a != null);
    }

    [Fact]
    public void OperatorEquality_both_null()
    {
        TestValueObject? a = null;
        TestValueObject? b = null;

        Assert.True(a == b);
        Assert.False(a != b);
    }
}
