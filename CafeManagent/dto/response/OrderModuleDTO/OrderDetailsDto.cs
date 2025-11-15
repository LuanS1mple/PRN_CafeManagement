using System.Collections.Generic;
namespace CafeManagent.dto.response.OrderModuleDTO;
public class OrderDetailsDto : OrderDto 
{
    public string CustomerPhone { get; set; }
    public string Note { get; set; }
    public decimal Discount { get; set; }
    public bool Vat { get; set; }

    // Danh sách chi tiết các món hàng
    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}