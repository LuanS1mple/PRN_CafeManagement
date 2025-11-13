namespace CafeManagent.dto.response.RequestModuleDTO
{
    public class PendingWorkSchedule
    {
        public int RequestId { get; set; }
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public DateOnly Date { get; set; }
        public int ShiftId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int OldShift { get; set; }
        public int NewShift { get; set; }
    }
}
