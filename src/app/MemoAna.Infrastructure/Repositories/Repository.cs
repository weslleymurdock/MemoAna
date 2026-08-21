using MemoAna.Application.Common.Abstract.Repositories;
using MemoAna.Infrastructure.Persistence;
using MemoAna.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace MemoAna.Infrastructure.Repositories;

public sealed class Repository(GameDbContext context) : IRepository
{
    public async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : EntityBase
        => await context.Set<T>().AddAsync(entity, cancellationToken);

    public async Task DeleteAsync<T>(string id, CancellationToken cancellationToken = default) where T : EntityBase
        => context.Set<T>().Remove(await GetAsync<T>(id, cancellationToken));

    public async Task<T> GetAsync<T>(string id, CancellationToken cancellationToken = default) where T : EntityBase
        => await context.Set<T>().FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Entity ({nameof(T)}) not found");

    public async Task<IReadOnlyCollection<T>> GetAsync<T>(CancellationToken cancellationToken = default) where T : EntityBase
        => await context.Set<T>().ToListAsync(cancellationToken);

    public IQueryable<T> Query<T>() where T : EntityBase
        => context.Set<T>();
    public IQueryable<T> QueryAsNoTracking<T>() where T : EntityBase
        => context.Set<T>().AsNoTracking();

    public int SaveChanges() => context.SaveChanges();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);

    public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : EntityBase
        => context.Set<T>().Update(entity);
}
