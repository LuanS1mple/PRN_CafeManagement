using CafeManagent.dto.request;
using CafeManagent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CafeManagent.Services.Imp
{
    public class WorkShiftService : IWorkShiftService
    {
        private readonly CafeManagementContext _context;

        public WorkShiftService(CafeManagementContext context)
        {
            _context = context;
        }

        public List<WorkSchedule> GetAll()
        {
            return _context.WorkSchedules
                    .Include(ws => ws.Workshift)
                    .Include( ws => ws.Staff)
                        .ThenInclude(s => s.Role).ToList();
        }

        public List<WorkSchedules> Filter(FilterWorkShiftDTO filter)
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
                query = query.Where(ws => ws.Staff.Role.RoleName == filter.Position);

            if (!string.IsNullOrEmpty(filter.ShiftType))
                query = query.Where(ws => ws.Workshift.ShiftName == filter.ShiftType);

            if (!string.IsNullOrEmpty(filter.Keyword))
                query = query.Where(ws =>
                    (ws.Staff.FullName.Contains(filter.Keyword) ||
                     ws.Staff.Role.RoleName.Contains(filter.Keyword)) ||
                    ws.Description.Contains(filter.Keyword));

            var shifts = query.Select(ws => new
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
            .ToList();

            return shifts;
        }

        public bool AddWorkShift(AddWorkShiftDTO dto, out string message)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (dto.Date < today)
            {
                message = "Không thể thêm ca ở ngày đã qua.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.EmployeeName))
            {
                message = "Tên nhân viên không được để trống.";
                return false;
            }

            var staff = _context.Staff
                .Include(s => s.Role)
                .FirstOrDefault(s => s.FullName == dto.EmployeeName);

            if (staff == null)
            {
                message = "Không tìm thấy nhân viên.";
                return false;
            }

            var workShift = _context.WorkShifts
                .FirstOrDefault(ws => ws.ShiftName == dto.ShiftType);

            if (workShift == null)
            {
                message = "Không tìm thấy loại ca làm việc.";
                return false;
            }

            var existsSame = _context.WorkSchedules
                .Any(ws => ws.StaffId == staff.StaffId &&
                           ws.Date == dto.Date &&
                           ws.WorkshiftId == workShift.WorkshiftId);

            if (existsSame)
            {
                message = "Nhân viên đã có ca này vào ngày đã chọn (trùng ca).";
                return false;
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
            _context.SaveChanges();

            var attendance = new Attendance
            {
                StaffId = staff.StaffId,
                WorkshiftId = workShift.WorkshiftId,
                Workdate = dto.Date,
            };

            _context.Attendances.Add(attendance);
            _context.SaveChanges();

            message = "Thêm ca làm thành công!";
            return true;
        }

        public bool DeleteWorkShift(int id, out string message)
        {
            var schedule = _context.WorkSchedules.FirstOrDefault(ws => ws.ShiftId == id);

            if (schedule == null)
            {
                message = "Không tìm thấy ca làm để xóa.";
                return false;
            }

            var attendance = _context.Attendances.FirstOrDefault(a =>
                a.StaffId == schedule.StaffId &&
                a.WorkshiftId == schedule.WorkshiftId &&
                a.Workdate == schedule.Date);

            if (attendance != null)
                _context.Attendances.Remove(attendance);

            _context.WorkSchedules.Remove(schedule);
            _context.SaveChanges();

            message = "Đã xóa ca làm thành công!";
            return true;
        }

        public List<string> GetPositions()
        {
            return _context.Roles
                .Where(r => r.RoleId != 1)
                .Select(r => r.RoleName)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();
        }

        public List<string> GetShiftTypes()
        {
            return _context.WorkShifts
                .Select(s => s.ShiftName)
                .Where(s => s != null && s != "")
                .Distinct()
                .ToList();
        }

        public List<string> GetEmployees()
        {
            return _context.Staff
                .Where(s => s.RoleId != 1)
                .Select(s => s.FullName)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();
        }

        public int GetTotalShifts(List<object> shifts)
        {
            return shifts.Count;
        }

        public int GetTotalEmployees(List<object> shifts)
        {
            return shifts.Select(s => s.GetType().GetProperty("Employee")!.GetValue(s)).Distinct().Count();
        }

        public int GetTodayShifts(List<object> shifts)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return shifts.Count(s => (DateOnly)s.GetType().GetProperty("Date")!.GetValue(s)! == today);
        }

        public double GetTotalHours(List<object> shifts)
        {
            return shifts.Sum(s => (double)s.GetType().GetProperty("TotalHours")!.GetValue(s)!);
        }
    }
}
