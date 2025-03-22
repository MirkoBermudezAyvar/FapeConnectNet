namespace venar_bus_api_jakar_bckd_net.Core.Entities
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}