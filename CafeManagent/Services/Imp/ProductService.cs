using CafeManagent.Models;
using CafeManagent.Services.Interface;

namespace CafeManagent.Services.Imp
{
    public class ProductService : IProductService
    {
        private readonly CafeManagementContext _db;
        public ProductService(CafeManagementContext db) { _db = db; }

        public List<Product> GetAllActive()
        {
            return _db.Products.Where(p => p.Status == true).ToList();
        }
    }
}
