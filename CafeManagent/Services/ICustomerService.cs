using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface ICustomerService
    {
        Customer? GetByPhone(string phone); 
        void UpdateLoyaltyPoints(int customerId, int points); 
    }
}
