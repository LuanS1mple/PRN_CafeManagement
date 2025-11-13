using CafeManagent.dto.request;
using CafeManagent.Models;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Controllers
{
    public class WorkShiftController : Controller
    {
        private readonly IWorkShiftService _service;
        private readonly CafeManagementContext _context;

        public WorkShiftController(IWorkShiftService service, CafeManagementContext context)
        {
            _service = service;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var shifts = await _service.GetAllWorkShiftsAsync();

            ViewBag.Positions = await _context.Roles
                .Where(r => r.RoleId != 1)
                .Select(r => r.RoleName)
                .Distinct()
                .Where(rn => !string.IsNullOrEmpty(rn))
                .ToListAsync();

            ViewBag.ShiftTypes = await _context.WorkShifts
                .Select(s => s.ShiftName)
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s))
                .ToListAsync();

            ViewBag.Employees = await _context.Staff
                .Where(s => s.RoleId != 1)
                .Select(s => s.FullName)
                .Distinct()
                .Where(n => !string.IsNullOrEmpty(n))
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Now);
            ViewBag.TotalShifts = shifts.Count;
            ViewBag.TotalEmployees = shifts.Select(s => s.Employee).Distinct().Count();
            ViewBag.TodayShifts = shifts.Count(s => s.Date == today);
            ViewBag.TotalHours = shifts.Sum(s => s.TotalHours);

            return View(shifts);
        }

        [HttpPost]
        public async Task<IActionResult> FilterWorkShift([FromForm] FilterWorkShiftDTO filter)
        {
            var shifts = await _service.FilterWorkShiftsAsync(filter);

            ViewBag.Positions = await _context.Roles
                .Where(r => r.RoleId != 1)
                .Select(r => r.RoleName)
                .Distinct()
                .Where(rn => !string.IsNullOrEmpty(rn))
                .ToListAsync();

            ViewBag.ShiftTypes = await _context.WorkShifts
                .Select(s => s.ShiftName)
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s))
                .ToListAsync();

            ViewBag.Employees = await _context.Staff
                .Where(s => s.RoleId != 1)
                .Select(s => s.FullName)
                .Distinct()
                .Where(n => !string.IsNullOrEmpty(n))
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Now);
            ViewBag.TotalShifts = shifts.Count;
            ViewBag.TotalEmployees = shifts.Select(s => s.Employee).Distinct().Count();
            ViewBag.TodayShifts = shifts.Count(s => s.Date == today);
            ViewBag.TotalHours = shifts.Sum(s => s.TotalHours);

            return View("Index", shifts);
        }

        [HttpPost]
        public async Task<IActionResult> AddWorkShift([FromForm] AddWorkShiftDTO dto)
        {
            var (success, message) = await _service.AddWorkShiftAsync(dto);

            if (!success)
            {
                TempData["Error"] = message;
                TempData["ShowError"] = "1";
            }
            else TempData["Success"] = message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWorkShift(int id)
        {
            var (success, message) = await _service.DeleteWorkShiftAsync(id);

            if (!success)
                TempData["Error"] = message;
            else
                TempData["Success"] = message;

            return RedirectToAction("Index");
        }
    }
}
