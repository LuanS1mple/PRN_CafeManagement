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
            var staff = _context.Staff
                .Include(s => s.Role)
                .FirstOrDefault(s => s.UserName.ToLower().Trim() == username.ToLower().Trim());

            if (staff == null) return null;
            bool verified = BCrypt.Net.BCrypt.Verify(password, staff.Password);

            return verified ? staff : null;
        }
    }
}
