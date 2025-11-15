using CafeManagent.dto.request.OrderModuleDTO;
using CafeManagent.dto.response.NotifyModuleDTO;
using CafeManagent.dto.response.OrderModuleDTO;
using CafeManagent.Enums;
using CafeManagent.Helpers;
using CafeManagent.Hubs;
using CafeManagent.mapper;
using CafeManagent.Models;
using CafeManagent.Services.Interface;
using CafeManagent.Services.Interface.CustomerModule;
using CafeManagent.Services.Interface.OrderModule;
using CafeManagent.Services.Interface.ProductModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafeManagent.Controllers.Staffs.OrderModule
{
    public class OrderController : Controller
    {
        private const string SESSION_KEY_DRAFT = "OrderDraft";
        
        private readonly IOrderService _svc;
        private readonly IHubContext<OrderHub> _hub;
        private readonly IProductService _productSvc; 
        private readonly ICustomerService _customerSvc;
        private readonly IVnPayService _vnpaySvc;
        public OrderController(IOrderService svc, IHubContext<OrderHub> hub, IProductService productSvc, ICustomerService customerSvc, IVnPayService vnpaySvc)
        {
            _svc = svc;
            _hub = hub;
            _productSvc = productSvc;
            _customerSvc = customerSvc;
            _vnpaySvc = vnpaySvc;

        }

        //view
        [Authorize(Roles = "Cashier")]
        public IActionResult Waiter() 
        {
            var activeOrders = _svc.GetByStatuses(0)
                 .Concat(_svc.GetByStatuses(1))
                 .Concat(_svc.GetByStatuses(2))
                 .ToList();

            ViewBag.ActiveOrders = ToDto(activeOrders);
            return View();
        }
        [Authorize(Roles = "Barista")]
        public IActionResult Bartender()
        {
            var activeOrders = _svc.GetByStatuses(0)
                 .Concat(_svc.GetByStatuses(1))
                 .ToList();

            ViewBag.ActiveOrders = ToDto(activeOrders);
            return View();
        }
        [Authorize(Roles = "Cashier")]
        public IActionResult AddOrder()
        {
            var draftDetails = HttpContext.Session.GetObject<OrderDraftDetailsDto>(SESSION_KEY_DRAFT);
            ViewBag.DraftDetails = draftDetails;
            ViewBag.Products = _productSvc.GetAllActive();

            return View();
        }

        [Authorize(Roles = "Barista")]
        public IActionResult CompletedHistory()
        {
            var completedOrders = ToDto(_svc.GetByStatuses(3));
            return View(completedOrders);
        }
        [Authorize(Roles = "Barista")]
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



        [HttpPost]
        public IActionResult CalculateDraftApi([FromBody] OrderDraftDto draft)
        {
            if (draft == null)
            {
                return Json(new { success = false, message = "Dữ liệu đơn hàng trống." });
            }
            decimal subtotal = draft.Items.Sum(i => i.Quantity * i.UnitPrice);
            decimal discountPercentage = draft.DiscountPercent / 100m;
            decimal discountAmount = subtotal * discountPercentage;

            decimal totalBeforeVAT = subtotal - discountAmount;
            const decimal VAT_PERCENT = 0.05m;
            decimal vatAmount = totalBeforeVAT * VAT_PERCENT;
            decimal grandTotal = totalBeforeVAT + vatAmount;
            Customer? customer = null;
            string customerStatus = "Khách vãng lai";
            int pointsEarned = 0;
            int customerFoundStatus = 0;

            if (!string.IsNullOrEmpty(draft.CustomerPhone) && draft.CustomerPhone.Length >= 10)
            {

                customer = _customerSvc.GetByPhone(draft.CustomerPhone);
                const decimal POINTS_PER_VND = 1000m;
                pointsEarned = (int)Math.Floor(grandTotal / POINTS_PER_VND);

                if (customer != null)
                {
                    customerFoundStatus = 1;

                    customerStatus = $"<span class='text-success fw-bold'>Khách thân thiết: {customer.FullName}</span> (Điểm hiện có: {customer.LoyaltyPoint ?? 0})";
                }
                else
                {
                    customerFoundStatus = 2; 
                    customerStatus = $"<span class='text-info fw-bold'>SĐT mới ({draft.CustomerPhone})</span>. Sẽ nhận {pointsEarned} điểm.";
                }
            }
            else
            {
                customerFoundStatus = 0;
                customerStatus = "Chưa nhập SĐT/SĐT không hợp lệ.";
                pointsEarned = 0; 
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    Subtotal = subtotal,
                    DiscountAmount = discountAmount,
                    VATAmount = vatAmount,
                    GrandTotal = grandTotal,
                    CustomerName = customer?.FullName ?? "N/A",
                    CustomerPhone = draft.CustomerPhone ?? "N/A",
                    CustomerStatus = customerStatus,
                    PointsEarned = pointsEarned,
                    CustomerFoundStatus = customerFoundStatus 
                }
            });
        }

        [HttpPost]
        public IActionResult CreateDraft([FromBody] OrderDraftDto draft)
        {
            if (draft == null || !draft.Items.Any())
            {
                return Json(new { success = false, message = "Không có sản phẩm trong đơn hàng." });
            }

            try
            {
                decimal subtotal = draft.Items.Sum(i => i.Quantity * i.UnitPrice);
                decimal discountPercentage = draft.DiscountPercent / 100m;
                decimal discountAmount = subtotal * discountPercentage;
                decimal totalBeforeVAT = subtotal - discountAmount;
                decimal vatAmount = totalBeforeVAT * 0.05m;
                decimal grandTotal = totalBeforeVAT + vatAmount;

                Customer? customer = string.IsNullOrEmpty(draft.CustomerPhone) ? null : _customerSvc.GetByPhone(draft.CustomerPhone);
                const decimal POINTS_PER_VND = 1000m;
                int pointsEarned = (int)Math.Floor(grandTotal / POINTS_PER_VND);
                if (!string.IsNullOrEmpty(draft.CustomerPhone) && draft.CustomerPhone.Length >= 10)
                {
                    customer = _customerSvc.GetByPhone(draft.CustomerPhone);
                    if (customer == null && !string.IsNullOrEmpty(draft.NewCustomerName))
                    {
                        customer = new Customer
                        {
                            CustomerId = 0,
                            FullName = draft.NewCustomerName,
                            Phone = draft.CustomerPhone,
                            LoyaltyPoint = 0
                        };
                    }
                }
                var draftData = new OrderDraftDetailsDto
                {
                    Draft = draft,
                    Subtotal = Math.Round(subtotal, 0),
                    DiscountAmount = Math.Round(discountAmount, 0),
                    TotalBeforeVAT = Math.Round(totalBeforeVAT, 0),
                    VATAmount = Math.Round(vatAmount, 0),
                    GrandTotal = Math.Round(grandTotal, 0),
                    Customer = customer,
                    PointsEarned = pointsEarned
                };
                HttpContext.Session.SetObject(SESSION_KEY_DRAFT, draftData);
                return Json(new { success = true, redirectUrl = Url.Action("DraftView") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi hệ thống khi tạo nháp: {ex.Message}" });
            }
        }
        [Authorize(Roles = "Cashier")]
        public IActionResult DraftView()
        {
            var draftDetails = HttpContext.Session.GetObject<OrderDraftDetailsDto>(SESSION_KEY_DRAFT);

            if (draftDetails == null)
            {
                return RedirectToAction("AddOrder");
            }
            return View(draftDetails); 
        }
      
        [HttpPost]
        [Authorize(Roles = "Cashier")]
        public IActionResult CancelDraft()
        {
            return RedirectToAction("AddOrder");
        }

        [HttpPost]
        [Authorize(Roles = "Cashier")]
        public IActionResult CancelCompleteDraft()
        {
            HttpContext.Session.Remove(SESSION_KEY_DRAFT);
            return RedirectToAction("AddOrder");
        }
        
        [HttpPost]
        [Authorize(Roles = "Cashier")]
        public async Task<IActionResult> CompletePayment(string paymentMethod)
        {
            var draftDetails = HttpContext.Session.GetObject<OrderDraftDetailsDto>(SESSION_KEY_DRAFT);

            if (draftDetails == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bản nháp để hoàn tất thanh toán." });
            }

            int pointsEarned = draftDetails.PointsEarned;
            int? customerId = draftDetails.Customer?.CustomerId;

            try
            {
                if(draftDetails.Customer!=null && draftDetails.Customer.CustomerId == 0)
                {
                    var newCustomer = new Customer
                    {
                        FullName = draftDetails.Customer.FullName,
                        Phone = draftDetails.Customer.Phone,
                        LoyaltyPoint = 0
                    };
                    var savedCustomer = _customerSvc.Add(newCustomer);
                    customerId = savedCustomer.CustomerId;
                }
                int? staffId = HttpContext.Session.GetInt32("StaffId");
                var newOrder = DraftOrderMapper.MapDraftToOrder(
                    draftDetails,
                    paymentMethod,
                    status: 0,
                    staffId: staffId,
                    newCustomerId: customerId
                );
                var savedOrder = _svc.Add(newOrder);
                if (customerId.HasValue && pointsEarned > 0)
                {
                    _customerSvc.UpdateLoyaltyPoints(customerId.Value, pointsEarned);
                }


                HttpContext.Session.Remove(SESSION_KEY_DRAFT);
                int STAFF_ID = HttpContext.Session.GetInt32("StaffId") ?? 0;
                SystemNotify systemNotify = new SystemNotify()
                {
                    IsSuccess = true,
                    Message = NotifyMessage.CREATE_ORDER.Message
                };
                ResponseHub.SetNotify(STAFF_ID, systemNotify);
                await _hub.Clients.All.SendAsync("OrderCreated", ToDto(savedOrder));
                return Json(new
                {
                    success = true,
                   
                    redirectUrl = Url.Action("Waiter", "Order", new { id = savedOrder.OrderId })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi lưu DB hoặc cập nhật điểm: {ex.Message}" });
            }
        }
        [Authorize(Roles = "Cashier")]
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

        //action status
        [Authorize(Roles = "Cashier")]
        public IActionResult Details(int id)
        {
            string staffRole = HttpContext.Session.GetString("StaffRole");
            var o = _svc.GetById(id);
            if (o == null) return NotFound();
            ViewData["Role"] = staffRole;
            return View(ToDetailsDto(o));
        }


        //[HttpPost]
        //public async Task<IActionResult> Create([FromForm] Order order)
        //{
        //    var o = _svc.Add(order);
        //    var oWithDetails = _svc.GetById(o.OrderId);
        //    var dto = ToDto(oWithDetails!);
        //    await _hub.Clients.All.SendAsync("OrderCreated", dto);
        //    return Json(new { success = true, order = dto });
        //}

        [HttpPost]
        [Authorize(Roles = "Cashier")]
        public async Task<IActionResult> Cancel(int id)
        {
            var ok = _svc.Cancel(id);

            if (ok)
            {
                var o = _svc.GetById(id);

                if (o != null)
                {
                    if (o.CustomerId.HasValue )
                    {
                        int pointsEarned = (int)Math.Floor(o.TotalAmount.Value / 1000m);
                        _customerSvc.UpdateLoyaltyPoints(o.CustomerId.Value, -pointsEarned);
                    }
                    int STAFF_ID = HttpContext.Session.GetInt32("StaffId") ?? 0;
                    SystemNotify systemNotify = new SystemNotify()
                    {
                        IsSuccess = true,
                        Message = NotifyMessage.UPDATE_STATUS_ORDER.Message
                    };
                    ResponseHub.SetNotify(STAFF_ID, systemNotify);
                    await _hub.Clients.All.SendAsync("OrderCanceled", ToDto(o!));
                    return Json(new { success = ok, order = ToDto(o!) });
                }
            }

            return Json(new { success = ok, message = ok ? "Đã hủy đơn hàng nhưng không tìm thấy dữ liệu để trả về." : "Lỗi hủy đơn hàng trong Service." });
        }

        [HttpPost]
        [Authorize(Roles = "Cashier")]
        public async Task<IActionResult> StartPreparing(int id)
        {
            var ok = _svc.SetPreparing(id);
            if (ok)
            {
                var o = _svc.GetById(id);
                SystemNotify systemNotify = new SystemNotify()
                {
                    IsSuccess = true,
                    Message = NotifyMessage.UPDATE_STATUS_ORDER.Message
                };
                int STAFF_ID = HttpContext.Session.GetInt32("StaffId") ?? 0;
                ResponseHub.SetNotify(STAFF_ID, systemNotify);
                await _hub.Clients.All.SendAsync("OrderPreparing", ToDto(o!));
                return Json(new { success = ok, order = ToDto(o!) });
            }
            
            return Json(new { success = ok });
        }

        [HttpPost]
        [Authorize(Roles = "Cashier")]
        public async Task<IActionResult> MarkReady(int id)
        {
            var ok = _svc.SetReady(id);
            if (ok)
            {
                var o = _svc.GetById(id);
                SystemNotify systemNotify = new SystemNotify()
                {
                    IsSuccess = true,
                    Message = NotifyMessage.UPDATE_STATUS_ORDER.Message
                };
                int STAFF_ID = HttpContext.Session.GetInt32("StaffId") ?? 0;
                ResponseHub.SetNotify(STAFF_ID, systemNotify);
                await _hub.Clients.All.SendAsync("OrderReady", ToDto(o!));
                return Json(new { success = ok, order = ToDto(o!) });
            }
            return Json(new { success = ok });
        }


        [HttpPost]
        [Authorize(Roles = "Cashier")]
        public async Task<IActionResult> ConfirmByWaiter(int id)
        {
            var ok = _svc.ConfirmDelivered(id);
            if (ok)
            {
                var o = _svc.GetById(id);
                SystemNotify systemNotify = new SystemNotify()
                {
                    IsSuccess = true,
                    Message = NotifyMessage.UPDATE_STATUS_ORDER.Message
                };
                int STAFF_ID = HttpContext.Session.GetInt32("StaffId") ?? 0;
                ResponseHub.SetNotify(STAFF_ID, systemNotify);
                await _hub.Clients.All.SendAsync("OrderConfirmed", ToDto(o!));
                return Json(new { success = ok, order = ToDto(o!) });
            }
            return Json(new { success = ok });
        }

        [HttpPost]
        [Authorize(Roles = "Cashier")]
        public async Task<IActionResult> Refund(int id)
        {
            var ok = _svc.SetRefunded(id);

            if (ok)
            {
                var o = _svc.GetById(id);
                SystemNotify systemNotify = new SystemNotify()
                {
                    IsSuccess = true,
                    Message = NotifyMessage.UPDATE_STATUS_ORDER.Message
                };
                int STAFF_ID = (int)HttpContext.Session.GetInt32("StaffId");
                ResponseHub.SetNotify(STAFF_ID, systemNotify);
                await _hub.Clients.All.SendAsync("OrderRefunded", ToDto(o!));
                return Json(new { success = ok, order = ToDto(o!) });
            }
            return Json(new { success = ok });
        }
        //Search


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
        [HttpGet]
        public IActionResult SearchActiveOrdersApi(string query, [FromQuery] List<int> statusFilter, string dateFilter)
        {
            var allOrders = _svc.GetAll();
            var filteredOrders = allOrders.AsEnumerable();
            if (statusFilter != null && statusFilter.Any())
            {
                filteredOrders = filteredOrders.Where(o => o.Status.HasValue && statusFilter.Contains(o.Status.Value));
            }
            if (!string.IsNullOrEmpty(query))
            {
                string normalizedQuery = query.Trim().ToLower();
                filteredOrders = filteredOrders.Where(o =>
                    (o.Customer?.FullName?.ToLower().Contains(normalizedQuery) ?? false) ||
                    (o.Note?.ToLower().Contains(normalizedQuery) ?? false) ||
                    o.OrderId.ToString().Contains(normalizedQuery));
            }
            if (!string.IsNullOrEmpty(dateFilter))
            {
                if (DateTime.TryParse(dateFilter, out DateTime targetDate))
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == targetDate.Date);
                }
            }

            var result = ToDto(filteredOrders.ToList())
                                 .OrderByDescending(o => o.Status)
                                 .ToList();

            return Json(new { success = true, orders = result });
        }

        [HttpPost]

        public IActionResult ProcessVnPay()
        {
            var draftDetails = HttpContext.Session.GetObject<OrderDraftDetailsDto>(SESSION_KEY_DRAFT);

            if (draftDetails == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bản nháp để xử lý VNPay." });
            }

            try
            {
                int? customerId = draftDetails.Customer?.CustomerId;
                if (draftDetails.Customer != null && draftDetails.Customer.CustomerId == 0)
                {
                    var newCustomer = new Customer
                    {
                        FullName = draftDetails.Customer.FullName,
                        Phone = draftDetails.Customer.Phone,
                        LoyaltyPoint = 0
                    };
                    var savedCustomer = _customerSvc.Add(newCustomer);
                    customerId = savedCustomer.CustomerId;
                }
                int? staffId = HttpContext.Session.GetInt32("StaffId");
                var tempOrder = DraftOrderMapper.MapDraftToOrder(
                    draftDetails,
                    paymentMethod: "Online",
                    status: 0, 
                    staffId: staffId,
                    newCustomerId: customerId
                );

                var savedTempOrder = _svc.Add(tempOrder);
                HttpContext.Session.SetInt32("ProcessingOrderId", savedTempOrder.OrderId);

                string paymentUrl = _vnpaySvc.CreatePaymentUrl(
                    savedTempOrder.OrderId.ToString(),
                    savedTempOrder.TotalAmount ?? 0,
                    HttpContext
                );

                return Json(new { success = true, redirectUrl = paymentUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi tạo URL VNPay: {ex.Message}" });
            }
        }
        [HttpGet]
        public IActionResult CheckOrderStatusApi(int id)
        {
            // 1. Tìm đơn hàng theo ID
            var order = _svc.GetById(id);

            if (order == null)
            {

                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy đơn hàng.",
                    orderId = id,
                    status = -1
                });
            }

            return Json(new
            {
                success = true,
                orderId = order.OrderId,
                status = order.Status ?? 0, // Trạng thái hiện tại
                statusText = StatusText(order.Status),
                totalAmount = order.TotalAmount ?? 0,
                orderTime = order.OrderDate?.ToString("HH:mm:ss dd/MM") ?? ""
            });
        }


        [HttpGet]
        public async Task<IActionResult> VnPayReturn()
        {
            // Lấy thông tin phản hồi từ query string
            var response = _vnpaySvc.ProcessVnPayReturn(Request.Query);

            // Lấy OrderId tạm thời và Draft Details
            int? tempOrderId = HttpContext.Session.GetInt32("ProcessingOrderId");
            var draftDetails = HttpContext.Session.GetObject<OrderDraftDetailsDto>(SESSION_KEY_DRAFT);

            // Xóa ProcessingOrderId ngay sau khi lấy
            HttpContext.Session.Remove("ProcessingOrderId");

            if (tempOrderId == null || draftDetails == null)
            {
                // Nếu không tìm thấy ID tạm hoặc Draft, chuyển hướng.
                return RedirectToAction("CompletedHistory");
            }

            if (response.ResponseCode == "00" && response.IsSuccess)
            {
   
                try
                {
                    _svc.Cancel(tempOrderId.Value);
                    int? customerId = draftDetails.Customer?.CustomerId;
                    int pointsEarned = draftDetails.PointsEarned;

                    if (draftDetails.Customer != null && draftDetails.Customer.CustomerId == 0)
                    {
                        var newCustomer = new Customer
                        {
                            FullName = draftDetails.Customer.FullName,
                            Phone = draftDetails.Customer.Phone,
                            LoyaltyPoint = 0
                        };
                        var savedCustomer = _customerSvc.Add(newCustomer);
                        customerId = savedCustomer.CustomerId;
                    }

                    // Map Draft sang Order Model (Status 0: Chờ)
                    int? staffId = HttpContext.Session.GetInt32("StaffId");
                    var newOrder = DraftOrderMapper.MapDraftToOrder(
                        draftDetails,
                        paymentMethod: "Online", // Xác định phương thức thanh toán
                        status: 0, // Trạng thái ban đầu: Chờ
                        staffId: staffId,
                        newCustomerId: customerId
                    );
                    var savedOrder = _svc.Add(newOrder); // TẠO ORDER MỚI CUỐI CÙNG

                    // 3. CẬP NHẬT ĐIỂM TÍCH LŨY (Vì thanh toán đã xong)
                    if (customerId.HasValue && pointsEarned > 0)
                    {
                        _customerSvc.UpdateLoyaltyPoints(customerId.Value, pointsEarned);
                    }

                    // 4. Xóa Session Draft
                    HttpContext.Session.Remove(SESSION_KEY_DRAFT);

                    // 5. Gửi SignalR thông báo đơn hàng mới
                    int STAFF_ID = HttpContext.Session.GetInt32("StaffId") ?? 0;
                    SystemNotify systemNotify = new SystemNotify()
                    {
                        IsSuccess = true,
                        Message = NotifyMessage.CREATE_ORDER.Message
                    };
                    ResponseHub.SetNotify(STAFF_ID, systemNotify);
                    await _hub.Clients.All.SendAsync("OrderCreated", ToDto(savedOrder));

                    // 6. CHUYỂN HƯỚNG VỀ TRANG WAITER/DETAILS CỦA ĐƠN HÀNG MỚI
                    // Hành động này sẽ được thực hiện khi VNPay Redirect trình duyệt người dùng
                    return RedirectToAction("Details", new { id = savedOrder.OrderId, success = true });
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi trong quá trình lưu DB (nên hủy đơn hàng tạm để tránh rác DB)
                    _svc.Cancel(tempOrderId.Value);
                    TempData["VnPayError"] = $"Thanh toán VNPay thành công nhưng lỗi lưu DB: {ex.Message}";
                    return RedirectToAction("AddOrder");
                }
            }
            else
            {
                // THANH TOÁN THẤT BẠI: Hủy đơn hàng tạm và chuyển hướng về trang tạo đơn
                _svc.Cancel(tempOrderId.Value);
                TempData["VnPayError"] = response.Message;
                return RedirectToAction("AddOrder");
            }
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
    

        private OrderDetailsDto ToDetailsDto(Order o)
        {
            var baseDto = ToDto(o);
            var itemsDto = o.OrderItems.Select(item => new OrderItemDto
            {
                ProductName = item.ProductName,
                Quantity = item.Quantity ?? 0,
                UnitPrice = item.UnitPrice ?? 0
            }).ToList();
            return new OrderDetailsDto
            {
                OrderId = baseDto.OrderId,
                CustomerName = baseDto.CustomerName,
                TotalAmount = baseDto.TotalAmount,
                Status = baseDto.Status,
                StatusText = baseDto.StatusText,
                OrderTime = baseDto.OrderTime,
                CustomerPhone = o.Customer?.Phone ?? "N/A",
                Note = o.Note ?? "Không có ghi chú.",
                Discount = o.Discount ?? 0,
                Vat = o.Vat ?? false,
                Items = itemsDto
            };
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