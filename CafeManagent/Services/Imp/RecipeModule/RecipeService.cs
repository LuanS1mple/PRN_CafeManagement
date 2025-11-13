using CafeManagent.dto.request.ProductModuleDTO;
using CafeManagent.Models;
using CafeManagent.Services.Interface.RecipeModule;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp.RecipeModule
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
            return await _context.Products.Where(p => p.ProductName.Contains(Keyword))
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

        public async Task<bool> AddProductAsync(AddProductDTO dto)
        {
            // Kiểm tra trùng tên sản phẩm
            if (await _context.Products.AnyAsync(p => p.ProductName == dto.ProductName))
                return false;

            // ✅ Tạo sản phẩm mới
            var product = new Product
            {
                ProductName = dto.ProductName,
                Price = dto.Price,
                Description = dto.Description,
                CreatedAt = DateTime.Now,
                Status = true
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync(); // để có ProductId

            // ✅ Kiểm tra xem nguyên liệu đã tồn tại chưa
            Ingredient? ingredient = null;

            if (!string.IsNullOrEmpty(dto.IngredientName))
            {
                ingredient = await _context.Ingredients
                    .FirstOrDefaultAsync(i => i.IngredientName == dto.IngredientName);

                if (ingredient == null)
                {
                    ingredient = new Ingredient
                    {
                        IngredientName = dto.IngredientName,
                        Unit = dto.Unit ?? "đơn vị", // tránh null
                        QuantityInStock = 0,
                        Status = true,
                        CreatedAt = DateTime.Now,
                        CostPerUnit = 0
                    };
                    await _context.Ingredients.AddAsync(ingredient);
                    await _context.SaveChangesAsync();
                }

                var recipe = new Recipe
                {
                    ProductId = product.ProductId,
                    IngredientId = ingredient.IngredientId,
                    QuantityNeeded = dto.QuantityNeeded
                };

                await _context.Recipes.AddAsync(recipe);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }

}
