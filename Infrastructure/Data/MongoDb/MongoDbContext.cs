// Infrastructure/Data/MongoDb/MongoDbContext.cs
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace venar_bus_api_jakar_bckd_net.Infrastructure.Data.MongoDb
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            _database = client.GetDatabase(options.Value.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}