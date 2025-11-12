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

        public List<WorkShift> GetPaged(int page, int pageSize, out int totalItems)
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

            totalItems = query.Count();

            return query
                .OrderByDescending(s => s.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Cast<WorkShift>()
                .ToList();
        }

        public List<WorkShift> Filter(FilterWorkShiftDTO filter, int page, int pageSize, out int totalItems)
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

            totalItems = query.Count();

            var shifts = query
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
                .Cast<WorkShift>()
                .ToList();

            return shifts;
        }

        public void Add(AddWorkShiftDTO dto, out bool success, out string message)
        {
            success = false;
            message = "";

            var today = DateOnly.FromDateTime(DateTime.Now);
            if (dto.Date < today)
            {
                message = "Không thể thêm ca ở ngày đã qua.";
                return;
            }

            var staff = _context.Staff.FirstOrDefault(s => s.FullName == dto.EmployeeName);
            if (staff == null)
            {
                message = "Không tìm thấy nhân viên.";
                return;
            }

            var workShift = _context.WorkShifts.FirstOrDefault(ws => ws.ShiftName == dto.ShiftType);
            if (workShift == null)
            {
                message = "Không tìm thấy loại ca.";
                return;
            }

            var existsSame = _context.WorkSchedules.Any(ws =>
                ws.StaffId == staff.StaffId &&
                ws.Date == dto.Date &&
                ws.WorkshiftId == workShift.WorkshiftId);

            if (existsSame)
            {
                message = "Nhân viên đã có ca này vào ngày đã chọn.";
                return;
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
                Workdate = dto.Date
            };
            _context.Attendances.Add(attendance);
            _context.SaveChanges();

            success = true;
            message = "Thêm ca làm thành công!";
        }

        public void Update(UpdateWorkShiftDTO dto, out bool success, out string message)
        {
            success = false;
            message = "";

            var schedule = _context.WorkSchedules.FirstOrDefault(ws => ws.ShiftId == dto.ShiftId);
            if (schedule == null)
            {
                message = "Không tìm thấy ca làm.";
                return;
            }

            var staff = _context.Staff.FirstOrDefault(s => s.FullName == dto.EmployeeName);
            if (staff == null)
            {
                message = "Không tìm thấy nhân viên.";
                return;
            }

            var workShift = _context.WorkShifts.FirstOrDefault(ws => ws.ShiftName == dto.ShiftType);
            if (workShift == null)
            {
                message = "Không tìm thấy loại ca.";
                return;
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            if (dto.Date < today)
            {
                message = "Không thể sửa ca sang ngày đã qua.";
                return;
            }

            var existsSame = _context.WorkSchedules.Any(ws =>
                ws.ShiftId != dto.ShiftId &&
                ws.StaffId == staff.StaffId &&
                ws.Date == dto.Date &&
                ws.WorkshiftId == workShift.WorkshiftId);

            if (existsSame)
            {
                message = "Nhân viên đã có ca tương tự trong ngày đã chọn.";
                return;
            }

            schedule.Date = dto.Date;
            schedule.StaffId = staff.StaffId;
            schedule.WorkshiftId = workShift.WorkshiftId;
            schedule.Description = dto.Note;
            schedule.ShiftName = dto.ShiftType;

            _context.SaveChanges();

            success = true;
            message = "Cập nhật ca làm thành công!";
        }

        public void Delete(int id, out bool success, out string message)
        {
            success = false;
            message = "";

            var schedule = _context.WorkSchedules.FirstOrDefault(ws => ws.ShiftId == id);
            if (schedule == null)
            {
                message = "Không tìm thấy ca làm để xóa.";
                return;
            }

            var attendance = _context.Attendances.FirstOrDefault(a =>
                a.StaffId == schedule.StaffId &&
                a.WorkshiftId == schedule.WorkshiftId &&
                a.Workdate == schedule.Date);

            if (attendance != null)
                _context.Attendances.Remove(attendance);

            _context.WorkSchedules.Remove(schedule);
            _context.SaveChanges();

            success = true;
            message = "Đã xóa ca làm thành công!";
        }

        public void GetFilterData(out List<string> positions, out List<string> shiftTypes, out List<string> employees)
        {
            positions = _context.Roles
                .Where(r => r.RoleId != 1)
                .Select(r => r.RoleName)
                .Distinct()
                .ToList();

            shiftTypes = _context.WorkShifts
                .Select(s => s.ShiftName)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            employees = _context.Staff
                .Where(s => s.RoleId != 1)
                .Select(s => s.FullName)
                .Distinct()
                .ToList();
        }
    }
}
