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
