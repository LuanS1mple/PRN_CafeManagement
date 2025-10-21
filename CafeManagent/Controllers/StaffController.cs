using Microsoft.AspNetCore.Mvc;
using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Controllers
{
    public class StaffController : Controller
    {
        private readonly CafeManagementContext _context;
        public StaffController(CafeManagementContext dbContext)
        {
            _context = dbContext;
        }
        public IActionResult Index(string? searchString, string? RoleFilter)
        {
            var staffs = _context.Staff.Include(s => s.Role).ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                staffs = staffs.Where(s => s.FullName.Contains(searchString) || s.Email.Contains(searchString)).ToList();
            }

            if (!string.IsNullOrEmpty(RoleFilter))
            {
                staffs = staffs.Where(s => s.Role.RoleName.Contains(RoleFilter)).ToList();
            }

            ViewBag.Roles = _context.Roles.ToList();

            return View(staffs);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Staff staff)
        {
            if (ModelState.IsValid)
            {
                staff.CreateAt = DateTime.Now;
                _context.Staff.Add(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View("Index", await _context.Staff.Include(s => s.Role).ToListAsync());
        }
    }
}
