using CafeManagent.Models;

namespace CafeManagent.Services.Interface
{
    public interface IProductService
    {
        List<Product> GetAllActive();
    }
}
