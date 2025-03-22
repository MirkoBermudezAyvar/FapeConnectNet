// Infrastructure/Extensions/MongoDbServiceExtensions.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using venar_bus_api_jakar_bckd_net.Core.Entities;
using venar_bus_api_jakar_bckd_net.Core.Interfaces;
using venar_bus_api_jakar_bckd_net.Infrastructure.Data.MongoDb;
using venar_bus_api_jakar_bckd_net.Infrastructure.Repositories;

namespace venar_bus_api_jakar_bckd_net.Infrastructure.Extensions
{
    public static class MongoDbServiceExtensions
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            // Register MongoDB serializers
            BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc));
            BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));

            // Register MongoDB configuration
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));
            services.AddSingleton<IMongoDbContext, MongoDbContext>();

            // Register MongoDB repositories for each entity
            services.AddScoped<IMongoRepository<User>>(provider => 
                new MongoRepository<User>(provider.GetRequiredService<IMongoDbContext>(), "users"));
            
            services.AddScoped<IMongoRepository<Client>>(provider => 
                new MongoRepository<Client>(provider.GetRequiredService<IMongoDbContext>(), "clients"));
            
            services.AddScoped<IMongoRepository<Product>>(provider => 
                new MongoRepository<Product>(provider.GetRequiredService<IMongoDbContext>(), "products"));
            
            services.AddScoped<IMongoRepository<Category>>(provider => 
                new MongoRepository<Category>(provider.GetRequiredService<IMongoDbContext>(), "categories"));
            
            services.AddScoped<IMongoRepository<Order>>(provider => 
                new MongoRepository<Order>(provider.GetRequiredService<IMongoDbContext>(), "orders"));
            
            services.AddScoped<IMongoRepository<OrderItem>>(provider => 
                new MongoRepository<OrderItem>(provider.GetRequiredService<IMongoDbContext>(), "order_items"));

            return services;
        }
    }
}