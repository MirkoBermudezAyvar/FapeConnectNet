namespace venar_bus_api_jakar_bckd_net.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalClients { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NewOrdersToday { get; set; }
        public int PendingOrders { get; set; }
        public List<MonthlySaleDto> MonthlySales { get; set; } = new List<MonthlySaleDto>();
        public List<TopProductDto> TopProducts { get; set; } = new List<TopProductDto>();
        public List<TopClientDto> TopClients { get; set; } = new List<TopClientDto>();
    }

    public class MonthlySaleDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopClientDto
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
    }

    public class SalesSummaryDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NewClients { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}