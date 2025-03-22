// Infrastructure/Data/MongoDb/IMongoDbContext.cs
using MongoDB.Driver;

namespace venar_bus_api_jakar_bckd_net.Infrastructure.Data.MongoDb
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string name);
    }
}