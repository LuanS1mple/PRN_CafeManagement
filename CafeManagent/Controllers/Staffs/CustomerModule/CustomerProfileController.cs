using CafeManagent.dto.request.CustomerModuleDTO;
using CafeManagent.dto.request.ProductModuleDTO;
using CafeManagent.dto.response.NotifyModuleDTO;
using CafeManagent.Enums;
using CafeManagent.Hubs;
using CafeManagent.Models;
using CafeManagent.Services.Interface.CustomerModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Controllers.Staffs.CustomerModule
{
    public class CustomerProfileController : Controller
    {
        private readonly ICustomerProfileService _service;
        private readonly CafeManagementContext _context;
        private readonly IHubContext<ResponseHub> _hubContext;

        public CustomerProfileController(
            ICustomerProfileService service,
            CafeManagementContext context,
            IHubContext<ResponseHub> hubContext)
        {
            _service = service;
            _context = context;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Cashier , Barista")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 6)
        {

            var (customers, totalItem) = await _service.GetPageCustomerProfile(page, pageSize);
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItem;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItem / pageSize);
            return View(customers);
        }

        [Authorize(Roles = "Cashier , Barista")]
        [HttpPost]
        public async Task<IActionResult> AddCustomerProfile(AddCustomerProfileDTO dto)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId") ?? 0;
            if (!ModelState.IsValid)
            {
                ResponseHub.SetNotify(staffId, new SystemNotify()
                {
                    IsSuccess = false,
                    Message = NotifyMessage.DU_LIEU_KHONG_HOP_LE.Message
                });

                return RedirectToAction("Index");
            }

            var success = await _service.AddCustomerAsync(dto);

            ResponseHub.SetNotify(staffId, new SystemNotify()
            {
                IsSuccess = success,
                Message = success
                    ? NotifyMessage.THEM_KHACH_HANG_THANH_CONG.Message
                    : NotifyMessage.KHACH_HANG_DA_TON_TAI.Message
            });

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Cashier , Barista")]
        public async Task<IActionResult> FilterCustomerProfile([FromForm] FilterCustomerProfileDTO filter, int page = 1, int pageSize = 6)
        {
            var (customers, totalItem) = await _service.FilterCustomerProfile(filter, page, pageSize);
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItem;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItem / pageSize);
            return View("Index", customers);
        }

        [Authorize(Roles = "Cashier , Barista")]
        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId") ?? 0;
            var result = await _service.DeleteCustomerAsync(id);

            ResponseHub.SetNotify(staffId, new SystemNotify()
            {
                IsSuccess = result,
                Message = result
                    ? NotifyMessage.XOA_KHACH_HANG_THANH_CONG.Message
                    : NotifyMessage.KHACH_HANG_KHONG_TON_TAI.Message
            });
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Cashier , Barista")]
        [HttpPost]
        public async Task<IActionResult> UpdateCustomerProfile([FromForm] UpdateCustomerProfileDTO dto)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId") ?? 0;
            var result = await _service.UpdateCustomerAsync(dto);

            ResponseHub.SetNotify(staffId, new SystemNotify()
            {
                IsSuccess = result,
                Message = result
                    ? NotifyMessage.SUA_KHACH_HANG_THANH_CONG.Message
                    : NotifyMessage.SUA_KHACH_HANG_THAT_BAI.Message
            });
            return RedirectToAction("Index");
        }


    }
}
