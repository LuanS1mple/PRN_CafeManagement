namespace CafeManagent.dto.request.OrderModuleDTO
{
    public class OrderItemDraftDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
