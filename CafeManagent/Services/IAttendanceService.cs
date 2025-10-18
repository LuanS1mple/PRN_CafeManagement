using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IAttendanceService
    {
        public Attendance GetAttendance(DateOnly date, int shift,int staffId);
    }
}
