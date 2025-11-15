namespace CafeManagent.dto.response.StaffWorkScheduleDTO
{
    public class StaffWorkScheduleDTO
    {
        public DateOnly Date { get; set; }

        public string? Position { get; set; }

        public string? ShiftType { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public double? TotalHours { get; set; }

        public string? Description { get; set; }
    }
}
