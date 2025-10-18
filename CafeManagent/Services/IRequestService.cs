using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IRequestService
    {
        public void Add(Request request);
        public List<Request> GetWaitingAttendanceRequest();
        public List<Request> GetWaitingShiftRequest();
        public Request GetById(int id);
    }
}
