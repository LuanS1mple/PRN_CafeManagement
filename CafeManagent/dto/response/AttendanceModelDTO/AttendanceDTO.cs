namespace CafeManagent.dto.response.attendance
{
    public class AttendanceDTO
    {
        public int AttendanceId { get; set; }
        public string? StaffName { get; set; }
        public string? ShiftName { get; set; }
        public DateOnly? Workdate { get; set; }
        public TimeOnly? CheckIn { get; set; }
        public TimeOnly? CheckOut { get; set; }
        public decimal? TotalHour { get; set; }
        public string? Note { get; set; }

        public int? Status { get; set; }

    }
}
