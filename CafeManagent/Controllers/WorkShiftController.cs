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

            var shifts = await _context.WorkSchedules
                    .Include(ws => ws.Workshift)
                    .Include(ws => ws.Staff)
                        .ThenInclude(s => s.Role)
                    .Select(ws => new
                    {
                        ws.ShiftId,
                        Employee = ws.Staff.FullName,
                        Date = ws.Date,
                        StartTime = ws.Workshift.StartTime,
                        EndTime = ws.Workshift.EndTime,
                        Position =ws.Staff.Role.RoleName,
                        ShiftType = ws.Workshift.ShiftName,
                        TotalHours = (ws.Workshift.EndTime.Value - ws.Workshift.StartTime.Value).TotalHours,
                        Description = ws.Description
                    })
                    .ToListAsync();




            var positions = await _context.Roles
                .Where(s => s.RoleId != 1)
                .Select(r => r.RoleName)
                .Distinct()
                .Where(rn => !string.IsNullOrEmpty(rn))
                .ToListAsync();

            var shiftTypes = await _context.WorkShifts
                .Select(s => s.ShiftName)
                .Distinct()
                .Where(s => s != null && s != "")
                .ToListAsync();

            var employees = await _context.Staff
                .Where(s => s.RoleId != 1) 
                .Select(s => s.FullName)
                .Distinct()
                .Where(n => !string.IsNullOrEmpty(n))
                .ToListAsync();



            ViewBag.TotalShifts = shifts.Count;
            ViewBag.TotalEmployees = shifts.Select(s => s.Employee).Distinct().Count();
            ViewBag.TodayShifts = shifts.Count(s => s.Date == today);
            ViewBag.TotalHours = shifts.Sum(s => s.TotalHours);

            ViewBag.Positions = positions;
            ViewBag.ShiftTypes = shiftTypes;
            ViewBag.Employees = employees;

            return View(shifts);
        }

        [HttpPost]
        public async Task<IActionResult> FilterWorkShift([FromForm] FilterWorkShiftDTO filter)
        {
            var query = _context.WorkSchedules
                .Include(ws => ws.Workshift)
                .Include(ws => ws.Staff)
                    .ThenInclude(s => s.Role)
                .AsQueryable();

            if (filter.FromDate != null)
                query = query.Where(ws => ws.Date >= filter.FromDate);

            if (filter.ToDate != null)
                query = query.Where(ws => ws.Date <= filter.ToDate);

            if (!string.IsNullOrEmpty(filter.Position))
                query = query.Where(ws => ws.Staff != null && ws.Staff.Role.RoleName == filter.Position);

            if (!string.IsNullOrEmpty(filter.ShiftType))
                query = query.Where(ws => ws.Workshift.ShiftName == filter.ShiftType);

            
            if (!string.IsNullOrEmpty(filter.Keyword))
                query = query.Where(ws =>
                    (ws.Staff != null &&
                     (ws.Staff.FullName.Contains(filter.Keyword) ||
                      ws.Staff.Role.RoleName.Contains(filter.Keyword))) ||
                    ws.Description.Contains(filter.Keyword));

            
            var shifts = await query
                .Select(ws => new
                {
                    ws.ShiftId,
                    Employee =  ws.Staff.FullName,
                    Date = ws.Date,
                    StartTime = ws.Workshift.StartTime,
                    EndTime = ws.Workshift.EndTime,
                    Position = ws.Staff.Role.RoleName,
                    ShiftType = ws.Workshift.ShiftName,
                    TotalHours = (ws.Workshift.EndTime.Value - ws.Workshift.StartTime.Value).TotalHours,
                    Description = ws.Description
                })
                .ToListAsync();

            ViewBag.Positions = await _context.Roles
                .Where(r => r.RoleId != 1) 
                .Select(r => r.RoleName)
                .Distinct()
                .Where(rn => !string.IsNullOrEmpty(rn))
                .ToListAsync();

            ViewBag.ShiftTypes = await _context.WorkShifts
                .Select(s => s.ShiftName)
                .Where(s => s != null && s != "")
                .Distinct()
                .ToListAsync();

            var employees = await _context.Staff
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

            ViewBag.Employees = employees;

            return View("Index", shifts);
        }



        [HttpPost]
        public async Task<IActionResult> AddWorkShift([FromForm] AddWorkShiftDTO dto)
        {
            Console.WriteLine($"👉 Received: {dto.EmployeeName}, {dto.ShiftType}, {dto.Date}");

            var today = DateOnly.FromDateTime(DateTime.Now);

            if (dto.Date < today)
            {
                TempData["Error"] = "Không thể thêm ca ở ngày đã qua.";
                TempData["ShowError"] = "1";
                return RedirectToAction("Index");
            }

            

            if (string.IsNullOrWhiteSpace(dto.EmployeeName))
            {
                TempData["Error"] = "Tên nhân viên không được để trống";
                return RedirectToAction("Index");
            }

            var staff = await _context.Staff
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.FullName == dto.EmployeeName);



            if (staff == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên";
                return RedirectToAction("Index");
            }

            var workShift = await _context.WorkShifts
                .FirstOrDefaultAsync(ws => ws.ShiftName == dto.ShiftType);

            if (workShift == null)
            {
                TempData["Error"] = "Không tìm thấy loại ca làm việc";
                return RedirectToAction("Index");
            }

            var existsSame = await _context.WorkSchedules
            .AnyAsync(ws => ws.StaffId == staff.StaffId
                        && ws.Date == dto.Date
                        && ws.WorkshiftId == workShift.WorkshiftId);

            if (existsSame)
            {
                TempData["Error"] = "Nhân viên đã có ca này vào ngày đã chọn (trùng ca).";
                TempData["ShowError"] = "1";
                return RedirectToAction("Index");
            }

            var schedule = new WorkSchedule
            {
                Date = dto.Date,
                WorkshiftId = workShift.WorkshiftId,
                StaffId = staff.StaffId,
                ShiftName = dto.ShiftType,
                Description = dto.Note
            };
            _context.WorkSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            var attendance = new Attendance
            {
                StaffId = staff.StaffId,
                WorkshiftId = workShift.WorkshiftId,
                Workdate = dto.Date,
                
            };
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm ca làm thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWorkShift(int id)
        {
            var schedule = await _context.WorkSchedules
                .FirstOrDefaultAsync(ws => ws.ShiftId == id);

            if (schedule == null)
            {
                TempData["Error"] = "Không tìm thấy ca làm để xóa.";
                return RedirectToAction("Index");
            }

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.StaffId == schedule.StaffId &&
                    a.WorkshiftId == schedule.WorkshiftId &&
                    a.Workdate == schedule.Date);

            if (attendance != null)
                _context.Attendances.Remove(attendance);

            _context.WorkSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa ca làm thành công!";
            return RedirectToAction("Index");
        }




    }
}
