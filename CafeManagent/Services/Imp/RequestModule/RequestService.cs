using CafeManagent.ErrorHandler;
using CafeManagent.Models;
using CafeManagent.Services.Interface.RequestModuleDTO;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace CafeManagent.Services.Imp.RequestModule
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
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw new AppException(ErrorCode.LOI_CHAP_NHAN_REQUEST, ex);
            }
        }

        public void Add(Request request)
        {
            try
            {
                _context.Requests.Add(request);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new AppException(ErrorCode.LOI_THEM_REQUEST, ex);
            }
        }

        public Request GetById(int id)
        {
            try
            {
                return _context.Requests.Include(r => r.Staff)
                .Where(r => r.ReportId == id).First();
            }
            catch(Exception e)
            {
                throw new AppException(ErrorCode.LOI_LAY_REQUEST, e);
            }
        }

        public List<Request> GetWaitingAttendanceRequest()
        {
            try
            {
                return _context.Requests.Include(r => r.Staff)
              .Where(r => r.ReportType.Equals("Attendance") && r.Status == 0).ToList();
            }
            catch( Exception e)
            {
                throw new AppException(ErrorCode.LOI_PHAN_LOAI_REQUEST, e);
            }
        }

        public List<Request> GetWaitingShiftRequest()
        {
            try
            {
                return _context.Requests.Include(r => r.Staff).Where(r => r.ReportType.Equals("WorkSchedule") && r.Status == 0).ToList();
            }
            catch(Exception ex)
            {
                throw new AppException(ErrorCode.LOI_LAY_REQUEST, ex);
            }
        }
        public List<Request> GetDoneRequest()
        {
            try
            {
                return _context.Requests.Include(r => r.Staff).Where(r => r.Status != 0).ToList();

            }
            catch (Exception ex)
            {
                throw new AppException(ErrorCode.LOI_LAY_REQUEST, ex);
            }
        }
        public async Task RejectRequest(Request request)
        {
            try
            {
                _context.Requests.Update(request);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw new AppException(ErrorCode.LOI_TU_CHOI_REQUEST, ex);
            }
        }

        public async Task AcceptWorkScheduleRequest(Request request, WorkSchedule workSchedule)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (workSchedule.StaffId == 0)
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new AppException(ErrorCode.DefaultError, ex);
            }
        }

        public void Delele(Request request)
        {
            try
            {
                _context.Requests.Remove(request);
                _context.SaveChanges();
            }
            catch(Exception e)
            {
                throw new AppException(ErrorCode.LOI_XOA_REQUEST, e);
            }
        }

        public List<Request> GetByStaffId(int id)
        {
            try
            {
                List<Request> requests = _context.Requests.Where(r => r.StaffId == id).ToList();
                if (requests != null)
                {
                    return requests;
                }
                return null;
            }
            catch (Exception e)
            {
                throw new AppException(ErrorCode.LOI_LAY_REQUEST, e);
            }
        }

        public List<Request> GetDoneRequest(int id)
        {
            try
            {
                return _context.Requests.Include(r => r.Staff).Where(r => r.Status != 0 && r.StaffId == id).ToList();
            }
            catch (Exception e)
            {
                throw new AppException(ErrorCode.LOI_LAY_REQUEST, e);
            }
        }

        public void Delele(int? id)
        {
            try
            {
                Request request = _context.Requests.FirstOrDefault(r => r.ReportId == id)!;
                _context.Requests.Remove(request);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new AppException(ErrorCode.LOI_XOA_REQUEST, e);
            }
        }
    }
}
