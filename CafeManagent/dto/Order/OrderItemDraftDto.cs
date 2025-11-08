namespace CafeManagent.dto.Order
{
    public class OrderDraftDto
    {
        public List<OrderItemDraftDto> Items { get; set; } = new List<OrderItemDraftDto>();
        public string CustomerPhone { get; set; } 
        public decimal DiscountPercent { get; set; } 
        public string PaymentMethod { get; set; } 
        public string Note { get; set; }
    }
}
