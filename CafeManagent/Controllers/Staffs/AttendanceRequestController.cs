using CafeManagent.dto.request;
using CafeManagent.mapper;
using CafeManagent.Models;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers.Staff
{
    public class AttendanceRequestController : Controller
    {
        private readonly IAttendanceService attendanceService;
        private readonly IRequestService requestService;
        public AttendanceRequestController(IAttendanceService attendanceService, IRequestService requestService)
        {
            this.attendanceService = attendanceService;
            this.requestService = requestService;
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
        public IActionResult SubmitRequest(AttendanceRequestDTO requestDTO)
        {
            //tạm
            requestDTO.StaffId = 1;
            Request request = MapperHelper.GetRequestFromAttendanceRequestDTO(requestDTO);
            requestService.Add(request);
            return RedirectToAction("Init");
        }
    }
}
