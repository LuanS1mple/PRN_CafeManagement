namespace CafeManagent.dto.response.OrderModuleDTO
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string? CustomerName { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? Status { get; set; }
        public string? StatusText { get; set; }
        public string? OrderTime { get; set; }
    }
}


