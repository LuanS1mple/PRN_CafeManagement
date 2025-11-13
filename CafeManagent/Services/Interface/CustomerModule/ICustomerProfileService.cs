using CafeManagent.dto.request.CustomerModuleDTO;
using CafeManagent.Models;

namespace CafeManagent.Services.Interface.CustomerModule
{
    public interface ICustomerProfileService
    {
        Task<(List<Customer> customers, int totalItem)> GetPageCustomerProfile(int page, int pageSize);
        Task<(List<Customer> customers, int totalItem)> FilterCustomerProfile(FilterCustomerProfileDTO dto, int page, int pageSize);
    }
}
