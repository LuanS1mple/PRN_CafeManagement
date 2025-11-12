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

        public async Task<(List<object> Shifts, int TotalItems)> GetPagedAsync(int page, int pageSize)
        {
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

            return (shifts.Cast<object>().ToList(), totalItems);
        }

        public async Task<(List<object> Shifts, int TotalItems)> FilterAsync(FilterWorkShiftDTO filter, int page, int pageSize)
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

            return (shifts.Cast<object>().ToList(), totalItems);
        }

        public async Task<(bool Success, string Message)> AddAsync(AddWorkShiftDTO dto)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            if (dto.Date < today)
                return (false, "Không thể thêm ca ở ngày đã qua.");

            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.FullName == dto.EmployeeName);
            if (staff == null)
                return (false, "Không tìm thấy nhân viên.");

            var workShift = await _context.WorkShifts.FirstOrDefaultAsync(ws => ws.ShiftName == dto.ShiftType);
            if (workShift == null)
                return (false, "Không tìm thấy loại ca.");

            var existsSame = await _context.WorkSchedules.AnyAsync(ws =>
                ws.StaffId == staff.StaffId &&
                ws.Date == dto.Date &&
                ws.WorkshiftId == workShift.WorkshiftId);

            if (existsSame)
                return (false, "Nhân viên đã có ca này vào ngày đã chọn.");

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
                Workdate = dto.Date
            };
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return (true, "Thêm ca làm thành công!");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(UpdateWorkShiftDTO dto)
        {
            var schedule = await _context.WorkSchedules.FirstOrDefaultAsync(ws => ws.ShiftId == dto.ShiftId);
            if (schedule == null)
                return (false, "Không tìm thấy ca làm.");

            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.FullName == dto.EmployeeName);
            if (staff == null)
                return (false, "Không tìm thấy nhân viên.");

            var workShift = await _context.WorkShifts.FirstOrDefaultAsync(ws => ws.ShiftName == dto.ShiftType);
            if (workShift == null)
                return (false, "Không tìm thấy loại ca.");

            var today = DateOnly.FromDateTime(DateTime.Now);
            if (dto.Date < today)
                return (false, "Không thể sửa ca sang ngày đã qua.");

            var existsSame = await _context.WorkSchedules.AnyAsync(ws =>
                ws.ShiftId != dto.ShiftId &&
                ws.StaffId == staff.StaffId &&
                ws.Date == dto.Date &&
                ws.WorkshiftId == workShift.WorkshiftId);

            if (existsSame)
                return (false, "Nhân viên đã có ca tương tự trong ngày đã chọn.");

            schedule.Date = dto.Date;
            schedule.StaffId = staff.StaffId;
            schedule.WorkshiftId = workShift.WorkshiftId;
            schedule.Description = dto.Note;
            schedule.ShiftName = dto.ShiftType;

            await _context.SaveChangesAsync();
            return (true, "Cập nhật ca làm thành công!");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var schedule = await _context.WorkSchedules.FirstOrDefaultAsync(ws => ws.ShiftId == id);
            if (schedule == null)
                return (false, "Không tìm thấy ca làm để xóa.");

            var attendance = await _context.Attendances.FirstOrDefaultAsync(a =>
                a.StaffId == schedule.StaffId &&
                a.WorkshiftId == schedule.WorkshiftId &&
                a.Workdate == schedule.Date);

            if (attendance != null)
                _context.Attendances.Remove(attendance);

            _context.WorkSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return (true, "Đã xóa ca làm thành công!");
        }

        public async Task<(List<string> Positions, List<string> ShiftTypes, List<string> Employees)> GetFilterDataAsync()
        {
            var positions = await _context.Roles
                .Where(r => r.RoleId != 1)
                .Select(r => r.RoleName)
                .Distinct()
                .ToListAsync();

            var shiftTypes = await _context.WorkShifts
                .Select(s => s.ShiftName)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToListAsync();

            var employees = await _context.Staff
                .Where(s => s.RoleId != 1)
                .Select(s => s.FullName)
                .Distinct()
                .ToListAsync();

            return (positions, shiftTypes, employees);
        }
    }
}
