// Infrastructure/Data/MongoDb/MongoDbSettings.cs
namespace venar_bus_api_jakar_bckd_net.Infrastructure.Data.MongoDb
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}