using CafeManagent.dto.response.StaffWorkScheduleDTO;
using CafeManagent.dto.response.WorkShiftDTO;
using CafeManagent.Models;
using CafeManagent.Services.Interface.StaffWorkScheduleModule;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp.StaffWorkScheduleModule
{
    public class StaffWorkScheduleService : IStaffWorkScheduleService
    {
        private readonly CafeManagementContext _context;

        public StaffWorkScheduleService(CafeManagementContext context)
        {
            _context = context;
        }
        public async Task<List<StaffWorkScheduleDTO>> GetWorkShiftsByStaffAsync(int staffId)
        {
            var query = _context.WorkSchedules
                .Include(ws => ws.Workshift)
                .Include(ws => ws.Staff)
                    .ThenInclude(s => s.Role)
                .Where(ws => ws.StaffId == staffId)
                .Select(ws => new StaffWorkScheduleDTO
                {
                    Date = (DateOnly)ws.Date,
                    Position = ws.Staff.Role.RoleName,
                    ShiftType = ws.Workshift.ShiftName,
                    StartTime = ws.Workshift.StartTime,
                    EndTime = ws.Workshift.EndTime,
                    Description = ws.Description,
                });

            return await query
                .OrderByDescending(w => w.Date)
                .ToListAsync();
        }



    }
}
