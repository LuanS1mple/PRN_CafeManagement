using CafeManagent.dto.Order;
using CafeManagent.Models;
using CafeManagent.Services.Interface;
using CafeManagent.Services.Interface.CustomerModule;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CafeManagent.Services
{
    public class OrderService : IOrderService
    {
        private readonly CafeManagementContext _db;
        private readonly ICustomerService _customerService;
        public OrderService(CafeManagementContext db, ICustomerService customerService)
        {
            _db = db;
            _customerService = customerService;
        }

        public List<Order> GetAll()
        {
            return _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public List<Order> GetByStatuses(params int[] statuses)
        {
            return _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.OrderItems)
                .Where(o => o.Status != null && statuses.Contains(o.Status.Value))
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public Order? GetById(int id)
        {
            return _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .FirstOrDefault(o => o.OrderId == id);
        }

        public Order Add(Order order)
        {
            order.OrderDate = DateTime.Now;
            order.Status = 0; 
            _db.Orders.Add(order);
            _db.SaveChanges();
            return order;
        }

        public bool Cancel(int id)
        {
            var o = _db.Orders.Find(id);
            if (o == null) return false;
            o.Status = -1; // Đã hủy
            _db.SaveChanges();
            return true;
        }

        public bool SetPreparing(int id)
        {
            var o = _db.Orders.Find(id);
            if (o == null) return false;
            o.Status = 1; // Đang chuẩn bị
            _db.SaveChanges();
            return true;
        }

        public bool SetReady(int id)
        {
            var o = _db.Orders.Find(id);
            if (o == null) return false;
            o.Status = 2; // Đã sẵn sàng
            _db.SaveChanges();
            return true;
        }

        public bool ConfirmDelivered(int id)
        {
            var o = _db.Orders.Find(id);
            if (o == null) return false;
            o.Status = 3; // Hoàn thành
            _db.SaveChanges();
            return true;
        }

       
        public bool SetRefunded(int id)
        {
            var o = _db.Orders.Find(id);
            if (o == null || o.Status != 3) return false;

            o.Status = -2; // Đã hoàn tiền
            _db.SaveChanges();
            return true;
        }
        public Order CreateOrderFromDraft(OrderDraftDto draft, int? staffId, decimal grandTotal, int? customerId)
        {
            // 1. Tạo đối tượng Order
            var order = new Order
            {
                StaffId = staffId,
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                TotalAmount = grandTotal, // Tổng cuối cùng đã tính VAT & Discount
                Discount = draft.DiscountPercent,
                Vat = true, // Luôn bật VAT 5% theo yêu cầu
                Status = 0, // Trạng thái: Chờ (0)
                Note = draft.Note
                // OrderPrice (Giá gốc trước giảm giá/VAT) có thể được tính và lưu nếu cần
            };

            // 2. Thêm OrderItems
            foreach (var itemDraft in draft.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductName = itemDraft.ProductName,
                    Quantity = itemDraft.Quantity,
                    UnitPrice = itemDraft.UnitPrice,
                    // OrderId sẽ được thiết lập tự động khi thêm vào Order.OrderItems
                };
                order.OrderItems.Add(orderItem);
            }

            // 3. Xử lý điểm tích lũy (Giả định: 100.000 VNĐ = 1 điểm, hoặc bạn tự định nghĩa)
            if (customerId.HasValue)
            {
                int pointsEarned = (int)Math.Floor(grandTotal / 100000); // 1 điểm cho mỗi 100k
                _customerService.UpdateLoyaltyPoints(customerId.Value, pointsEarned);
            }

          
            _db.Orders.Add(order);
            _db.SaveChanges();
            return order;
        }
    }
}