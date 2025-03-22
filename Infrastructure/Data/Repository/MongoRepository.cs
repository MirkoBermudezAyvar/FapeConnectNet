// Infrastructure/Repositories/MongoRepository.cs
using MongoDB.Driver;
using System.Linq.Expressions;
using venar_bus_api_jakar_bckd_net.Core.Entities;
using venar_bus_api_jakar_bckd_net.Core.Interfaces;
using venar_bus_api_jakar_bckd_net.Infrastructure.Data.MongoDb;

namespace venar_bus_api_jakar_bckd_net.Infrastructure.Repositories
{
    public class MongoRepository<T> : IMongoRepository<T> where T : BaseEntity
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoDbContext context, string collectionName)
        {
            _collection = context.GetCollection<T>(collectionName);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(e => e.IsActive).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await _collection.Find(e => e.Id == id && e.IsActive).FirstOrDefaultAsync();
        }

        
        public async Task<T> AddAsync(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
        }

        public async Task DeleteAsync(string id)
        {
            var update = Builders<T>.Update.Set(e => e.IsActive, false).Set(e => e.UpdatedAt, DateTime.UtcNow);
            await _collection.UpdateOneAsync(e => e.Id == id, update);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _collection.Find(e => e.Id == id && e.IsActive).AnyAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return (int)await _collection.CountDocumentsAsync(e => e.IsActive);
            
            return (int)await _collection.CountDocumentsAsync(e => e.IsActive && predicate.Compile()(e));
        }

        public IFindFluent<T, T> GetFindFluent(Expression<Func<T, bool>> predicate)
        {
            return _collection.Find(e => e.IsActive && predicate.Compile()(e));
        }

        public IAggregateFluent<T> GetAggregateFluent()
        {
            return _collection.Aggregate().Match(e => e.IsActive);
        }
    }
}