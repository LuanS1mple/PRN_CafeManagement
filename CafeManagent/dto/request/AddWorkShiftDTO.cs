namespace CafeManagent.dto.request
{
    public class AddWorkShiftDTO
    {
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public string ShiftType { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
