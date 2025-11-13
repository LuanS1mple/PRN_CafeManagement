using CafeManagent.Models;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
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
            return View("Index",recipes);
        }
    }
}
