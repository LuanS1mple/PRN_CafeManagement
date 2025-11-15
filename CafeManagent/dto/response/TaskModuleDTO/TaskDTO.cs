namespace CafeManagent.dto.response.TaskModuleDTO
{
    public class TaskDTO
    {
        public int TaskId { get; set; }
        public int Status { get; set; }
        public DateTime? AssignTime { get; set; }
        public DateTime? DueTime { get; set; }
        public int? TasktypeId { get; set; }
        public string TasktypeName { get; set; } 
        public string Description { get; set; }  
        public int? ManagerId { get; set; }
        public string ManagerName { get; set; } 
        public int? StaffId { get; set; }
        public string StaffName { get; set; }
    }
}
