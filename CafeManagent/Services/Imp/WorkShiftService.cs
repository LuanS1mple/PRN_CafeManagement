using CafeManagent.dto.request;
using CafeManagent.dto.response;
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

        public async Task<(List<WorkShiftDTO> Shifts, int TotalItems)> GetPagedWorkShiftsAsync(int page, int pageSize)
        {
            var query = _context.WorkSchedules
                .Include(ws => ws.Workshift)
                .Include(ws => ws.Staff)
                    .ThenInclude(s => s.Role)
                .Select(ws => new WorkShiftDTO
                {
                    ShiftId = ws.ShiftId,
                    Employee = ws.Staff.FullName,
                    Date = (DateOnly)ws.Date,
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

            return (shifts, totalItems);
        }


        public async Task<(List<WorkShiftDTO> Shifts, int TotalItems)> FilterPagedWorkShiftsAsync(FilterWorkShiftDTO filter, int page, int pageSize)
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
                .Select(ws => new WorkShiftDTO
                {
                    ShiftId = ws.ShiftId,
                    Employee = ws.Staff.FullName,
                    Date = (DateOnly)ws.Date,
                    StartTime = ws.Workshift.StartTime,
                    EndTime = ws.Workshift.EndTime,
                    Position = ws.Staff.Role.RoleName,
                    ShiftType = ws.Workshift.ShiftName,
                    TotalHours = (ws.Workshift.EndTime.Value - ws.Workshift.StartTime.Value).TotalHours,
                    Description = ws.Description
                })
                .OrderByDescending(s => s.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (shifts, totalItems);
        }


        public async Task<(bool Success, string Message)> AddWorkShiftAsync(AddWorkShiftDTO dto)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (dto.Date < today)
                return (false, "Không thể thêm ca ở ngày đã qua.");

            if (string.IsNullOrWhiteSpace(dto.EmployeeName))
                return (false, "Tên nhân viên không được để trống.");

            var staff = await _context.Staff
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.FullName == dto.EmployeeName);

            if (staff == null)
                return (false, "Không tìm thấy nhân viên.");

            var workShift = await _context.WorkShifts
                .FirstOrDefaultAsync(ws => ws.ShiftName == dto.ShiftType);

            if (workShift == null)
                return (false, "Không tìm thấy loại ca làm việc.");

            var existsSame = await _context.WorkSchedules
                .AnyAsync(ws => ws.StaffId == staff.StaffId
                            && ws.Date == dto.Date
                            && ws.WorkshiftId == workShift.WorkshiftId);

            if (existsSame)
                return (false, "Nhân viên đã có ca này vào ngày đã chọn (trùng ca).");

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

        public async Task<(bool Success, string Message)> DeleteWorkShiftAsync(int id)
        {
            var schedule = await _context.WorkSchedules.FirstOrDefaultAsync(ws => ws.ShiftId == id);
            if (schedule == null)
                return (false, "Không tìm thấy ca làm để xóa.");

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.StaffId == schedule.StaffId &&
                    a.WorkshiftId == schedule.WorkshiftId &&
                    a.Workdate == schedule.Date);

            if (attendance != null)
                _context.Attendances.Remove(attendance);

            _context.WorkSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return (true, "Đã xóa ca làm thành công!");
        }
    }


    
}
