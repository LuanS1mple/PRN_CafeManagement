using CafeManagent.ErrorHandler;
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

        public async Task AcceptAttendanceRequest(Request request, Attendance attendance)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Attach(request);
                _context.Entry(request).State = EntityState.Modified;
                _context.Attach(attendance); 
                _context.Entry(attendance).State = attendance.AttendanceId == 0 ? EntityState.Added : EntityState.Modified;
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
            return _context.Requests.Include(r => r.Staff).Where(r => r.ReportType.Equals("WorkSchedule") && r.Status ==0).ToList();
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

        public async Task AcceptWorkScheduleRequest(Request request, WorkSchedule workSchedule)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if(workSchedule.StaffId==0)
                {
                    _context.WorkSchedules.Remove(workSchedule);
                }
                else
                {
                    _context.WorkSchedules.Update(workSchedule);
                }
                _context.Requests.Update(request);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch(Exception ex) 
            {
                await transaction.RollbackAsync();
                throw new AppException(ErrorCode.DefaultError, ex);
            }
        }

        public void Delele(Request request)
        {
            _context.Requests.Remove(request);
            _context.SaveChanges();
        }

        public List<Request> GetByStaffId(int id)
        {
            List<Request> requests = _context.Requests.Where(r => r.StaffId == id).ToList();
            if (requests != null)
            {
                return requests;
            }
            return null;
        }

        public List<Request> GetDoneRequest(int id)
        {
            return _context.Requests.Include(r => r.Staff).Where(r => r.Status != 0 && r.StaffId == id).ToList();
        }

        public void Delele(int? id)
        {
            Request request = _context.Requests.FirstOrDefault(r => r.ReportId == id)!;
            _context.Requests.Remove(request);
            _context.SaveChanges();
        }
    }
}
