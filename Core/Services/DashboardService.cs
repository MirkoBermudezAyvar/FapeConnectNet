// Core/Services/DashboardService.cs
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using venar_bus_api_jakar_bckd_net.Core.Entities;
using venar_bus_api_jakar_bckd_net.Core.Interfaces;
using venar_bus_api_jakar_bckd_net.DTOs;
using Microsoft.Extensions.Options;
using venar_bus_api_jakar_bckd_net.Infrastructure.Mongo;

namespace venar_bus_api_jakar_bckd_net.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly MongoDbSettings _settings;
        private readonly IRepository<Client> _clientRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Order> _orderRepository;

        public DashboardService(
            IOptions<MongoDbSettings> settings,
            IMongoClient mongoClient,
            IRepository<Client> clientRepository,
            IRepository<Product> productRepository,
            IRepository<Order> orderRepository)
        {
            _settings = settings.Value;
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase(_settings.DatabaseName);
            _clientRepository = clientRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            
            var totalClients = await _clientRepository.CountAsync();
            var totalProducts = await _productRepository.CountAsync();
            var totalOrders = await _orderRepository.CountAsync();
            var newOrdersToday = await _orderRepository.CountAsync(o => o.OrderDate.Date == today);
            var pendingOrders = await _orderRepository.CountAsync(o => o.Status == OrderStatus.Pending);
            
            // Calculate total revenue using MongoDB aggregation
            var ordersCollection = _database.GetCollection<Order>(_settings.OrderCollection);
            var totalRevenuePipeline = new[]
            {
                new BsonDocument("$match", 
                    new BsonDocument("IsActive", true)),
                new BsonDocument("$group", 
                    new BsonDocument
                    {
                        { "_id", BsonNull.Value },
                        { "totalRevenue", new BsonDocument("$sum", "$TotalAmount") }
                    })
            };
            
            var totalRevenueResult = await ordersCollection.Aggregate<BsonDocument>(totalRevenuePipeline).FirstOrDefaultAsync();
            decimal totalRevenue = totalRevenueResult != null && totalRevenueResult.Contains("totalRevenue") 
                ? totalRevenueResult["totalRevenue"].AsDecimal 
                : 0;

            // Get monthly sales for the last 6 months
            var sixMonthsAgo = today.AddMonths(-6);
            var monthlySalesPipeline = new[]
            {
                new BsonDocument("$match", 
                    new BsonDocument
                    {
                        { "IsActive", true },
                        { "OrderDate", new BsonDocument("$gte", sixMonthsAgo) }
                    }),
                new BsonDocument("$group", 
                    new BsonDocument
                    {
                        { "_id", new BsonDocument
                            {
                                { "month", new BsonDocument("$month", "$OrderDate") },
                                { "year", new BsonDocument("$year", "$OrderDate") }
                            }
                        },
                        { "revenue", new BsonDocument("$sum", "$TotalAmount") },
                        { "orderCount", new BsonDocument("$sum", 1) }
                    }),
                new BsonDocument("$sort", 
                    new BsonDocument("_id.year", 1).Add("_id.month", 1))
            };
            
            var monthlySalesResults = await ordersCollection.Aggregate<BsonDocument>(monthlySalesPipeline).ToListAsync();
            
            var monthlySales = monthlySalesResults.Select(doc => new MonthlySale
            {
                Month = $"{doc["_id"]["year"].AsInt32}-{doc["_id"]["month"].AsInt32:00}",
                Revenue = doc["revenue"].AsDecimal,
                OrderCount = doc["orderCount"].AsInt32
            }).ToList();

            // Get top products
            var orderItemsCollection = _database.GetCollection<OrderItem>(_settings.OrderItemCollection);
            var topProductsPipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("IsActive", true)),
                new BsonDocument("$lookup", 
                    new BsonDocument
                    {
                        { "from", _settings.ProductCollection },
                        { "localField", "ProductId" },
                        { "foreignField", "_id" },
                        { "as", "product" }
                    }),
                new BsonDocument("$unwind", "$product"),
                new BsonDocument("$group", 
                    new BsonDocument
                    {
                        { "_id", "$ProductId" },
                        { "productName", new BsonDocument("$first", "$product.Name") },
                        { "quantitySold", new BsonDocument("$sum", "$Quantity") },
                        { "revenue", new BsonDocument("$sum", "$TotalPrice") }
                    }),
                new BsonDocument("$sort", new BsonDocument("revenue", -1)),
                new BsonDocument("$limit", 5)
            };
            
            var topProductsResults = await orderItemsCollection.Aggregate<BsonDocument>(topProductsPipeline).ToListAsync();
            
            var topProducts = topProductsResults.Select(doc => new TopProduct
            {
                ProductId = int.Parse(doc["_id"].AsString),
                ProductName = doc["productName"].AsString,
                QuantitySold = doc["quantitySold"].AsInt32,
                Revenue = doc["revenue"].AsDecimal
            }).ToList();

            // Get top clients
            var topClientsPipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("IsActive", true)),
                new BsonDocument("$lookup", 
                    new BsonDocument
                    {
                        { "from", _settings.ClientCollection },
                        { "localField", "ClientId" },
                        { "foreignField", "_id" },
                        { "as", "client" }
                    }),
                new BsonDocument("$unwind", "$client"),
                new BsonDocument("$group", 
                    new BsonDocument
                    {
                        { "_id", "$ClientId" },
                        { "clientName", new BsonDocument("$first", "$client.Name") },
                        { "totalSpent", new BsonDocument("$sum", "$TotalAmount") },
                        { "orderCount", new BsonDocument("$sum", 1) }
                    }),
                new BsonDocument("$sort", new BsonDocument("totalSpent", -1)),
                new BsonDocument("$limit", 5)
            };
            
            var topClientsResults = await ordersCollection.Aggregate<BsonDocument>(topClientsPipeline).ToListAsync();
            
            var topClients = topClientsResults.Select(doc => new TopClient
            {
                ClientId = int.Parse(doc["_id"].AsString),
                ClientName = doc["clientName"].AsString,
                TotalSpent = doc["totalSpent"].AsDecimal,
                OrderCount = doc["orderCount"].AsInt32
            }).ToList();

            return new DashboardStats
            {
                TotalClients = totalClients,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                NewOrdersToday = newOrdersToday,
                PendingOrders = pendingOrders,
                MonthlySales = monthlySales,
                TopProducts = topProducts,
                TopClients = topClients
            };
        }

        public async Task<IEnumerable<MonthlySale>> GetMonthlySalesAsync(int year)
        {
            var ordersCollection = _database.GetCollection<Order>(_settings.OrderCollection);
            
            var monthlySalesPipeline = new[]
            {
                new BsonDocument("$match", 
                    new BsonDocument
                    {
                        { "IsActive", true },
                        { "OrderDate", new BsonDocument("$gte", new DateTime(year, 1, 1)) },
                        { "OrderDate", new BsonDocument("$lt", new DateTime(year + 1, 1, 1)) }
                    }),
                new BsonDocument("$group", 
                    new BsonDocument
                    {
                        { "_id", new BsonDocument("$month", "$OrderDate") },
                        { "revenue", new BsonDocument("$sum", "$TotalAmount") },
                        { "orderCount", new BsonDocument("$sum", 1) }
                    }),
                new BsonDocument("$sort", new BsonDocument("_id", 1))
            };
            
            var monthlySalesResults = await ordersCollection.Aggregate<BsonDocument>(monthlySalesPipeline).ToListAsync();
            
            var monthlySales = monthlySalesResults.Select(doc => new MonthlySale
            {
                Month = $"{year}-{doc["_id"].AsInt32:00}",
                Revenue = doc["revenue"].AsDecimal,
                OrderCount = doc["orderCount"].AsInt32
            }).ToList();

            return monthlySales;
        }

        public async Task<IEnumerable<TopProduct>> GetTopProductsAsync(int limit = 5)
        {
            var orderItemsCollection = _database.GetCollection<OrderItem>(_settings.OrderItemCollection);
            
            var topProductsPipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("IsActive", true)),
                new BsonDocument("$lookup", 
                    new BsonDocument
                    {
                        { "from", _settings.ProductCollection },
                        { "localField", "ProductId" },
                        { "foreignField", "_id" },
                        { "as", "product" }
                    }),
                new BsonDocument("$unwind", "$product"),
                new BsonDocument("$group", 
                    new BsonDocument
                    {
                        { "_id", "$ProductId" },
                        { "productName", new BsonDocument("$first", "$product.Name") },
                        { "quantitySold", new BsonDocument("$sum", "$Quantity") },
                        { "revenue", new BsonDocument("$sum", "$TotalPrice") }
                    }),
                new BsonDocument("$sort", new BsonDocument("revenue", -1)),
                new BsonDocument("$limit", limit)
            };
            
            var topProductsResults = await orderItemsCollection.Aggregate<BsonDocument>(topProductsPipeline).ToListAsync();
            
            var topProducts = topProductsResults.Select(doc => new TopProduct
            {
                ProductId = int.Parse(doc["_id"].AsString),
                ProductName = doc["productName"].AsString,
                QuantitySold = doc["quantitySold"].AsInt32,
                Revenue = doc["revenue"].AsDecimal
            }).ToList();

            return topProducts;
        }

        public async Task<IEnumerable<TopClient>> GetTopClientsAsync(int limit = 5)
        {
            var ordersCollection = _database.GetCollection<Order>(_settings.OrderCollection);
            
            var topClientsPipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("IsActive", true)),
                new BsonDocument("$lookup", 
                    new BsonDocument
                    {
                        { "from", _settings.ClientCollection },
                        { "localField", "ClientId" },
                        { "foreignField", "_id" },
                        { "as", "client" }
                    }),
                new BsonDocument("$unwind", "$client"),
                new BsonDocument("$group", 
                    new BsonDocument
                    {
                        { "_id", "$ClientId" },
                        { "clientName", new BsonDocument("$first", "$client.Name") },
                        { "totalSpent", new BsonDocument("$sum", "$TotalAmount") },
                        { "orderCount", new BsonDocument("$sum", 1) }
                    }),
                new BsonDocument("$sort", new BsonDocument("totalSpent", -1)),
                new BsonDocument("$limit", limit)
            };
            
            var topClientsResults = await ordersCollection.Aggregate<BsonDocument>(topClientsPipeline).ToListAsync();
            
            var topClients = topClientsResults.Select(doc => new TopClient
            {
                ClientId = int.Parse(doc["_id"].AsString),
                ClientName = doc["clientName"].AsString,
                TotalSpent = doc["totalSpent"].AsDecimal,
                OrderCount = doc["orderCount"].AsInt32
            }).ToList();

            return topClients;
        }

        public async Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var ordersCollection = _database.GetCollection<Order>(_settings.OrderCollection);
            
            var salesSummaryPipeline = new[]
            {
                new BsonDocument("$match", 
                    new BsonDocument
                    {
                        { "IsActive", true },
                        { "OrderDate", new BsonDocument
                            {
                                { "$gte", startDate },
                                { "$lte", endDate }
                            } 
                        }
                    }),
                new BsonDocument("$group", 
                    new BsonDocument
                    {
                        { "_id", BsonNull.Value },
                        { "totalOrders", new BsonDocument("$sum", 1) },
                        { "totalRevenue", new BsonDocument("$sum", "$TotalAmount") }
                    })
            };
            
            var salesSummaryResult = await ordersCollection.Aggregate<BsonDocument>(salesSummaryPipeline).FirstOrDefaultAsync();
            
            int totalOrders = salesSummaryResult != null ? salesSummaryResult["totalOrders"].AsInt32 : 0;
            decimal totalRevenue = salesSummaryResult != null ? salesSummaryResult["totalRevenue"].AsDecimal : 0;
            
            // Find new clients who made their first order in this period
            var newClientsQuery = new[]
            {
                new BsonDocument("$match", 
                    new BsonDocument
                    {
                        { "IsActive", true },
                        { "OrderDate", new BsonDocument
                            {
                                { "$gte", startDate },
                                { "$lte", endDate }
                            } 
                        }
                    }),
                new BsonDocument("$group", 
                    new BsonDocument
                    {
                        { "_id", "$ClientId" },
                        { "firstOrderDate", new BsonDocument("$min", "$OrderDate") }
                    }),
                new BsonDocument("$lookup", 
                    new BsonDocument
                    {
                        { "from", _settings.OrderCollection },
                        { "let", new BsonDocument("clientId", "$_id") },
                        { "pipeline", new BsonArray
                            {
                                new BsonDocument("$match", 
                                    new BsonDocument("$expr", 
                                        new BsonDocument
                                        {
                                            { "$and", new BsonArray
                                                {
                                                    new BsonDocument("$eq", new BsonArray { "$ClientId", "$$clientId" }),
                                                    new BsonDocument("$lt", new BsonArray { "$OrderDate", startDate })
                                                }
                                            }
                                        }
                                    )
                                )
                            }
                        },
                        { "as", "previousOrders" }
                    }),
                new BsonDocument("$match", 
                    new BsonDocument("previousOrders", 
                        new BsonDocument("$size", 0))),
                new BsonDocument("$count", "count")
            };
            
            var newClientsResult = await ordersCollection.Aggregate<BsonDocument>(newClientsQuery).FirstOrDefaultAsync();
            int newClients = newClientsResult != null ? newClientsResult["count"].AsInt32 : 0;
            
            decimal averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            return new SalesSummaryDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                NewClients = newClients,
                AverageOrderValue = averageOrderValue
            };
        }

        // Reactive methods
        public IObservable<DashboardStats> GetDashboardStatsReactive()
        {
            return Observable.FromAsync(() => GetDashboardStatsAsync());
        }

        public IObservable<IEnumerable<MonthlySale>> GetMonthlySalesReactive(int year)
        {
            return Observable.FromAsync(() => GetMonthlySalesAsync(year));
        }

        public IObservable<IEnumerable<TopProduct>> GetTopProductsReactive(int limit = 5)
        {
            return Observable.FromAsync(() => GetTopProductsAsync(limit));
        }

        public IObservable<IEnumerable<TopClient>> GetTopClientsReactive(int limit = 5)
        {
            return Observable.FromAsync(() => GetTopClientsAsync(limit));
        }

        public IObservable<SalesSummaryDto> GetSalesSummaryReactive(DateTime startDate, DateTime endDate)
        {
            return Observable.FromAsync(() => GetSalesSummaryAsync(startDate, endDate));
        }
    }
}