using BuildingBlocks.Kernel.Domain.Entities;
using BuildingBlocks.Kernel.Domain.Specifications;
using BuildingBlocks.Kernel.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Tests.Kernel.Infrastructure;

public class SpecificationEvaluatorTests
{
    [Fact]
    public void ApplySpecification_with_no_criteria_returns_same_query()
    {
        using var context = new TestDbContext();
        var query = context.Set<TestAggregate>().AsQueryable();
        var spec = new EmptySpec();

        var result = query.ApplySpecification(spec);

        Assert.NotNull(result);
    }

    [Fact]
    public void ApplySpecification_with_criteria_adds_where()
    {
        using var context = new TestDbContext();
        context.AddRange(
            new TestAggregate { Name = "Active", IsActive = true },
            new TestAggregate { Name = "Inactive", IsActive = false });
        context.SaveChanges();

        var spec = new ActiveSpec();
        var query = context.Set<TestAggregate>().AsQueryable();

        var result = query.ApplySpecification(spec).ToList();

        Assert.Single(result);
        Assert.Equal("Active", result[0].Name);
    }

    [Fact]
    public void ApplySpecification_with_skip_and_take()
    {
        using var context = new TestDbContext();
        var seed = Enumerable.Range(1, 10)
            .Select(i => new TestAggregate { Name = $"Item{i}", IsActive = true })
            .ToList();
        context.AddRange(seed);
        context.SaveChanges();

        var spec = new PagedSpec(2, 3);
        var query = context.Set<TestAggregate>().AsQueryable();

        var result = query.ApplySpecification(spec).ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplySpecification_with_order_by()
    {
        using var context = new TestDbContext();
        context.AddRange(
            new TestAggregate { Name = "Charlie", IsActive = true },
            new TestAggregate { Name = "Alpha", IsActive = true },
            new TestAggregate { Name = "Bravo", IsActive = true });
        context.SaveChanges();

        var spec = new OrderedSpec();
        var query = context.Set<TestAggregate>().AsQueryable();

        var result = query.ApplySpecification(spec).ToList();

        Assert.Equal("Alpha", result[0].Name);
        Assert.Equal("Bravo", result[1].Name);
        Assert.Equal("Charlie", result[2].Name);
    }

    private sealed class EmptySpec : Specification<TestAggregate>;

    private sealed class ActiveSpec : Specification<TestAggregate>
    {
        public ActiveSpec() => Where(a => a.IsActive);
    }

    private sealed class PagedSpec : Specification<TestAggregate>
    {
        public PagedSpec(int skip, int take)
        {
            ApplyPaging(skip, take);
        }
    }

    private sealed class OrderedSpec : Specification<TestAggregate>
    {
        public OrderedSpec() => ApplyOrderBy(a => a.Name);
    }

    private sealed class TestDbContext : DbContext
    {
        public DbSet<TestAggregate> TestAggregates => Set<TestAggregate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAggregate>().HasKey(a => a.Id);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
    }

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
