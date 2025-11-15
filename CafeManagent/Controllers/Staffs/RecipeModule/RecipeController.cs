using CafeManagent.dto.request.ProductModuleDTO;
using CafeManagent.dto.response.NotifyModuleDTO;
using CafeManagent.Enums;
using CafeManagent.Hubs;
using CafeManagent.Models;
using CafeManagent.Services.Imp.RecipeModule;
using CafeManagent.Services.Interface.RecipeModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Controllers.Staffs.RecipeModule
{
    public class RecipeController : Controller
    {
        private readonly IRecipeService _service;
        private readonly CafeManagementContext _context;
        private readonly IHubContext<ResponseHub> _hubContext;

        public RecipeController(IRecipeService service,
                        CafeManagementContext context,
                        IHubContext<ResponseHub> hubContext)
        {
            _service = service;
            _context = context;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Cashier , Barista")]
        public async Task<IActionResult> Index()
        {
            var recipes = await _service.GetRecipeProduct();
            return View(recipes);
        }

        [Authorize(Roles = "Cashier , Barista")]
        [HttpPost]
        public async Task<IActionResult> FilterRecipeProduct(string Keyword)
        {
            var recipes = await _service.FilterRecipeProduct(Keyword);
            return View("Index", recipes);
        }

        [Authorize(Roles = "Cashier , Barista")]
        [HttpPost]
        public async Task<IActionResult> AddProduct(AddProductDTO dto)
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

            var ok = await _service.AddProductAsync(dto);

            ResponseHub.SetNotify(staffId, new SystemNotify()
            {
                IsSuccess = ok,
                Message = ok
                    ? NotifyMessage.THEM_SAN_PHAM_THANH_CONG.Message
                    : NotifyMessage.SAN_PHAM_DA_TON_TAI.Message
            });

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Cashier , Barista")]
        [HttpPost]
        public async Task<IActionResult> EditProduct(int productId, AddProductDTO dto)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId") ?? 0;

            var ok = await _service.EditProductAsync(productId, dto);

            var notify = new SystemNotify()
            {
                IsSuccess = ok,
                Message = NotifyMessage.SUA_SAN_PHAM_THANH_CONG.Message
            };

            ResponseHub.SetNotify(staffId, notify);

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Cashier , Barista")]
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId") ?? 0;

            var ok = await _service.DeleteProductAsync(id);

            ResponseHub.SetNotify(staffId, new SystemNotify()
            {
                IsSuccess = ok,
                Message = ok
                    ? NotifyMessage.XOA_SAN_PHAM_THANH_CONG.Message
                    : NotifyMessage.SAN_PHAM_KHONG_TON_TAI.Message
            });

            return RedirectToAction("Index");
        }


    }
}
