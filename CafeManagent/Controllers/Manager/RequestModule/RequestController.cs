using CafeManagent.dto.response;
using CafeManagent.dto.response.NotifyModuleDTO;
using CafeManagent.dto.response.RequestModuleDTO;
using CafeManagent.Enums;
using CafeManagent.Hubs;
using CafeManagent.Models;
using CafeManagent.Services.Interface.AttendanceModule;
using CafeManagent.Services.Interface.RequestModuleDTO;
using CafeManagent.Services.Interface.WorkScheduleModule;
using CafeManagent.Ulties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Task = System.Threading.Tasks.Task;

namespace CafeManagent.Controllers.Manager.RequestModule
{
    public class RequestController : Controller
    {
        private readonly IRequestService requestService;
        private readonly IAttendanceService attendanceService;
        private readonly IWorkScheduleService workScheduleService;
        //Hub
        private readonly IHubContext<ResponseHub> _hub;
        public RequestController(IRequestService requestService, IAttendanceService attendanceService, IHubContext<ResponseHub> hub, IWorkScheduleService workScheduleService)
        {
            this.requestService = requestService;
            this.attendanceService = attendanceService;
            _hub = hub;
            this.workScheduleService = workScheduleService;
        }
        public IActionResult Index(int? page)
        {
            //data cho phần cần phản hồi
            WaitingRequests waitingRequests = new WaitingRequests()
            {
                AttendanceRequests = requestService.GetWaitingAttendanceRequest().Select(r => new RequestBasic
                {
                    Id = r.ReportId,
                    Date = r.ReportDate,
                    Description = r.Description,
                    StaffName = r.Staff.FullName,
                    Title = r.Title
                }).ToList(),
                ShiftRequests = requestService.GetWaitingShiftRequest().Select(r => new RequestBasic
                {
                    Id = r.ReportId,
                    Date = r.ReportDate,
                    Description = r.Description,
                    StaffName = r.Staff.FullName,
                    Title = r.Title
                }).ToList(),
            };
            //paging
            int pageIndex = 1;
            if (page != null)
            {
                pageIndex = page.Value;
            }
            Paging<RequestBasic> paging = PagingUlti.PagingDoneRequest(requestService.GetDoneRequest(), pageIndex);
            ViewBag.Paging = paging;

            return View("Request_List", waitingRequests);
        }
        public IActionResult DetailAttendanceRequest(int id)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            Request request = requestService.GetById(id);
            string detail = request.Detail;
            string[] values = detail.Split(";");
            var changeData = new
            {
                WorkDate = DateOnly.Parse(values[0]),
                WorkShiftId = int.Parse(values[1]),
                NewCheckIn = TimeOnly.Parse(values[2]),
                NewCheckOut = TimeOnly.Parse(values[3]),
            };
            Attendance attendance = attendanceService.GetAttendance(changeData.WorkDate, changeData.WorkShiftId, request.StaffId.Value);
            PendingAttendance pendingAttendance = new PendingAttendance
            {
                RequestId = id,
                Date = changeData.WorkDate,
                Description = request.Description,
                NewCheckIn = changeData.NewCheckIn,
                NewCheckOut = changeData.NewCheckOut,
                OldCheckIn = attendance?.CheckIn ?? null,
                OldCheckOut = attendance?.CheckOut ?? null,
                StaffId = request.StaffId.Value,
                ShiftId = changeData.WorkShiftId,
                StaffName = request.Staff.FullName,
                Title = request.Title
            };
            return View(pendingAttendance);
        }
        public async Task<IActionResult> AcceptAttendanceRequest(int id)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            Request request = requestService.GetById(id);
            string detail = request.Detail;
            string[] values = detail.Split(";");
            var changeData = new
            {
                WorkDate = DateOnly.Parse(values[0]),
                WorkShiftId = int.Parse(values[1]),
                NewCheckIn = TimeOnly.Parse(values[2]),
                NewCheckOut = TimeOnly.Parse(values[3]),
            };
            //cập nhập lại attendance
            Attendance attendance = attendanceService.GetAttendance(changeData.WorkDate, changeData.WorkShiftId, request.StaffId.Value);
            if (attendance != null)
            {
                attendance.CheckIn = changeData.NewCheckIn;
                attendance.CheckOut = changeData.NewCheckOut;
            }
            else
            {
                attendance = new Attendance()
                {
                    StaffId = request.StaffId,
                    ShiftId = changeData?.WorkShiftId,
                    CheckIn = changeData.NewCheckIn,
                    CheckOut = changeData.NewCheckOut,
                    Workdate = changeData.WorkDate,
                    Status = 1,
                    TotalHour = (decimal)(changeData.NewCheckOut.ToTimeSpan() - changeData.NewCheckIn.ToTimeSpan()).TotalHours
                };
            }
            //cập nhập lại request
            request.Status = 1;
            request.ResolvedBy = 1;
            request.ResolvedDate = DateTime.Now;
            //serivce
            await requestService.AcceptAttendanceRequest(request, attendance);
            //hub
            SystemNotify systemNotify = new SystemNotify()
            {
                IsSuccess = true,
                Message = NotifyMessage.PHAN_HOI_THANH_CONG.Message
            };
            ResponseHub.SetNotify(staffId, systemNotify);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> RejectAttendanceRequest(int id)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            Request request = requestService.GetById(id);
            //cập nhập lại request
            request.Status = 2;
            request.ResolvedBy = 1;
            request.ResolvedDate = DateTime.Now;
            //serivce
            await requestService.RejectRequest(request);
            //hub
            SystemNotify systemNotify = new SystemNotify()
            {
                IsSuccess = false,
                Message = NotifyMessage.PHAN_HOI_THANH_CONG.Message
            };
            ResponseHub.SetNotify(staffId, systemNotify);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult DetailWorkscheduleRequest(int id)
        {
            //int staffId = 1;
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            Request request = requestService.GetById(id);
            string detail = request.Detail;
            string[] values = detail.Split(";");
            var changeData = new
            {
                WorkDate = DateOnly.Parse(values[0]),
                OldShiftId = int.Parse(values[1]),
                NewShiftId = values[2].Equals("cancel", StringComparison.OrdinalIgnoreCase) ? 0 : int.Parse(values[2]),
            };
            WorkSchedule workSchedule = workScheduleService.Get(request.StaffId.Value, changeData.OldShiftId, changeData.WorkDate);
            //khi lich bi loi (2 request tren 1 lich)
            if (workSchedule == null)
            {
                requestService.Delele(request);
                ResponseHub.SetNotify(staffId, new SystemNotify { IsSuccess = false, Message = NotifyMessage.GET_REQUEST_THAT_BAI.Message });
                return RedirectToAction("Index");
            }
            PendingWorkSchedule pendingWorkSchedule = new PendingWorkSchedule
            {
                RequestId = id,
                Date = changeData.WorkDate,
                Description = request.Description,
                StaffId = request.StaffId.Value,
                StaffName = request.Staff.FullName,
                Title = request.Title,
                ShiftId = workSchedule.ShiftId,
                NewShift = changeData.NewShiftId,
                OldShift = changeData.OldShiftId,
            };
            return View("DetailWorkscheduleRequest", pendingWorkSchedule);
        }
        public async Task<IActionResult> AcceptWorkScheduleRequest(int id)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            Request request = requestService.GetById(id);
            string detail = request.Detail;
            string[] values = detail.Split(";");
            var changeData = new
            {
                WorkDate = DateOnly.Parse(values[0]),
                OldShiftId = int.Parse(values[1]),
                NewShiftId = values[2].Equals("cancel", StringComparison.OrdinalIgnoreCase) ? 0 : int.Parse(values[2]),
            };
            //cập nhập lại workschedule
            WorkSchedule workSchedule = workScheduleService.Get(request.StaffId.Value, changeData.OldShiftId, changeData.WorkDate);
            if (values[2].Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                //xoa ca
                workSchedule.StaffId = 0;
            }
            else
            {
                workSchedule.WorkshiftId = changeData.NewShiftId;
                workScheduleService.Update(workSchedule);
            }
            //cập nhập lại request
            request.Status = 1;
            request.ResolvedBy = staffId;
            request.ResolvedDate = DateTime.Now;
            //serivce
            await requestService.AcceptWorkScheduleRequest(request, workSchedule);
            //hub
            SystemNotify systemNotify = new SystemNotify()
            {
                IsSuccess = true,
                Message = NotifyMessage.PHAN_HOI_THANH_CONG.Message
            };
            ResponseHub.SetNotify(staffId, systemNotify);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> RejectWorkScheduleRequest(int id)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            Request request = requestService.GetById(id);
            //cập nhập lại request
            request.Status = 2;
            request.ResolvedBy = 1;
            request.ResolvedDate = DateTime.Now;
            //serivce
            await requestService.RejectRequest(request);
            //hub
            SystemNotify systemNotify = new SystemNotify()
            {
                IsSuccess = false,
                Message = NotifyMessage.PHAN_HOI_THANH_CONG.Message
            };
            ResponseHub.SetNotify(staffId, systemNotify);
            return RedirectToAction("Index");

        }
    }
}
