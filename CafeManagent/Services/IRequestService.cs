using CafeManagent.Models;
using Task = System.Threading.Tasks.Task;

namespace CafeManagent.Services
{
    public interface IRequestService
    {
        public void Add(Request request);
        public List<Request> GetWaitingAttendanceRequest();
        public List<Request> GetWaitingShiftRequest();
        public List<Request> GetDoneRequest();
        public Request GetById(int id);
        public Task AcceptRequest(Request request, Attendance attendance);
        public Task RejectRequest(Request request);

  

    }
}
