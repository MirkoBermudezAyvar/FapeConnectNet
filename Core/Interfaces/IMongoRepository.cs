// Core/Interfaces/IMongoRepository.cs
using System.Linq.Expressions;
using MongoDB.Driver;
using venar_bus_api_jakar_bckd_net.Core.Entities;

namespace venar_bus_api_jakar_bckd_net.Core.Interfaces
{
    public interface IMongoRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        // Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(string id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        
        // MongoDB-specific operations
        IFindFluent<T, T> GetFindFluent(Expression<Func<T, bool>> predicate);
        IAggregateFluent<T> GetAggregateFluent();
    }
}