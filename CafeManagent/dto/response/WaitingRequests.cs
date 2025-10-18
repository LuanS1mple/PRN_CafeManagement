using CafeManagent.Models;

namespace CafeManagent.dto.response
{
    public class WaitingRequests
    {
        public List<RequestBasic> AttendanceRequests { get; set; }
        public List<RequestBasic> ShiftRequests { get; set; }
    }
}
