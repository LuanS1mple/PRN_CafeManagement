using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IAttendanceService
    {
        public Attendance GetAttendance(DateOnly date, int shift,int staffId);
        public void Update(Attendance attendance);  
        public List<Attendance> GetAllAttance();
        public List<Attendance> FilterAttendance(DateOnly? fromDate, DateOnly? toDate, string? keyword);
        public Attendance CheckIn(int staffId, int shiftId);
        public Attendance CheckOut(int staffId, int shiftId);
        public Attendance GetAttendanceWithShift(int shiftId, int staffId, DateOnly date);
    }
}
