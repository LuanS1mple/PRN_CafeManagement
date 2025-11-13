using CafeManagent.Models;
using CafeManagent.Services.Interface.CustomerModule;

namespace CafeManagent.Services.Imp.CustomerModule
{
    public class CustomerService : ICustomerService
    {
        private readonly CafeManagementContext _db;
        public CustomerService(CafeManagementContext db) { _db = db; }

        public Customer? GetByPhone(string phone)
        {
            return _db.Customers.FirstOrDefault(c => c.Phone == phone);
        }

        public void UpdateLoyaltyPoints(int customerId, int points)
        {
            var customer = _db.Customers.Find(customerId);
            if (customer != null)
            {
                customer.LoyaltyPoint = (customer.LoyaltyPoint ?? 0) + points;
                _db.SaveChanges();
            }
        }
    }
}
