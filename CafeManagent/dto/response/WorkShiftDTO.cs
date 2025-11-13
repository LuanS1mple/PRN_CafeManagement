namespace CafeManagent.dto.response
{
    public class WorkShiftDTO
    {
        public int ShiftId { get; set; }
        public string Employee { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public string Position { get; set; }
        public string ShiftType { get; set; }
        public double TotalHours { get; set; }
        public string Description { get; set; }

    }
}
