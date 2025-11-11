using CafeManagent.dto.request;
using CafeManagent.dto.response;
using CafeManagent.Enums;
using CafeManagent.Hubs;
using CafeManagent.mapper;
using CafeManagent.Models;
using CafeManagent.Services;
using CafeManagent.Ulties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Controllers.Staff
{
    public class AttendanceRequestController : Controller
    {
        private readonly IAttendanceService attendanceService;
        private readonly IRequestService requestService;
        //hub
        private readonly NotifyUlti notifyUlti;
        public AttendanceRequestController(IAttendanceService attendanceService, IRequestService requestService, NotifyUlti notifyUlti)
        {
            this.attendanceService = attendanceService;
            this.requestService = requestService;
            this.notifyUlti = notifyUlti;
        }
        public IActionResult Init()
        {
            return View();
        }
        public IActionResult GetAttendance(DateOnly workDate, int workshiftId)
        {
            Attendance attendance = attendanceService.GetAttendance(workDate, workshiftId, 1);
            if (attendance != null)
            {
                return Json(new
                {
                    checkIn = attendance.CheckIn,
                    checkOut = attendance.CheckOut,
                });
            }
            else
            {
                return Json(new
                {
                    checkIn = "",
                    checkOut = "",
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SubmitRequest(AttendanceRequestDTO requestDTO)
        {
            //tạm
            requestDTO.StaffId = 1;
            Request request = MapperHelper.GetRequestFromAttendanceRequestDTO(requestDTO);
            requestService.Add(request);

            //thêm thông báomwis
            Notify notify = new Notify() {
                Message = NotifyMessage.HAVE_REQUEST.Message,
                Time = DateTime.Now,
            };
            notifyUlti.AddManager(notify);

            return RedirectToAction("Init");
        }
    }
}
