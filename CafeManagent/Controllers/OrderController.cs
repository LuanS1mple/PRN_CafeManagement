using CafeManagent.dto.order;

using CafeManagent.Hubs;
using CafeManagent.Models;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafeManagent.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _svc;
        private readonly IHubContext<OrderHub> _hub;

        public OrderController(IOrderService svc, IHubContext<OrderHub> hub)
        {
            _svc = svc;
            _hub = hub;
        }

        // --- VIEWS ---

        public IActionResult Waiter() // màn hình Waiter: Chờ (0), Đang chuẩn bị (1), Đã sẵn sàng (2)
        {
            var activeOrders = _svc.GetByStatuses(0)
                 .Concat(_svc.GetByStatuses(1))
                 .Concat(_svc.GetByStatuses(2))
                 .ToList();

            ViewBag.ActiveOrders = ToDto(activeOrders);
            return View();
        }

        public IActionResult Bartender()
        {
            var activeOrders = _svc.GetByStatuses(0)
                 .Concat(_svc.GetByStatuses(1))
                 .ToList();

            ViewBag.ActiveOrders = ToDto(activeOrders);
            return View();
        }

        public IActionResult CompletedHistory()
        {
            var completedOrders = ToDto(_svc.GetByStatuses(3));
            return View(completedOrders);
        }
        public IActionResult BartenderHistory()
        {

            var readyOrders = _svc.GetByStatuses(2);
            var completedOrders = _svc.GetByStatuses(3);
            var historyOrders = readyOrders
                .Concat(completedOrders)
                .ToList();
            var orderDtos = ToDto(historyOrders);
            var sortedHistory = orderDtos
                .OrderBy(o => o.Status)           
                .ThenByDescending(o => o.OrderId) 
                .ToList();
            return View("CompletedHistory", sortedHistory);
        }
        public IActionResult AllHistory(string query, int? statusFilter, string dateFilter)
        {

            var allOrders = _svc.GetAll();
            var filteredOrders = allOrders.AsEnumerable();

            if (!string.IsNullOrEmpty(query))
            {
                string normalizedQuery = query.Trim().ToLower();
                filteredOrders = filteredOrders.Where(o =>
                    (o.Customer?.FullName?.ToLower().Contains(normalizedQuery) ?? false) ||
                    (o.Note?.ToLower().Contains(normalizedQuery) ?? false) ||
                    o.OrderId.ToString().Contains(normalizedQuery));
            }

            // statusFilter: 99 = Tất cả, -2 = Hoàn tiền, -1 = Hủy, 3 = Hoàn thành, 0/1/2 = Đang hoạt động
            if (statusFilter.HasValue && statusFilter != 99)
            {
                filteredOrders = filteredOrders.Where(o => o.Status == statusFilter.Value);
            }

            if (!string.IsNullOrEmpty(dateFilter))
            {
                if (DateTime.TryParse(dateFilter, out DateTime targetDate))
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == targetDate.Date);
                }
            }

            ViewBag.Query = query;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.DateFilter = dateFilter;

            var result = ToDto(filteredOrders.ToList());
            return View(result.OrderByDescending(o => o.OrderId).ToList());
        }

     
        public IActionResult Details(int id)
        {
            string staffRole = HttpContext.Session.GetString("StaffRole");
            var o = _svc.GetById(id);
            if (o == null) return NotFound();
            ViewData["Role"] = staffRole;
            return View(ToDetailsDto(o));
        }


        // --- ACTIONS ---

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Order order)
        {
            var o = _svc.Add(order);
            var oWithDetails = _svc.GetById(o.OrderId);
            var dto = ToDto(oWithDetails!);
            await _hub.Clients.All.SendAsync("OrderCreated", dto);
            return Json(new { success = true, order = dto });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var ok = _svc.Cancel(id);
           
            if (ok)
            {
                var o = _svc.GetById(id);
                if (ok) await _hub.Clients.All.SendAsync("OrderCanceled", ToDto(o!));
                return Json(new { success = ok, order = ToDto(o!) });
            }
            return Json(new { success = ok });
        }

        [HttpPost]
        public async Task<IActionResult> StartPreparing(int id)
        {
            var ok = _svc.SetPreparing(id);
            if (ok)
            {
                var o = _svc.GetById(id);
                await _hub.Clients.All.SendAsync("OrderPreparing", ToDto(o!));
                return Json(new { success = ok, order = ToDto(o!) });
            }
            
            return Json(new { success = ok });
        }

        [HttpPost]
        public async Task<IActionResult> MarkReady(int id)
        {
            var ok = _svc.SetReady(id);
            if (ok)
            {
                var o = _svc.GetById(id);
                await _hub.Clients.All.SendAsync("OrderReady", ToDto(o!));
                return Json(new { success = ok, order = ToDto(o!) });
            }
            return Json(new { success = ok });
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmByWaiter(int id)
        {
            var ok = _svc.ConfirmDelivered(id);
            if (ok)
            {
                var o = _svc.GetById(id);
                await _hub.Clients.All.SendAsync("OrderConfirmed", ToDto(o!));
                return Json(new { success = ok, order = ToDto(o!) });
            }
            return Json(new { success = ok });
        }

        [HttpPost]
        public async Task<IActionResult> Refund(int id)
        {
            var ok = _svc.SetRefunded(id);

            if (ok)
            {
                var o = _svc.GetById(id);
                await _hub.Clients.All.SendAsync("OrderRefunded", ToDto(o!));
                return Json(new { success = ok, order = ToDto(o!) });
            }
            return Json(new { success = ok });
        }


        [HttpGet]
        public IActionResult SearchHistoryApi(string query, int? statusFilter, string dateFilter)
        {
            var allOrders = _svc.GetAll();
            var filteredOrders = allOrders.AsEnumerable();

            if (!string.IsNullOrEmpty(query))
            {
                string normalizedQuery = query.Trim().ToLower();
                filteredOrders = filteredOrders.Where(o =>
                    (o.Customer?.FullName?.ToLower().Contains(normalizedQuery) ?? false) ||
                    (o.Note?.ToLower().Contains(normalizedQuery) ?? false) ||
                    o.OrderId.ToString().Contains(normalizedQuery));
            }

            if (statusFilter.HasValue && statusFilter != 99)
            {
                filteredOrders = filteredOrders.Where(o => o.Status == statusFilter.Value);
            }
            if (!string.IsNullOrEmpty(dateFilter))
            {
                if (DateTime.TryParse(dateFilter, out DateTime targetDate))
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == targetDate.Date);
                }
            }
            var result = ToDto(filteredOrders.ToList())
                                 .OrderByDescending(o => o.OrderId)
                                 .ToList();

            return Json(new { success = true, orders = result });
        }


        // --- HELPERS ---

        private List<OrderDto> ToDto(IEnumerable<Order> orders)
        {
            return orders.Select(ToDto).ToList();
        }

        private OrderDto ToDto(Order o)
        {
            string customerName = o.Customer?.FullName;

            if (string.IsNullOrEmpty(customerName))
            {
                customerName = "Khách vãng lai";
            }

            return new OrderDto
            {
                OrderId = o.OrderId,
                CustomerName = customerName,
                TotalAmount = o.TotalAmount ?? o.OrderPrice ?? 0,
                Status = o.Status ?? 0,
                StatusText = StatusText(o.Status),
                OrderTime = o.OrderDate?.ToString("HH:mm:ss dd/MM") ?? ""
            };
        }
        // Trong OrderController.cs (Thêm hàm này vào phần HELPERS)

        private OrderDetailsDto ToDetailsDto(Order o)
        {
            // Lấy thông tin cơ bản từ hàm ToDto hiện tại
            var baseDto = ToDto(o);

            // Chuyển đổi chi tiết các món hàng
            var itemsDto = o.OrderItems.Select(item => new OrderItemDto
            {
                ProductName = item.ProductName,
                Quantity = item.Quantity ?? 0,
                UnitPrice = item.UnitPrice ?? 0
            }).ToList();

            return new OrderDetailsDto
            {
                // Copy các trường từ baseDto
                OrderId = baseDto.OrderId,
                CustomerName = baseDto.CustomerName,
                TotalAmount = baseDto.TotalAmount,
                Status = baseDto.Status,
                StatusText = baseDto.StatusText,
                OrderTime = baseDto.OrderTime,

                // Thêm chi tiết
                CustomerPhone = o.Customer?.Phone ?? "N/A",
                Note = o.Note ?? "Không có ghi chú.",
                Discount = o.Discount ?? 0,
                Vat = o.Vat ?? false,
                Items = itemsDto
            };
        }
        [HttpGet]
        public IActionResult SearchActiveOrdersApi(string query, [FromQuery] List<int> statusFilter, string dateFilter)
        {
            var allOrders = _svc.GetAll();
            var filteredOrders = allOrders.AsEnumerable();

            // Lọc theo TRẠNG THÁI: Sử dụng danh sách statusFilter
            if (statusFilter != null && statusFilter.Any())
            {
                filteredOrders = filteredOrders.Where(o => o.Status.HasValue && statusFilter.Contains(o.Status.Value));
            }

            // Lọc theo TÌM KIẾM (Query)
            if (!string.IsNullOrEmpty(query))
            {
                string normalizedQuery = query.Trim().ToLower();
                filteredOrders = filteredOrders.Where(o =>
                    (o.Customer?.FullName?.ToLower().Contains(normalizedQuery) ?? false) ||
                    (o.Note?.ToLower().Contains(normalizedQuery) ?? false) ||
                    o.OrderId.ToString().Contains(normalizedQuery));
            }

            // Lọc theo ngày (YYYY-MM-DD)
            if (!string.IsNullOrEmpty(dateFilter))
            {
                if (DateTime.TryParse(dateFilter, out DateTime targetDate))
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == targetDate.Date);
                }
            }

            // Chuyển sang DTO và Sắp xếp (Status cao hơn lên trước: 2 > 1 > 0)
            var result = ToDto(filteredOrders.ToList())
                                 .OrderByDescending(o => o.Status)
                                 .ToList();

            return Json(new { success = true, orders = result });
        }

        private string StatusText(int? s) => s switch
        {
            -2 => "Đã hoàn tiền",
            -1 => "Đã hủy",
            0 => "Chờ",
            1 => "Đang chuẩn bị",
            2 => "Đã sẵn sàng",
            3 => "Hoàn thành",
            _ => "Không rõ"
        };





    }
}