using CafeManagent.dto.request.ProductModuleDTO;
using CafeManagent.Models;

namespace CafeManagent.Services.Interface.RecipeModule
{
    public interface IRecipeService
    {
        Task<List<Product>> GetRecipeProduct();
        Task<List<Product>> FilterRecipeProduct(string name);

        Task<bool> AddProductAsync(AddProductDTO dto);

        Task<bool> EditProductAsync(int productId, AddProductDTO dto);
        Task<bool> DeleteProductAsync(int productId);
    }
}
