 namespace venar_bus_api_jakar_bckd_net.Core.Entities
{
    public class DashboardStats
    {
        public int TotalClients { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NewOrdersToday { get; set; }
        public int PendingOrders { get; set; }
        public List<MonthlySale> MonthlySales { get; set; } = new List<MonthlySale>();
        public List<TopProduct> TopProducts { get; set; } = new List<TopProduct>();
        public List<TopClient> TopClients { get; set; } = new List<TopClient>();
    }

    public class MonthlySale
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopClient
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
    }
}