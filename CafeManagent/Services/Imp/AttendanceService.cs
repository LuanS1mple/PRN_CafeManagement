using CafeManagent.Models;

namespace CafeManagent.Services.Imp
{
    public class AttendanceService : IAttendanceService
    {
        private readonly CafeManagementContext _context;
        public AttendanceService(CafeManagementContext context)
        {
            _context = context;
        }
        public Attendance GetAttendance(DateOnly date, int shift, int staffId)
        {
            return _context.Attendances.FirstOrDefault(s => s.StaffId == staffId && s.Workdate.Equals(date) && s.ShiftId==shift);
        }
    }
}
