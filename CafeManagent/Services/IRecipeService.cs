using CafeManagent.dto.request;
using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IRecipeService
    {
        Task<List<Product>> GetRecipeProduct();
        Task<List<Product>> FilterRecipeProduct(string name);

        Task<bool> AddProductAsync(AddProductDTO dto);

    }
}
