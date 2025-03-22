// Infrastructure/Data/MongoDb/DataMigrationService.cs
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using venar_bus_api_jakar_bckd_net.Core.Entities;
using venar_bus_api_jakar_bckd_net.Core.Interfaces;

namespace venar_bus_api_jakar_bckd_net.Infrastructure.Data.MongoDb
{
    public class DataMigrationService
    {
        private readonly AppDbContext _sqlContext;
        private readonly IMongoDbContext _mongoContext;

        public DataMigrationService(AppDbContext sqlContext, IMongoDbContext mongoContext)
        {
            _sqlContext = sqlContext;
            _mongoContext = mongoContext;
        }

        public async Task MigrateAllData()
        {
            await MigrateUsers();
            await MigrateCategories();
            await MigrateProducts();
            await MigrateClients();
            await MigrateOrders();
        }

        private async Task MigrateUsers()
        {
            var users = await _sqlContext.Users.ToListAsync();
            if (users.Any())
            {
                var collection = _mongoContext.GetCollection<User>("users");
                await collection.InsertManyAsync(users);
            }
        }

        private async Task MigrateCategories()
        {
            var categories = await _sqlContext.Categories.ToListAsync();
            if (categories.Any())
            {
                var collection = _mongoContext.GetCollection<Category>("categories");
                await collection.InsertManyAsync(categories);
            }
        }

        private async Task MigrateProducts()
        {
            var products = await _sqlContext.Products.ToListAsync();
            if (products.Any())
            {
                var collection = _mongoContext.GetCollection<Product>("products");
                await collection.InsertManyAsync(products);
            }
        }

        private async Task MigrateClients()
        {
            var clients = await _sqlContext.Clients.ToListAsync();
            if (clients.Any())
            {
                var collection = _mongoContext.GetCollection<Client>("clients");
                await collection.InsertManyAsync(clients);
            }
        }

        private async Task MigrateOrders()
        {
            var orders = await _sqlContext.Orders.Include(o => o.Items).ToListAsync();
            if (orders.Any())
            {
                var orderCollection = _mongoContext.GetCollection<Order>("orders");
                await orderCollection.InsertManyAsync(orders);

                var orderItems = orders.SelectMany(o => o.Items).ToList();
                if (orderItems.Any())
                {
                    var orderItemCollection = _mongoContext.GetCollection<OrderItem>("order_items");
                    await orderItemCollection.InsertManyAsync(orderItems);
                }
            }
        }
    }
}