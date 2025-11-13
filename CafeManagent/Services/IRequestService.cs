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
        public List<Request> GetByStaffId(int id);
        public Task AcceptAttendanceRequest(Request request, Attendance attendance);
        public Task AcceptWorkScheduleRequest(Request request, WorkSchedule workSchedule);
        public Task RejectRequest(Request request);
        public void Delele(Request request);
        List<Request> GetDoneRequest(int id);
        void Delele(int? id);
    }
}
