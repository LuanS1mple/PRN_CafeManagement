using CafeManagent.dto.request.RequestModuleDTO;
using CafeManagent.dto.response.NotifyModuleDTO;
using CafeManagent.Enums;
using CafeManagent.Hubs;
using CafeManagent.mapper;
using CafeManagent.Models;
using CafeManagent.Services.Interface.AttendanceModule;
using CafeManagent.Services.Interface.RequestModuleDTO;
using CafeManagent.Ulties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Controllers.Staffs.RequestModule
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
        [Authorize(Roles = "Cashier,Barista")]
        public IActionResult Init()
        {
            return View();
        }
        [Authorize(Roles = "Cashier,Barista")]
        public IActionResult GetAttendance(DateOnly workDate, int workshiftId)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            Attendance attendance = attendanceService.GetAttendance(workDate, workshiftId, staffId);
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
        [Authorize(Roles = "Cashier,Barista")]
        [HttpPost]
        public async Task<IActionResult> SubmitRequest(AttendanceRequestDTO requestDTO)
        {
            if (!ModelState.IsValid)
            {
                return View("Init", requestDTO);
            }
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            requestDTO.StaffId = staffId;
            Request request = MapperHelper.GetRequestFromAttendanceRequestDTO(requestDTO);
            requestService.Add(request);

            //thêm thông báomwis
            Notify notify = new Notify()
            {
                Message = NotifyMessage.HAVE_REQUEST.Message,
                Time = DateTime.Now,
            };
            notifyUlti.AddManager(notify);

            return RedirectToAction("Init");
        }
    }
}
