using CafeManagent.dto.response;
using CafeManagent.Hubs;
using CafeManagent.Models;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Task = System.Threading.Tasks.Task;

namespace CafeManagent.Controllers.Manager
{
    public class RequestController : Controller
    {
        private readonly IRequestService requestService;
        private readonly IAttendanceService attendanceService;
        //Hub
        private readonly IHubContext<ResponseHub> _hub;
        public RequestController(IRequestService requestService, IAttendanceService attendanceService,IHubContext<ResponseHub> hub)
        {
            this.requestService = requestService;
            this.attendanceService = attendanceService;
            this._hub = hub;
        }
        public IActionResult Index()
        {
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
            return View("Request_List", waitingRequests);
        }
        public IActionResult DetailAttendanceRequest(int id)
        {
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
                attendance = new Attendance() {
                    StaffId = request.StaffId,
                    ShiftId = changeData?.WorkShiftId,
                    WorkshiftId = changeData.WorkShiftId
                };
            }
            //cập nhập lại request
            request.Status = 1;
            request.ResolvedBy = 1;
            request.ResolvedDate = DateTime.Now;
            //serivce
            await requestService.AcceptRequest(request, attendance);
            //hub
            await _hub.Clients.All.SendAsync("ReceiveResponseStatus", true, "Phản hồi thành công");
            await Task.Delay(2000); 
            return RedirectToAction("Index");   
        }
        public async Task<IActionResult> RejectAttendanceRequest(int id)
        {
            Request request = requestService.GetById(id);
            //cập nhập lại request
            request.Status = 2;
            request.ResolvedBy = 1;
            request.ResolvedDate = DateTime.Now;
            //serivce
            await requestService.RejectRequest(request);
            //hub
            await _hub.Clients.All.SendAsync("ReceiveResponseStatus", false, "Phản hồi thất bại, vui lòng thử lại");
            return RedirectToAction("Index");

        }
    }
}
