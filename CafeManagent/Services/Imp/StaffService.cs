using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp
{
    public class StaffService : IStaffService
    {
        private readonly CafeManagementContext _context;
        public StaffService(CafeManagementContext context)
        {
            _context = context;
        }
        public Staff? Authentication(string username, string password)
        {
            return _context.Staff.Include(r => r.Role).
                FirstOrDefault(s => s.UserName.ToLower().Equals(username.ToLower()) && s.Password.ToLower().Equals(password.ToLower()));
            //var staff = _context.Staff.FirstOrDefault(s => s.UserName.ToLower().Equals(username.ToLower()) && s.Password.ToLower().Equals(password.ToLower()));
            //if (staff == null) return null;
            //bool verified = BCrypt.Net.BCrypt.Verify(password, staff.Password);
            //return verified ? staff : null; 
        }
    }
}
