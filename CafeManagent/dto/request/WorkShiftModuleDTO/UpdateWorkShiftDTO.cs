namespace CafeManagent.dto.request.WorkShiftModuleDTO
{
    public class UpdateWorkShiftDTO
    {
        public int ShiftId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ShiftType { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public string? Note { get; set; }
    }
}
