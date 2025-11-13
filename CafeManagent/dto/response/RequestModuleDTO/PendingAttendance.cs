namespace CafeManagent.dto.response.RequestModuleDTO
{
    public class PendingAttendance
    {
        public int RequestId { get; set; }
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public DateOnly Date { get; set; }
        public int ShiftId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TimeOnly? OldCheckIn { get; set; }
        public TimeOnly? OldCheckOut { get; set; }
        public TimeOnly? NewCheckIn { get; set; }
        public TimeOnly? NewCheckOut { get; set; }
    }
}
