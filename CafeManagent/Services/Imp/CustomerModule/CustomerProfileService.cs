using CafeManagent.dto.request.CustomerModuleDTO;
using CafeManagent.Models;
using CafeManagent.Services.Interface.CustomerModule;
using Microsoft.EntityFrameworkCore;
namespace CafeManagent.Services.Imp.CustomerModule
{
    public class CustomerProfileService : ICustomerProfileService
    {
        private readonly CafeManagementContext _context;

        public CustomerProfileService(CafeManagementContext context)
        {
            _context = context;
        }

        public async Task<(List<Customer> customers, int totalItem)> FilterCustomerProfile(FilterCustomerProfileDTO filter, int page, int pageSize)
        {
            var query = _context.Customers.AsQueryable();

            if (filter.Keyword != null)
            {
                query = query.Where(ws => ws.FullName.Contains(filter.Keyword) || ws.Address.Contains(filter.Keyword) || ws.Phone.Contains(filter.Keyword));
            }
            int totalItem = await query.CountAsync();
            var customers = await query
                .OrderByDescending(c => c.CustomerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (customers, totalItem);

        }

        public async Task<(List<Customer> customers, int totalItem)> GetPageCustomerProfile(int page, int pageSize)
        {
            var query = _context.Customers.AsQueryable();
            int totalItem = await query.CountAsync();
            var customers = await query
                .OrderByDescending(c => c.CustomerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (customers, totalItem);
        }


    }
}
