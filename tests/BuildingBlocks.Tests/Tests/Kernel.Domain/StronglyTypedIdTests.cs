using BuildingBlocks.Tests.Helpers;
using BuildingBlocks.Kernel.Domain.ValueObjects;

namespace BuildingBlocks.Tests.Kernel.Domain;

public class StronglyTypedIdTests
{
    [Fact]
    public void Value_is_set_correctly()
    {
        var guid = Guid.NewGuid();
        var id = new OrderId(guid);

        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void ToString_returns_value_as_string()
    {
        var guid = Guid.NewGuid();
        var id = new OrderId(guid);

        Assert.Equal(guid.ToString(), id.ToString());
    }

    [Fact]
    public void Equality_same_value_returns_true()
    {
        var guid = Guid.NewGuid();
        var a = new OrderId(guid);
        var b = new OrderId(guid);

        Assert.Equal(a, b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equality_different_value_returns_false()
    {
        var a = new OrderId(Guid.NewGuid());
        var b = new OrderId(Guid.NewGuid());

        Assert.NotEqual(a, b);
        Assert.False(a.Equals(b));
    }
}

public class TenantIdTests
{
    [Fact]
    public void New_generates_non_empty_guid()
    {
        var tenantId = TenantId.New();

        Assert.NotEqual(Guid.Empty, tenantId.Value);
    }

    [Fact]
    public void From_sets_value()
    {
        var guid = Guid.NewGuid();
        var tenantId = TenantId.From(guid);

        Assert.Equal(guid, tenantId.Value);
    }

    [Fact]
    public void Implicit_conversion_from_guid()
    {
        var guid = Guid.NewGuid();
        TenantId tenantId = guid;

        Assert.Equal(guid, tenantId.Value);
    }

    [Fact]
    public void Value_is_correct()
    {
        var guid = Guid.NewGuid();
        var tenantId = new TenantId(guid);

        Assert.Equal(guid, tenantId.Value);
    }

    [Fact]
    public void ToString_returns_value_string()
    {
        var guid = Guid.NewGuid();
        var tenantId = TenantId.From(guid);

        Assert.Equal(guid.ToString(), tenantId.ToString());
    }
}
