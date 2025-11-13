using CafeManagent.dto.Order;
using CafeManagent.Models;

namespace CafeManagent.mapper
{
    public class DraftOrderMapper
    {
        public static Order MapDraftToOrder(
            OrderDraftDetailsDto draftDetails,
            string paymentMethod,
            int status,
            int? staffId)
        {
            if (draftDetails == null || draftDetails.Draft == null)
            {
                throw new ArgumentNullException(nameof(draftDetails), "Dữ liệu nháp không hợp lệ.");
            }
            var orderItems = draftDetails.Draft.Items.Select(itemDto => new OrderItem
            {
                ProductName = itemDto.ProductName,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
            }).ToList();
            return new Order
            {
                OrderDate = DateTime.Now,
                StaffId = staffId, 
                CustomerId = draftDetails.Customer?.CustomerId,
                TotalAmount = draftDetails.GrandTotal, 
                Discount = draftDetails.DiscountAmount,
                Vat = draftDetails.VATAmount > 0, 
                OrderPrice = draftDetails.Subtotal,
                Status = status,
                Note = draftDetails.Draft.Note,

                OrderItems = orderItems
            };
        }
    }
}
