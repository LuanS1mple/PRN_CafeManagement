using CafeManagent.dto.request;
using CafeManagent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Controllers
{
    public class WorkShiftController : Controller
    {
        private readonly CafeManagementContext _context;

        public WorkShiftController(CafeManagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // Lấy danh sách ca làm chi tiết
            var shifts = await _context.WorkSchedules
                .Include(ws => ws.Workshift)
                .Include(ws => ws.Attendances)
                    .ThenInclude(a => a.Staff)
                    .ThenInclude(s => s.Role)
                .SelectMany(ws => ws.Attendances.Select(a => new
                {
                    Employee = a.Staff.FullName, // hoặc ShiftName tùy bạn muốn
                    Date = ws.Date,
                    StartTime = ws.Workshift.StartTime,
                    EndTime = ws.Workshift.EndTime,
                    Position = a.Staff.Role.RoleName,
                    ShiftType = ws.Workshift.ShiftName,
                    TotalHours = (ws.Workshift.EndTime.Value - ws.Workshift.StartTime.Value).TotalHours

                }))
                .ToListAsync();



            // 🟢 Lấy danh sách vị trí và loại ca để hiển thị trong filter
            var positions = await _context.Roles
                .Select(r => r.RoleName)
                .Distinct()
                .Where(rn => !string.IsNullOrEmpty(rn))
                .ToListAsync();

            var shiftTypes = await _context.WorkShifts
                .Select(s => s.ShiftName)
                .Distinct()
                .Where(s => s != null && s != "")
                .ToListAsync();

            // Thống kê
            ViewBag.TotalShifts = shifts.Count;
            ViewBag.TotalEmployees = shifts.Select(s => s.Employee).Distinct().Count();
            ViewBag.TodayShifts = shifts.Count(s => s.Date == today);
            ViewBag.TotalHours = shifts.Sum(s => s.TotalHours);

            // Gửi danh sách cho View
            ViewBag.Positions = positions;
            ViewBag.ShiftTypes = shiftTypes;

            return View(shifts);
        }

        [HttpPost]
        public async Task<IActionResult> FilterWorkShift([FromForm] FilterWorkShiftDTO filter)
        {
            var query = _context.WorkSchedules
                .Include(ws => ws.Workshift)
                .Include(ws => ws.Attendances)
                    .ThenInclude(a => a.Staff)
                    .ThenInclude(s => s.Role)
                .AsQueryable();

            // Lọc theo từ ngày / đến ngày
            if (filter.FromDate != null)
                query = query.Where(ws => ws.Date >= filter.FromDate);

            if (filter.ToDate != null)
                query = query.Where(ws => ws.Date <= filter.ToDate);

            // Lọc theo vị trí
            if (!string.IsNullOrEmpty(filter.Position))
                query = query.Where(ws => ws.Attendances.Any(a => a.Staff.Role.RoleName == filter.Position));

            // Lọc theo loại ca
            if (!string.IsNullOrEmpty(filter.ShiftType))
                query = query.Where(ws => ws.Workshift.ShiftName == filter.ShiftType);

            // Lọc theo từ khóa (tên nhân viên hoặc vị trí)
            if (!string.IsNullOrEmpty(filter.Keyword))
                query = query.Where(ws =>
                    ws.Attendances.Any(a =>
                        a.Staff.FullName.Contains(filter.Keyword) ||
                        a.Staff.Role.RoleName.Contains(filter.Keyword)));

            var shifts = await query
                .SelectMany(ws => ws.Attendances.Select(a => new
                {
                    Employee = a.Staff.FullName,
                    Date = ws.Date,
                    StartTime = ws.Workshift.StartTime,
                    EndTime = ws.Workshift.EndTime,
                    Position = a.Staff.Role.RoleName,
                    ShiftType = ws.Workshift.ShiftName,
                    TotalHours = (ws.Workshift.EndTime.Value - ws.Workshift.StartTime.Value).TotalHours
                }))
                .ToListAsync();

            // Lấy lại dữ liệu filter
            ViewBag.Positions = await _context.Roles
                .Select(r => r.RoleName)
                .Distinct()
                .Where(rn => !string.IsNullOrEmpty(rn))
                .ToListAsync();

            ViewBag.ShiftTypes = await _context.WorkShifts
                .Select(s => s.ShiftName)
                .Where(s => s != null && s != "")
                .Distinct()
                .ToListAsync();

            // Thống kê
            var today = DateOnly.FromDateTime(DateTime.Now);
            ViewBag.TotalShifts = shifts.Count;
            ViewBag.TotalEmployees = shifts.Select(s => s.Employee).Distinct().Count();
            ViewBag.TodayShifts = shifts.Count(s => s.Date == today);
            ViewBag.TotalHours = shifts.Sum(s => s.TotalHours);

            return View("Index", shifts);
        }

    }
}
