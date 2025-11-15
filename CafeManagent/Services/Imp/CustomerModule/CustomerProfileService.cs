using CafeManagent.dto.request.CustomerModuleDTO;
using CafeManagent.dto.request.ProductModuleDTO;
using CafeManagent.Models;
using CafeManagent.Services.Interface.CustomerModule;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                return false;


            _context.Customers.Remove(customer);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCustomerAsync(UpdateCustomerProfileDTO dto)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

            if (customer == null)
                return false;

            customer.FullName = dto.FullName;
            customer.Phone = dto.Phone;
            customer.Address = dto.Address;
            customer.LoyaltyPoint = dto.LoyaltyPoint;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddCustomerAsync(AddCustomerProfileDTO dto)
        {
            if (await _context.Customers.AnyAsync(c => c.Phone == dto.Phone)) return false;

            var customer = new Customer()
            {
                FullName = dto.FullName,
                Phone = dto.Phone,
                Address = dto.Address,
                LoyaltyPoint = dto.LoyaltyPoint
            };

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return true;
        }


    }
}
