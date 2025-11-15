using CafeManagent.dto.request.ProductModuleDTO;
using CafeManagent.Models;
using CafeManagent.Services.Imp.RecipeModule;
using CafeManagent.Services.Interface.RecipeModule;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers.Staffs.RecipeModule
{
    public class RecipeController : Controller
    {
        private readonly IRecipeService _service;
        private readonly CafeManagementContext _context;

        public RecipeController(IRecipeService service, CafeManagementContext context)
        {
            _service = service;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var recipes = await _service.GetRecipeProduct();
            return View(recipes);
        }

        [HttpPost]
        public async Task<IActionResult> FilterRecipeProduct(string Keyword)
        {
            var recipes = await _service.FilterRecipeProduct(Keyword);
            return View("Index", recipes);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(AddProductDTO dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ!";
                return RedirectToAction("Index");
            }

            var success = await _service.AddProductAsync(dto);

            if (success)
                TempData["Success"] = "Thêm sản phẩm thành công!";
            else
                TempData["Error"] = "Tên sản phẩm đã tồn tại!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(int productId, AddProductDTO dto)
        {
            var ok = await _service.EditProductAsync(productId, dto);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var ok = await _service.DeleteProductAsync(id);
            return RedirectToAction("Index");
        }


    }
}
