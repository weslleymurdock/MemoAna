using MemoAna.Domain.Entities;

namespace MemoAna.Application.Abstract.Repositories;

public interface IRepository 
{
    IQueryable<T> Query<T>() where T : BaseEntity;
    IQueryable<T> QueryAsNoTracking<T>() where T : BaseEntity;
    Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity;
    Task<T> GetAsync<T>(string id, CancellationToken cancellationToken = default) where T : BaseEntity;
    Task<IReadOnlyCollection<T>> GetAsync<T>(CancellationToken cancellationToken = default) where T : BaseEntity;
    Task DeleteAsync<T>(string id, CancellationToken cancellationToken = default) where T : BaseEntity;
    Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity;
}
