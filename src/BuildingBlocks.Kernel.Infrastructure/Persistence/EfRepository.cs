using BuildingBlocks.Kernel.Domain.Entities;
using BuildingBlocks.Kernel.Domain.Repositories;
using BuildingBlocks.Kernel.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Kernel.Infrastructure.Persistence;

/// <summary>
/// Generic EF Core repository for aggregate roots. Derive per aggregate to add
/// intention-revealing queries (GetPendingOrdersAsync, ...).
/// </summary>
public class EfRepository<TAggregate, TId>(DbContext dbContext) : IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    protected DbContext DbContext { get; } = dbContext;

    protected DbSet<TAggregate> Set => DbContext.Set<TAggregate>();

    public virtual async Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(aggregate => aggregate.Id.Equals(id), cancellationToken);

    public virtual async Task<TAggregate?> FirstOrDefaultAsync(
        ISpecification<TAggregate> specification, CancellationToken cancellationToken = default) =>
        await Set.ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<TAggregate>> ListAsync(
        ISpecification<TAggregate> specification, CancellationToken cancellationToken = default) =>
        await Set.ApplySpecification(specification).ToListAsync(cancellationToken);

    public virtual async Task<bool> AnyAsync(
        ISpecification<TAggregate> specification, CancellationToken cancellationToken = default) =>
        await Set.ApplySpecification(specification).AnyAsync(cancellationToken);

    public virtual async Task<int> CountAsync(
        ISpecification<TAggregate> specification, CancellationToken cancellationToken = default) =>
        await Set.ApplySpecification(specification).CountAsync(cancellationToken);

    public virtual async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default) =>
        await Set.AddAsync(aggregate, cancellationToken);

    public virtual void Update(TAggregate aggregate) => Set.Update(aggregate);

    public virtual void Remove(TAggregate aggregate) => Set.Remove(aggregate);
}
