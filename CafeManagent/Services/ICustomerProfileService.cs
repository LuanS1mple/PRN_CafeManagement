using CafeManagent.dto.request;
using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface ICustomerProfileService
    {
        Task<(List<Customer> customers, int totalItem)> GetPageCustomerProfile(int page, int pageSize);
        Task<(List<Customer> customers, int totalItem)> FilterCustomerProfile(FilterCustomerProfileDTO dto, int page, int pageSize);
    }
}
