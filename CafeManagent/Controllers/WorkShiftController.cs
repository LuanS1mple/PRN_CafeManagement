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
                .Include(ws => ws.Staff)
                    .ThenInclude(s => s.Contract)
                .Include(ws => ws.Staff)
                    .ThenInclude(s => s.Role)
                .Select(ws => new
                {
                    ws.ShiftId,
                    Employee = ws.Staff != null ? ws.Staff.FullName : "Chưa gán",
                    Date = ws.Date,
                    Position = ws.Staff != null
                        ? (ws.Staff.Contract != null ? ws.Staff.Contract.Position :
                           (ws.Staff.Role != null ? ws.Staff.Role.RoleName : "Không rõ"))
                        : "Không rõ",
                    ShiftType = ws.Workshift != null ? ws.Workshift.ShiftName : ws.ShiftName,
                    StartTime = ws.Workshift != null ? ws.Workshift.StartTime : null,
                    EndTime = ws.Workshift != null ? ws.Workshift.EndTime : null,
                    TotalHours = ws.Workshift != null && ws.Workshift.StartTime.HasValue && ws.Workshift.EndTime.HasValue
                        ? (decimal)(ws.Workshift.EndTime.Value.ToTimeSpan() - ws.Workshift.StartTime.Value.ToTimeSpan()).TotalHours
                        : 0
                })
                .OrderBy(ws => ws.Date)
                .ThenBy(ws => ws.StartTime)
                .ToListAsync();

            // 🟢 Lấy danh sách vị trí và loại ca để hiển thị trong filter
            var positions = await _context.Contracts
                .Select(c => c.Position)
                .Distinct()
                .Where(p => p != null && p != "")
                .ToListAsync();

            var shiftTypes = await _context.WorkShifts
                .Select(s => s.ShiftName)
                .Distinct()
                .Where(s => s != null && s != "")
                .ToListAsync();

            // 🧾 Thống kê
            ViewBag.TotalShifts = shifts.Count;
            ViewBag.TotalEmployees = shifts.Select(s => s.Employee).Distinct().Count();
            ViewBag.TodayShifts = shifts.Count(s => s.Date == today);
            ViewBag.TotalHours = shifts.Sum(s => s.TotalHours);

            // 🟢 Gửi danh sách cho View
            ViewBag.Positions = positions;
            ViewBag.ShiftTypes = shiftTypes;

            return View(shifts);
        }
    }
}
