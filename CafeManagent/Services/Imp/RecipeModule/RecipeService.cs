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
            // Kiểm tra trùng tên
            if (await _context.Products.AnyAsync(p => p.ProductName == dto.ProductName))
                return false;

            var product = new Product
            {
                ProductName = dto.ProductName,
                Price = dto.Price,
                Description = dto.Description,
                CreatedAt = DateTime.Now,
                Status = true
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync(); // cần ProductId

            // ---------- ADD NGUYÊN LIỆU -----------
            if (dto.Ingredients != null && dto.Ingredients.Count > 0)
            {
                foreach (var ing in dto.Ingredients)
                {
                    // Tìm hoặc tạo nguyên liệu
                    var ingredient = await _context.Ingredients
                        .FirstOrDefaultAsync(i => i.IngredientName == ing.IngredientName);

                    if (ingredient == null)
                    {
                        ingredient = new Ingredient
                        {
                            IngredientName = ing.IngredientName,
                            Unit = ing.Unit,
                            QuantityInStock = 0,
                            Status = true,
                            CreatedAt = DateTime.Now,
                            CostPerUnit = 0
                        };

                        await _context.Ingredients.AddAsync(ingredient);
                        await _context.SaveChangesAsync();
                    }

                    // Tạo dòng recipe
                    var recipe = new Recipe
                    {
                        ProductId = product.ProductId,
                        IngredientId = ingredient.IngredientId,
                        QuantityNeeded = ing.QuantityNeeded
                    };

                    await _context.Recipes.AddAsync(recipe);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> EditProductAsync(int productId, AddProductDTO dto)
        {
            var product = await _context.Products
                .Include(p => p.Recipes)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) return false;

            // ===== Cập nhật thông tin sản phẩm =====
            product.ProductName = dto.ProductName;
            product.Price = dto.Price;
            product.Description = dto.Description;

            // ===== Xóa toàn bộ Recipe cũ ======
            _context.Recipes.RemoveRange(product.Recipes);
            await _context.SaveChangesAsync();

            // ===== Thêm Recipe mới =====
            if (dto.Ingredients != null)
            {
                foreach (var ing in dto.Ingredients)
                {
                    // Tìm hoặc tạo nguyên liệu
                    var ingredient = await _context.Ingredients
                        .FirstOrDefaultAsync(i => i.IngredientName == ing.IngredientName);

                    if (ingredient == null)
                    {
                        ingredient = new Ingredient
                        {
                            IngredientName = ing.IngredientName,
                            Unit = ing.Unit,
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
                        ProductId = productId,
                        IngredientId = ingredient.IngredientId,
                        QuantityNeeded = ing.QuantityNeeded
                    };

                    await _context.Recipes.AddAsync(recipe);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Recipes)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) return false;

            // Xóa công thức trước
            _context.Recipes.RemoveRange(product.Recipes);

            // Xóa sản phẩm
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
            return true;
        }


    }



}
