using CafeManagent.dto.request.WorkShiftModuleDTO;
using CafeManagent.dto.response.WorkShiftDTO;
using CafeManagent.Enums;
using CafeManagent.Hubs;
using CafeManagent.mapper;
using CafeManagent.Models;
using CafeManagent.Services.Interface.WorkShiftModule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp.WorkShiftModule
{
    public class WorkShiftService : IWorkShiftService
    {
        private readonly CafeManagementContext _context;
        private readonly IHubContext<WorkShiftHub> _hubContext;

        public WorkShiftService(CafeManagementContext context, IHubContext<WorkShiftHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<(List<WorkShiftDTO> shifts, int totalItems)> GetPagedWorkShiftsAsync(int page, int pageSize)
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
                    Description = ws.Description,
                    Email = ws.Staff.Email,
                });

            int totalItems = await query.CountAsync();

            var shifts = await query
                .OrderByDescending(s => s.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (shifts, totalItems);
        }


        public async Task<(List<WorkShiftDTO> shifts, int totalItems)> FilterPagedWorkShiftsAsync(FilterWorkShiftDTO filter, int page, int pageSize)
        {
            var query = _context.WorkSchedules
                .Include(ws => ws.Workshift)
                .Include(ws => ws.Staff)
                    .ThenInclude(s => s.Role)
                .AsQueryable();

            if (filter.FromDate != null)
            {
                query = query.Where(ws => ws.Date >= filter.FromDate);
            }


            if (filter.ToDate != null)
            {
                query = query.Where(ws => ws.Date <= filter.ToDate);
            }

            if (!string.IsNullOrEmpty(filter.Position))
            {
                query = query.Where(ws => ws.Staff.Role.RoleName == filter.Position);
            }

            if (!string.IsNullOrEmpty(filter.ShiftType))
            {
                query = query.Where(ws => ws.Workshift.ShiftName == filter.ShiftType);
            }

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                query = query.Where(ws =>
                    ws.Staff.FullName.Contains(filter.Keyword) ||
                    ws.Staff.Role.RoleName.Contains(filter.Keyword) ||
                    ws.Description.Contains(filter.Keyword));
            }

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


        public async Task<bool> AddWorkShiftAsync(AddWorkShiftDTO dto)
        {
            try
            {
                var staff = await _context.Staff.FirstAsync(s => s.FullName == dto.EmployeeName);
                var shift = await _context.WorkShifts.FirstAsync(s => s.ShiftName == dto.ShiftType);

                var ws = WorkShiftMapper.FromAddWorkShiftDTO(dto, staff.StaffId, shift.WorkshiftId);

                _context.WorkSchedules.Add(ws);

                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }


        public async Task<(bool Success, NotifyMessage Notify)> DeleteWorkShiftAsync(int id)
        {
            var schedule = await _context.WorkSchedules.FirstOrDefaultAsync(ws => ws.ShiftId == id);
            if (schedule == null)
                return (false, NotifyMessage.KHONG_THAY_CA_LAM);

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.StaffId == schedule.StaffId &&
                    a.WorkshiftId == schedule.WorkshiftId &&
                    a.Workdate == schedule.Date);

            if (attendance != null)
                _context.Attendances.Remove(attendance);

            _context.WorkSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return (true, NotifyMessage.XOA_CA_LAM_OK);
        }

        public async Task<(bool Success, NotifyMessage Notify)> UpdateWorkShiftAsync(UpdateWorkShiftDTO dto)
        {
            var schedule = await _context.WorkSchedules
                .Include(ws => ws.Staff)
                .Include(ws => ws.Workshift)
                .FirstOrDefaultAsync(ws => ws.ShiftId == dto.ShiftId);

            if (schedule == null)
                return (false, NotifyMessage.KHONG_THAY_CA_LAM);

            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.FullName == dto.EmployeeName);
            if (staff == null)
                return (false, NotifyMessage.THEM_CA_THAT_BAI);

            var workShift = await _context.WorkShifts.FirstOrDefaultAsync(ws => ws.ShiftName == dto.ShiftType);
            if (workShift == null)
                return (false, NotifyMessage.KHONG_THAY_CA_LAM);

            var today = DateOnly.FromDateTime(DateTime.Now);
            if (dto.Date < today)
                return (false, NotifyMessage.CA_CUA_NGAY_CU);

            var existsSame = await _context.WorkSchedules.AnyAsync(ws =>
                ws.ShiftId != dto.ShiftId &&
                ws.StaffId == staff.StaffId &&
                ws.Date == dto.Date &&
                ws.WorkshiftId == workShift.WorkshiftId);

            if (existsSame)
                return (false, NotifyMessage.TRUNG_CA);

            schedule.Date = dto.Date;
            schedule.StaffId = staff.StaffId;
            schedule.WorkshiftId = workShift.WorkshiftId;
            schedule.Description = dto.Note;
            schedule.ShiftName = dto.ShiftType;

            await _context.SaveChangesAsync();

            return (true, NotifyMessage.CAP_NHAT_CA_OK);
        }

    }



}
