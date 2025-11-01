using CafeManagent.dto.attendance;
using CafeManagent.Models;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
{
    public class AttendanceController : Controller
    {
        private IAttendanceService attendanceService;
        public AttendanceController(IAttendanceService attendanceService)
        {
            this.attendanceService = attendanceService;
        }
        public IActionResult ViewAllAttendance(string? keyword, DateTime? fromDate, DateTime? toDate)
        {
            DateOnly? from = fromDate.HasValue ? DateOnly.FromDateTime(fromDate.Value) : (DateOnly?)null;
            DateOnly? to = toDate.HasValue ? DateOnly.FromDateTime(toDate.Value) : (DateOnly?)null;

            var attendances = attendanceService.FilterAttendance(from, to, keyword)
                .Select(a => new AttendanceDTO
                {
                    AttendanceId = a.AttendanceId,
                    StaffName = a.Staff?.FullName,
                    ShiftName = a.Shift?.ShiftName,
                    Workdate = a.Workdate,
                    CheckIn = a.CheckIn,
                    CheckOut = a.CheckOut,
                    TotalHour = a.TotalHour,
                    Note = a.Note
                })
                .ToList();
            ViewBag.Keyword = keyword;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(attendances);
        }
        public IActionResult CheckInPage(int staffId, int shiftId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var schedule = attendanceService.GetAttendanceWithShift(shiftId, staffId, today);
            if (schedule == null)
            {
                TempData["error"] = "Không tìm thấy lịch làm hôm nay";
                return RedirectToAction("ViewAllAttendance");
            }
            return View(schedule);
        }
        



    }
}
