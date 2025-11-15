using CafeManagent.dto.request.OrderModuleDTO;
using CafeManagent.Models;
using System;
using System.Linq;

namespace CafeManagent.mapper
{
    public class DraftOrderMapper
    {
        public static Order MapDraftToOrder(
            OrderDraftDetailsDto draftDetails,
            string paymentMethod,
            int status,
            int? staffId,
            int? newCustomerId = null)
        {
            if (draftDetails == null || draftDetails.Draft == null)
            {
                throw new ArgumentNullException(nameof(draftDetails), "Dữ liệu nháp không hợp lệ.");
            }
            int? finalCustomerId = newCustomerId ?? draftDetails.Customer?.CustomerId;
            if (finalCustomerId.HasValue && finalCustomerId.Value == 0)
            {
                finalCustomerId = null;
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
                CustomerId = finalCustomerId,
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