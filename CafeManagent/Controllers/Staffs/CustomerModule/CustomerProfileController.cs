using CafeManagent.dto.request.CustomerModuleDTO;
using CafeManagent.dto.request.ProductModuleDTO;
using CafeManagent.Models;
using CafeManagent.Services.Interface.CustomerModule;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers.Staffs.CustomerModule
{
    public class CustomerProfileController : Controller
    {
        private readonly ICustomerProfileService _service;
        private readonly CafeManagementContext _context;

        public CustomerProfileController(ICustomerProfileService service, CafeManagementContext context)
        {
            _service = service;
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 6)
        {

            var (customers, totalItem) = await _service.GetPageCustomerProfile(page, pageSize);
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItem;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItem / pageSize);
            return View(customers);
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomerProfile(AddCustomerProfileDTO dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ!";
                return RedirectToAction("Index");
            }

            var success = await _service.AddCustomerAsync(dto);

            if (success)
                TempData["Success"] = "Thêm khách hàng thành công!";
            else
                TempData["Error"] = "Khách hàng đã tồn tại!";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> FilterCustomerProfile([FromForm] FilterCustomerProfileDTO filter, int page = 1, int pageSize = 6)
        {
            var (customers, totalItem) = await _service.FilterCustomerProfile(filter, page, pageSize);
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItem;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItem / pageSize);
            return View("Index", customers);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var result = await _service.DeleteCustomerAsync(id);

            if (!result)
                return NotFound();

            TempData["Success"] = "Xóa khách hàng thành công";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomerProfile([FromForm] UpdateCustomerProfileDTO dto)
        {
            var result = await _service.UpdateCustomerAsync(dto);

            if (!result)
            {
                TempData["Error"] = "Cập nhật khách hàng thất bại!";
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Cập nhật khách hàng thành công!";
            return RedirectToAction("Index");
        }


    }
}
