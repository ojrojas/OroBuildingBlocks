using BuildingBlocks.Tests.Helpers;

namespace BuildingBlocks.Tests.Kernel.Domain;

public class EnumerationTests
{
    [Fact]
    public void GetAll_returns_all_static_values()
    {
        var all = TestEnumeration.GetAll();

        Assert.Equal(3, all.Count);
    }

    [Fact]
    public void GetAll_contains_known_values()
    {
        var all = TestEnumeration.GetAll();

        Assert.Contains(all, e => e.Id == 1 && e.Name == "One");
        Assert.Contains(all, e => e.Id == 2 && e.Name == "Two");
        Assert.Contains(all, e => e.Id == 3 && e.Name == "Three");
    }

    [Fact]
    public void FromId_returns_correct_value()
    {
        var result = TestEnumeration.FromId(2);

        Assert.Equal(2, result.Id);
        Assert.Equal("Two", result.Name);
    }

    [Fact]
    public void FromId_throws_for_invalid_id()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestEnumeration.FromId(999));
    }

    [Fact]
    public void FromName_returns_correct_value()
    {
        var result = TestEnumeration.FromName("Three");

        Assert.Equal(3, result.Id);
        Assert.Equal("Three", result.Name);
    }

    [Fact]
    public void FromName_is_case_insensitive()
    {
        var result = TestEnumeration.FromName("one");

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void FromName_throws_for_invalid_name()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestEnumeration.FromName("NonExistent"));
    }

    [Fact]
    public void CompareTo_null_returns_1()
    {
        var value = TestEnumeration.One;

        Assert.Equal(1, value.CompareTo(null));
    }

    [Fact]
    public void CompareTo_other_returns_comparison()
    {
        var one = TestEnumeration.One;
        var two = TestEnumeration.Two;

        Assert.True(one.CompareTo(two) < 0);
        Assert.True(two.CompareTo(one) > 0);
    }

    [Fact]
    public void Equals_same_type_same_id_returns_true()
    {
        var a = TestEnumeration.FromId(1);
        var b = TestEnumeration.FromId(1);

        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_different_type_returns_false()
    {
        var a = TestEnumeration.One;

        Assert.False(a.Equals("One"));
    }

    [Fact]
    public void GetHashCode_same_for_same_id()
    {
        var a = TestEnumeration.FromId(1);
        var b = TestEnumeration.FromId(1);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_returns_name()
    {
        Assert.Equal("One", TestEnumeration.One.ToString());
    }
}
