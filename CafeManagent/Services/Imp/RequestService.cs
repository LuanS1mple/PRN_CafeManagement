using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace CafeManagent.Services.Imp
{
    public class RequestService : IRequestService
    {
        private readonly CafeManagementContext _context;
        public RequestService(CafeManagementContext context)
        {
            _context = context;
        }

        public async Task AcceptRequest(Request request, Attendance attendance)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Requests.Update(request);
                _context.Attendances.Update(attendance);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
                .Where(r => r.ReportType.Equals("Attendance") && r.Status == 0).ToList();
        }

        public List<Request> GetWaitingShiftRequest()
        {
            return _context.Requests.Include(r => r.Staff).Where(r => r.ReportType.Equals("Shift") && r.Status ==0).ToList();
        }
        public List<Request> GetDoneRequest()
        {
            return _context.Requests.Include(r => r.Staff).Where(r => r.Status!=0).ToList();
        }
        public async Task RejectRequest(Request request)
        {
            _context.Requests.Update(request);
            await _context.SaveChangesAsync();
        }
    }
}
