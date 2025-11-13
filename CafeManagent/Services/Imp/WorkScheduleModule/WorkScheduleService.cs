using CafeManagent.Models;
using CafeManagent.Services.Interface.WorkScheduleModule;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp.WorkScheduleModule
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

        public void Save(WorkSchedule workSchedule)
        {
            _context.WorkSchedules.Add(workSchedule);
            _context.SaveChanges();
        }

        public void Update(WorkSchedule workSchedule)
        {
            _context.WorkSchedules.Update(workSchedule);
            _context.SaveChanges();
        }

        public void Delete(WorkSchedule workSchedule)
        {
            _context.WorkSchedules.Remove(workSchedule);
            _context.SaveChanges();
        }
        public List<WorkSchedule> GetWorkSchedulesToday()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return _context.WorkSchedules
                .Include(ws => ws.Staff)
                .Include(ws => ws.Workshift)
                .Include(ws => ws.Manager)
                .Where(ws => ws.Date == today)
                .ToList();

        }
    }
}
