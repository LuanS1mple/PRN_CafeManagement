using CafeManagent.dto.request.CustomerModuleDTO;
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

        public async Task<IActionResult> FilterCustomerProfile([FromForm] FilterCustomerProfileDTO filter, int page = 1, int pageSize = 6)
        {
            var (customers, totalItem) = await _service.FilterCustomerProfile(filter, page, pageSize);
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItem;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItem / pageSize);
            return View("Index", customers);
        }
    }
}
