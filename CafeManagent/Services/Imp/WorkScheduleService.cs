using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp
{
    public class WorkScheduleService : IWorkScheduleService
    {
        private readonly CafeManagementContext _context;
        public WorkScheduleService(CafeManagementContext context)
        {
            _context = context;
        }
        public WorkSchedule Get(int staffId, int workShiftId, DateOnly date)
        {
            return _context.WorkSchedules.Include(w => w.Staff).Include(w => w.Workshift)
                .Where(w => w.StaffId == staffId && w.Date.Value.Equals(date) && w.WorkshiftId == workShiftId).FirstOrDefault()!;
        }
        public WorkSchedule GetById(int id)
        {
            return _context.WorkSchedules
                .Include(w => w.Staff)
                .Include(w => w.Manager)
                .Include(w => w.Workshift)
                .FirstOrDefault(w => w.ShiftId == id)!;
        }

        public List<WorkSchedule> Get(int staffId)
        {
            return _context.WorkSchedules.Where(w => w.StaffId == staffId && w.Date.Value > DateOnly.FromDateTime(DateTime.Now)).ToList();
        }
    }
}
