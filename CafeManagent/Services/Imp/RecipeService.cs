using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp
{
    public class RecipeService : IRecipeService
    {
        private readonly CafeManagementContext _context;

        public RecipeService(CafeManagementContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> FilterRecipeProduct(string Keyword)
        {
            return await _context.Products.Where(p=> p.ProductName.Contains(Keyword))
            .Include(p => p.Recipes)
            .ThenInclude(r => r.Ingredient)
            .ToListAsync(); 
        }

        public async Task<List<Product>> GetRecipeProduct()
        {
            return await _context.Products
            .Include(p => p.Recipes)
            .ThenInclude(r => r.Ingredient)
            .ToListAsync();
        }
    }
}
