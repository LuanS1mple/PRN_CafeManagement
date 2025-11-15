using CafeManagent.Models;
using CafeManagent.Services.Interface.CustomerModule;

namespace CafeManagent.Services.Imp.CustomerModule
{
    public class CustomerService : ICustomerService
    {
        private readonly CafeManagementContext _db;
        public CustomerService(CafeManagementContext db) { _db = db; }
        public Customer Add(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }


            customer.LoyaltyPoint = customer.LoyaltyPoint ?? 0;

            _db.Customers.Add(customer);
            _db.SaveChanges();
            return customer;
        }

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
