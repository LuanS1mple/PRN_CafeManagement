namespace CafeManagent.dto.attendance
{
    public class MonthlyReport
    {
        public int StaffId { get; set; }
        public string? StaffName { get; set; }  
        public int Month {  get; set; }
        public int Year { get; set; }
        public int workingDays { get; set; }
        public decimal? TotalHours { get; set; }
        public int LeaveEarlyCount { get; set; }
        public int LateCount { get; set; }
        public int LeaveDays { get; set; }
    }
}
