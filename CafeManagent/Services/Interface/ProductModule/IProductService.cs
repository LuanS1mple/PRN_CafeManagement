using CafeManagent.Models;

namespace CafeManagent.Services.Interface.ProductModule
{
    public interface IProductService
    {
        List<Product> GetAllActive();
    }
}
