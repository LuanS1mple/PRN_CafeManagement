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

        public async Task<IActionResult> Index(int page = 1, int pageSize = 6)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var query = _context.WorkSchedules
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
                    Position = ws.Staff.Role.RoleName,
                    ShiftType = ws.Workshift.ShiftName,
                    TotalHours = (ws.Workshift.EndTime.Value - ws.Workshift.StartTime.Value).TotalHours,
                    Description = ws.Description
                });

            int totalItems = await query.CountAsync();

            var shifts = await query
                .OrderByDescending(s => s.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Thông tin phân trang
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lấy dữ liệu khác
            var positions = await _context.Roles
                .Where(s => s.RoleId != 1)
                .Select(r => r.RoleName)
                .Distinct()
                .ToListAsync();

            var shiftTypes = await _context.WorkShifts
                .Select(s => s.ShiftName)
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s))
                .ToListAsync();

            var employees = await _context.Staff
                .Where(s => s.RoleId != 1)
                .Select(s => s.FullName)
                .Distinct()
                .ToListAsync();

            ViewBag.Positions = positions;
            ViewBag.ShiftTypes = shiftTypes;
            ViewBag.Employees = employees;

            ViewBag.TotalShifts = totalItems;
            ViewBag.TotalEmployees = shifts.Select(s => s.Employee).Distinct().Count();
            ViewBag.TodayShifts = shifts.Count(s => s.Date == today);
            ViewBag.TotalHours = shifts.Sum(s => s.TotalHours);

            return View(shifts);
        }


        [HttpPost]
        public async Task<IActionResult> FilterWorkShift(FilterWorkShiftDTO filter, int page = 1, int pageSize = 6)
        {
            var query = _context.WorkSchedules
                .Include(ws => ws.Workshift)
                .Include(ws => ws.Staff)
                    .ThenInclude(s => s.Role)
                .AsQueryable();

            // các điều kiện filter (như cũ)
            if (filter.FromDate != null)
                query = query.Where(ws => ws.Date >= filter.FromDate);
            if (filter.ToDate != null)
                query = query.Where(ws => ws.Date <= filter.ToDate);
            if (!string.IsNullOrEmpty(filter.Position))
                query = query.Where(ws => ws.Staff.Role.RoleName == filter.Position);
            if (!string.IsNullOrEmpty(filter.ShiftType))
                query = query.Where(ws => ws.Workshift.ShiftName == filter.ShiftType);
            if (!string.IsNullOrEmpty(filter.Keyword))
                query = query.Where(ws =>
                    ws.Staff.FullName.Contains(filter.Keyword) ||
                    ws.Staff.Role.RoleName.Contains(filter.Keyword) ||
                    ws.Description.Contains(filter.Keyword));

            int totalItems = await query.CountAsync();

            var shifts = await query
                .OrderByDescending(s => s.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ws => new
                {
                    ws.ShiftId,
                    Employee = ws.Staff.FullName,
                    Date = ws.Date,
                    StartTime = ws.Workshift.StartTime,
                    EndTime = ws.Workshift.EndTime,
                    Position = ws.Staff.Role.RoleName,
                    ShiftType = ws.Workshift.ShiftName,
                    TotalHours = (ws.Workshift.EndTime.Value - ws.Workshift.StartTime.Value).TotalHours,
                    Description = ws.Description
                })
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Gán lại các ViewBag cần thiết
            ViewBag.Positions = await _context.Roles.Select(r => r.RoleName).Distinct().ToListAsync();
            ViewBag.ShiftTypes = await _context.WorkShifts.Select(s => s.ShiftName).Distinct().ToListAsync();
            ViewBag.Employees = await _context.Staff.Select(s => s.FullName).Distinct().ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Now);
            ViewBag.TotalShifts = totalItems;
            ViewBag.TotalEmployees = shifts.Select(s => s.Employee).Distinct().Count();
            ViewBag.TodayShifts = shifts.Count(s => s.Date == today);
            ViewBag.TotalHours = shifts.Sum(s => s.TotalHours);

            return View("Index", shifts);
        }




        [HttpPost]
        public async Task<IActionResult> AddWorkShift( AddWorkShiftDTO dto)
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


        [HttpPost]
        public async Task<IActionResult> UpdateWorkShift( UpdateWorkShiftDTO dto)
        {
            Console.WriteLine($"🔄 Update request: ID={dto.ShiftId}, Employee={dto.EmployeeName}, Shift={dto.ShiftType}, Date={dto.Date}");

            var schedule = await _context.WorkSchedules
                .Include(ws => ws.Staff)
                .Include(ws => ws.Workshift)
                .FirstOrDefaultAsync(ws => ws.ShiftId == dto.ShiftId);

            if (schedule == null)
            {
                TempData["Error"] = "Không tìm thấy ca làm để cập nhật.";
                return RedirectToAction("Index");
            }

            // Kiểm tra nhân viên
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.FullName == dto.EmployeeName);
            if (staff == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên.";
                return RedirectToAction("Index");
            }

            var workShift = await _context.WorkShifts.FirstOrDefaultAsync(ws => ws.ShiftName == dto.ShiftType);
            if (workShift == null)
            {
                TempData["Error"] = "Không tìm thấy loại ca.";
                return RedirectToAction("Index");
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            if (dto.Date < today)
            {
                TempData["Error"] = "Không thể sửa ca sang ngày đã qua.";
                return RedirectToAction("Index");
            }

            var existsSame = await _context.WorkSchedules.AnyAsync(ws =>
                ws.ShiftId != dto.ShiftId &&
                ws.StaffId == staff.StaffId &&
                ws.Date == dto.Date &&
                ws.WorkshiftId == workShift.WorkshiftId);

            if (existsSame)
            {
                TempData["Error"] = "Nhân viên này đã có ca tương tự trong ngày đã chọn.";
                return RedirectToAction("Index");
            }

            schedule.Date = dto.Date;
            schedule.StaffId = staff.StaffId;
            schedule.WorkshiftId = workShift.WorkshiftId;
            schedule.Description = dto.Note;
            schedule.ShiftName = dto.ShiftType;

            await _context.SaveChangesAsync();

            var attendance = await _context.Attendances.FirstOrDefaultAsync(a =>
                a.StaffId == staff.StaffId && a.Workdate == schedule.Date);

            if (attendance != null)
            {
                attendance.WorkshiftId = workShift.WorkshiftId;
                attendance.Workdate = dto.Date;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Cập nhật ca làm thành công!";
            return RedirectToAction("Index");
        }


    }
}
