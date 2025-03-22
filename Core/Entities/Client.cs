namespace venar_bus_api_jakar_bckd_net.Core.Entities
{
    public class Client : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? ContactPerson { get; set; }
        public decimal TotalPurchases { get; set; } = 0;
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}