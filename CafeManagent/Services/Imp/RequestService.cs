using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp
{
    public class RequestService : IRequestService
    {
        private readonly CafeManagementContext _context;
        public RequestService(CafeManagementContext context)
        {
            _context = context;
        }
        public void Add(Request request)
        {
            _context.Requests.Add(request); 
            _context.SaveChanges();
        }

        public Request GetById(int id)
        {
            return _context.Requests.Include(r => r.Staff)
                .Where(r => r.ReportId==id).FirstOrDefault();
        }

        public List<Request> GetWaitingAttendanceRequest()
        {
            return _context.Requests.Include(r => r.Staff)
                .Where(r => r.ReportType.Equals("Attendance")).ToList();
        }

        public List<Request> GetWaitingShiftRequest()
        {
            return _context.Requests.Include(r => r.Staff).Where(r => r.ReportType.Equals("Shift")).ToList();
        }
    }
}
