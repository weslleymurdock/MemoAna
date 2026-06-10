using MemoAna.Application.Abstract.Repositories;
using MemoAna.Domain.Entities;
using MemoAna.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MemoAna.Infrastructure.Repositories;

internal sealed class Repository(GameDbContext context) : IRepository
{
    public async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
        => await context.Set<T>().AddAsync(entity, cancellationToken);

    public async Task DeleteAsync<T>(string id, CancellationToken cancellationToken = default) where T : BaseEntity
        => context.Set<T>().Remove(await GetAsync<T>(id, cancellationToken));

    public async Task<T> GetAsync<T>(string id, CancellationToken cancellationToken = default) where T : BaseEntity
        => await context.Set<T>().FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Entity ({nameof(T)}) not found");

    public async Task<IReadOnlyCollection<T>> GetAsync<T>(CancellationToken cancellationToken = default) where T : BaseEntity
        => await context.Set<T>().ToListAsync(cancellationToken);

    public IQueryable<T> Query<T>() where T : BaseEntity
        => context.Set<T>();
    public IQueryable<T> QueryAsNoTracking<T>() where T : BaseEntity
        => context.Set<T>().AsNoTracking();
    public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
        => context.Set<T>().Update(entity);
}
