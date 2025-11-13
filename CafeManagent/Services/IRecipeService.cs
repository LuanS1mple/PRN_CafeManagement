using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IRecipeService
    {
        Task<List<Product>> GetRecipeProduct();
        Task<List<Product>> FilterRecipeProduct(string name);

    }
}
