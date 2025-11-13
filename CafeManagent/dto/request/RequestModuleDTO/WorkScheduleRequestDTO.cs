namespace CafeManagent.dto.request.RequestModuleDTO
{
    public class WorkScheduleRequestDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateOnly DateChange { get; set; }
        public int OldShiftId { get; set; }
        public string Require { get; set; }
        public int NewShiftId { get; set; }
    }
}
