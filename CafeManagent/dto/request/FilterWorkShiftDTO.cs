namespace CafeManagent.dto.request
{
    public class FilterWorkShiftDTO
    {
        public string? Keyword { get; set; }  // tên, vị trí, nhân viên
        public string? Position { get; set; }
        public string? ShiftType { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
    }
}
