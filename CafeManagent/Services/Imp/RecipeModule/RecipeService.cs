using CafeManagent.dto.request.ProductModuleDTO;
using CafeManagent.mapper;
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
            try
            {
                var product = ProductMapper.FromDTO(dto);
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync(); // có ProductId

                if (dto.Ingredients != null && dto.Ingredients.Count > 0)
                {
                    foreach (var ingDto in dto.Ingredients)
                    {
                        var ingredient = await _context.Ingredients
                            .FirstOrDefaultAsync(i => i.IngredientName == ingDto.IngredientName);

                        if (ingredient == null)
                        {
                            ingredient = ProductMapper.FromDTO(ingDto);
                            await _context.Ingredients.AddAsync(ingredient);
                            await _context.SaveChangesAsync();
                        }

                        var recipe = ProductMapper.FromDTO(product, ingredient, ingDto.QuantityNeeded);
                        await _context.Recipes.AddAsync(recipe);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> EditProductAsync(UpdateProductDTO dto)
        {
            var product = await _context.Products
                .Include(p => p.Recipes)
                .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId);

            if (product == null) return false;

            // Cập nhật thông tin product
            ProductMapper.UpdateProductFromDTO(product, dto);

            // Xóa toàn bộ Recipe cũ
            _context.Recipes.RemoveRange(product.Recipes);
            await _context.SaveChangesAsync();

            // Thêm Recipe mới
            if (dto.Ingredients != null && dto.Ingredients.Count > 0)
            {
                foreach (var ingDto in dto.Ingredients)
                {
                    var ingredient = await _context.Ingredients
                        .FirstOrDefaultAsync(i => i.IngredientName == ingDto.IngredientName);

                    if (ingredient == null)
                    {
                        ingredient = ProductMapper.FromDTO(ingDto);
                        await _context.Ingredients.AddAsync(ingredient);
                        await _context.SaveChangesAsync();
                    }

                    var recipe = ProductMapper.FromDTO(product, ingredient, ingDto.QuantityNeeded);
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
