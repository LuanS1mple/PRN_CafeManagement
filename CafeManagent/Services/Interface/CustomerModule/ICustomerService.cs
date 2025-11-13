using CafeManagent.Models;

namespace CafeManagent.Services.Interface.CustomerModule
{
    public interface ICustomerService
    {
        Customer? GetByPhone(string phone);
        void UpdateLoyaltyPoints(int customerId, int points);
    }
}
