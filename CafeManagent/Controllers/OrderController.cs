using CafeManagent.dto.order;
using CafeManagent.Hubs;
using CafeManagent.Models;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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

        // Views
        public IActionResult Waiter() // màn hình waiter
        {
            // Waiter quan tâm đến Waiting (0), Preparing (1), Ready (2)
            ViewBag.Waiting = _svc.GetByStatuses(0);
            ViewBag.Preparing = _svc.GetByStatuses(1);
            ViewBag.Ready = _svc.GetByStatuses(2);
            return View();
        }

        public IActionResult Bartender() // màn hình bartender
        {
            // Bartender quan tâm đến Waiting (0) và Preparing (1)
            ViewBag.Waiting = _svc.GetByStatuses(0);
            ViewBag.Preparing = _svc.GetByStatuses(1);
            // Bartender cũng có thể xem đơn hàng đã hoàn thành (3)
            ViewBag.Completed = _svc.GetByStatuses(3);
            return View();
        }

        public IActionResult History()
        {
            var all = _svc.GetAll();
            return View(all);
        }

        public IActionResult Details(int id)
        {
            var o = _svc.GetById(id);
            if (o == null) return NotFound();
            return View(o);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Order order)
        {
            var o = _svc.Add(order);
            // Cần lấy lại đối tượng có Customer/Note cho đủ thông tin
            var oWithDetails = _svc.GetById(o.OrderId);
            var dto = ToDto(oWithDetails!);
            // broadcast new order -> bartender + waiter both update
            await _hub.Clients.All.SendAsync("OrderCreated", dto);
            return Json(new { success = true, order = dto });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var ok = _svc.Cancel(id);
            if (ok) await _hub.Clients.All.SendAsync("OrderCanceled", id);
            return Json(new { success = ok });
        }

        [HttpPost]
        public async Task<IActionResult> StartPreparing(int id)
        {
            var ok = _svc.SetPreparing(id);
            if (ok) await _hub.Clients.All.SendAsync("OrderPreparing", id);
            return Json(new { success = ok });
        }

        // Bartender đánh dấu SẴN SÀNG (Status 2)
        [HttpPost]
        public async Task<IActionResult> MarkReady(int id)
        {
            var ok = _svc.SetReady(id);
            if (ok)
            {
                var o = _svc.GetById(id);

                await _hub.Clients.All.SendAsync("OrderReady", ToDto(o!));
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
                // Gửi thông báo Confirmed (Status 3) tới tất cả (để Waiter xóa khỏi Ready, Bartender thêm vào History)
                await _hub.Clients.All.SendAsync("OrderConfirmed", ToDto(o!));
            }
            return Json(new { success = ok });
        }

        // helper
        private OrderDto ToDto(Order o)
        {
            return new OrderDto
            {
                OrderId = o.OrderId,
                CustomerName = o.Customer?.FullName ?? o.Note ?? "Khách lẻ",
                TotalAmount = o.TotalAmount ?? o.OrderPrice ?? 0,
                Status = o.Status ?? 0,
                StatusText = StatusText(o.Status),
                OrderTime = o.OrderDate?.ToString("HH:mm:ss dd/MM") ?? ""
            };
        }

        private string StatusText(int? s) => s switch
        {
            -1 => "Cancle",
            0 => "Waiting",
            1 => "Preparing",
            2 => "Ready",
            3 => "Completed",
            _ => "Unknown"
        };
    }
}

