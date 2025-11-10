using CafeManagent.dto.attendance;
using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IAttendanceService
    {
        public Attendance GetAttendance(DateOnly date, int shift,int staffId);
        public void Update(Attendance attendance);  
        public List<Attendance> GetAllAttance();
        public List<Attendance> FilterAttendance(DateOnly? fromDate, DateOnly? toDate, string? keyword);
        public Task<Attendance?> CheckInAsync(int workshiftId, int shiftId, int staffId, DateOnly date);
        public Task<Attendance?> CheckOutAsync(int workshiftId, int shiftId, int staffId, DateOnly date);
        public Task<Attendance?> GetAttendanceWithShiftAsync(int workshiftId, int staffId, DateOnly date, int shiftId);

        public List<Attendance> GetAttendanceByMonth(int? staffId, int? month, int? year);
        public Task<List<MonthlyReport>> GetMonthlyReportAsync(int? staffId, int month, int year);
        public Task<byte[]> ExportMonthlyReportToExcelAsync(List<MonthlyReport> monthlyReportList);
       
    }
}
