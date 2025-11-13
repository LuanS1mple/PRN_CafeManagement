using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IProductService
    {
        List<Product> GetAllActive();
    }
}
