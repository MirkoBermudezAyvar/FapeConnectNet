using venar_bus_api_jakar_bckd_net.Core.Entities;
using venar_bus_api_jakar_bckd_net.DTOs;
namespace venar_bus_api_jakar_bckd_net.Core.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<Order> CreateOrderAsync(CreateOrderDto orderDto);
        Task UpdateOrderStatusAsync(int id, OrderStatus status);
        Task<IEnumerable<Order>> GetOrdersByClientAsync(int clientId);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
    }
}