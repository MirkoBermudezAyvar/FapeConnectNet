using venar_bus_api_jakar_bckd_net.Core.Entities;
using venar_bus_api_jakar_bckd_net.DTOs;

namespace venar_bus_api_jakar_bckd_net.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStats> GetDashboardStatsAsync();
        Task<IEnumerable<MonthlySale>> GetMonthlySalesAsync(int year);
        Task<IEnumerable<TopProduct>> GetTopProductsAsync(int limit = 5);
        Task<IEnumerable<TopClient>> GetTopClientsAsync(int limit = 5);
        Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime startDate, DateTime endDate);
    }
}