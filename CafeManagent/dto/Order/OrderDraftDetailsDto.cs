using CafeManagent.Models;

namespace CafeManagent.dto.Order
{
    public class OrderDraftDetailsDto
    {
        public OrderDraftDto Draft { get; set; } 
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalBeforeVAT { get; set; }
        public decimal VATAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public Customer? Customer { get; set; }
        public int PointsEarned { get; set; } = 0;
    }
}
