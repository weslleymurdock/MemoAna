using MemoAna.Domain.Common.Entities;

namespace MemoAna.Application.Common.Abstract.Repositories;

public interface IRepository 
{
    IQueryable<T> Query<T>() where T : EntityBase;
    IQueryable<T> QueryAsNoTracking<T>() where T : EntityBase;
    Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : EntityBase;
    Task<T> GetAsync<T>(string id, CancellationToken cancellationToken = default) where T : EntityBase;
    Task<IReadOnlyCollection<T>> GetAsync<T>(CancellationToken cancellationToken = default) where T : EntityBase;
    Task DeleteAsync<T>(string id, CancellationToken cancellationToken = default) where T : EntityBase;
    Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : EntityBase;
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
