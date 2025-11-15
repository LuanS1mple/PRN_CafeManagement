namespace CafeManagent.dto.request.AttendanceModuleDTO
{
    public class AddCheckInAttendanceDTO
    {
        public int? StaffId { get; set; }

        public int? ShiftId { get; set; }

        public int? WorkshiftId { get; set; }

        public TimeOnly? CheckIn { get; set; }
        public DateOnly? Workdate { get; set; }
        public int? Status { get; set; }
        public string? Note { get; set; }
    }
}
