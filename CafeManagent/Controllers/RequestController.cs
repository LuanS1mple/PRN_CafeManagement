using CafeManagent.dto.response;
using CafeManagent.Models;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
{
    public class RequestController : Controller
    {
        private readonly IRequestService requestService;
        private readonly IAttendanceService attendanceService;
        public RequestController(IRequestService requestService, IAttendanceService attendanceService)
        {
            this.requestService = requestService;
            this.attendanceService = attendanceService;
        }
        public IActionResult Index()
        {
            WaitingRequests waitingRequests = new WaitingRequests() {
                AttendanceRequests = requestService.GetWaitingAttendanceRequest().Select(r => new RequestBasic
                {
                    Id = r.ReportId,
                    Date = r.ReportDate,
                    Description= r.Description,
                    StaffName= r.Staff.FullName,
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
            var changeData = new { 
                WorkDate = DateOnly.Parse(values[0]),
                WorkShiftId = int.Parse(values[1]),
                NewCheckIn = TimeOnly.Parse(values[2]),
                NewCheckOut = TimeOnly.Parse(values[3]),
            };
            Attendance attendance = attendanceService.GetAttendance(changeData.WorkDate, changeData.WorkShiftId, request.StaffId.Value);
            PendingAttendance pendingAttendance = new PendingAttendance
            {
                Date = changeData.WorkDate,
                Description = request.Description,
                NewCheckIn = changeData.NewCheckIn,
                NewCheckOut = changeData.NewCheckOut,
                OldCheckIn = attendance?.CheckIn??null,
                OldCheckOut = attendance?.CheckOut??null,
                 StaffId = request.StaffId.Value,
                 ShiftId = changeData.WorkShiftId,
                 StaffName = request.Staff.FullName,
                 Title = request.Title
            };
            return View(pendingAttendance);
        }
    }
}
