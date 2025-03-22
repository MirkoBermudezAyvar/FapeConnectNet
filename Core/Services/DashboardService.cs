// Core/Services/DashboardService.cs
using venar_bus_api_jakar_bckd_net.Core.Entities;
using venar_bus_api_jakar_bckd_net.Core.Interfaces;
using venar_bus_api_jakar_bckd_net.DTOs;
using venar_bus_api_jakar_bckd_net.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace venar_bus_api_jakar_bckd_net.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IRepository<Client> _clientRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Order> _orderRepository;

        public DashboardService(
            AppDbContext context,
            IRepository<Client> clientRepository,
            IRepository<Product> productRepository,
            IRepository<Order> orderRepository)
        {
            _context = context;
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
            
            // Calculate total revenue
            var totalRevenue = await _context.Orders
                .Where(o => o.IsActive)
                .SumAsync(o => o.TotalAmount);

            // Get monthly sales for the last 6 months
            var sixMonthsAgo = today.AddMonths(-6);
            var monthlySales = await _context.Orders
                .Where(o => o.IsActive && o.OrderDate >= sixMonthsAgo)
                .GroupBy(o => new { Month = o.OrderDate.Month, Year = o.OrderDate.Year })
                .Select(g => new MonthlySale
                {
                    Month = $"{g.Key.Year}-{g.Key.Month.ToString("00")}",
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToListAsync();

            // Get top 5 products
            var topProducts = await _context.OrderItems
                .Where(oi => oi.IsActive)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProduct
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(5)
                .ToListAsync();

            // Get top 5 clients
            var topClients = await _context.Orders
                .Where(o => o.IsActive)
                .GroupBy(o => new { o.ClientId, o.Client.Name })
                .Select(g => new TopClient
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.Name,
                    TotalSpent = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(5)
                .ToListAsync();

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
            var monthlySales = await _context.Orders
                .Where(o => o.IsActive && o.OrderDate.Year == year)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new MonthlySale
                {
                    Month = $"{year}-{g.Key.ToString("00")}",
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToListAsync();

            return monthlySales;
        }

        public async Task<IEnumerable<TopProduct>> GetTopProductsAsync(int limit = 5)
        {
            var topProducts = await _context.OrderItems
                .Where(oi => oi.IsActive)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProduct
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(limit)
                .ToListAsync();

            return topProducts;
        }

        public async Task<IEnumerable<TopClient>> GetTopClientsAsync(int limit = 5)
        {
            var topClients = await _context.Orders
                .Where(o => o.IsActive)
                .GroupBy(o => new { o.ClientId, o.Client.Name })
                .Select(g => new TopClient
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.Name,
                    TotalSpent = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(limit)
                .ToListAsync();

            return topClients;
        }

        public async Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Where(o => o.IsActive && o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToListAsync();

            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.TotalAmount);
            
            // Get clients who made their first order in this period
            var newClientIds = await _context.Orders
                .Where(o => o.IsActive && o.OrderDate >= startDate && o.OrderDate <= endDate)
                .GroupBy(o => o.ClientId)
                .Select(g => new 
                {
                    ClientId = g.Key,
                    FirstOrderDate = g.Min(o => o.OrderDate)
                })
                .Where(c => !_context.Orders.Any(o => 
                    o.ClientId == c.ClientId && o.OrderDate < startDate))
                .CountAsync();

            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            return new SalesSummaryDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                NewClients = newClientIds,
                AverageOrderValue = averageOrderValue
            };
        }
    }
}