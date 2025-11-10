using CafeManagent.dto.attendance;
using CafeManagent.Models;
using CafeManagent.Services;
using CafeManagent.Services.Imp;
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
                    Note = a.Note,
                    Status = a.Status
                })
                .ToList();
            ViewBag.Keyword = keyword;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(attendances);
        }
        public async Task<IActionResult> CheckInPage(int? staffId, int? workshiftId, int? shiftId, DateOnly date)
        {
            if (staffId == null || workshiftId == null || shiftId == null)
            {
                TempData["error"] = "Thiếu thông tin nhân viên hoặc ca làm việc!";
                return RedirectToAction(nameof(ViewAllAttendance));
            }

            var attendance = await attendanceService.GetAttendanceWithShiftAsync(workshiftId.Value, staffId.Value, date, shiftId.Value);

            if (attendance == null)
            {
                TempData["error"] = "Không tìm thấy ca làm của nhân viên trong ngày!";
                return RedirectToAction(nameof(ViewAllAttendance));
            }

            return View(attendance);
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn(int staffId, int shiftId, int workshiftId, DateOnly date)
        {
            try
            {
                var result = await attendanceService.CheckInAsync(workshiftId, shiftId, staffId, date);
                TempData["success"] = $"Nhân viên {result.Staff?.FullName ?? ""} đã Check-In thành công vào lúc {result.CheckIn?.ToString()}";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(CheckInPage), new { staffId, workshiftId, shiftId, date });
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut(int staffId, int shiftId, int workshiftId, DateOnly date)
        {
            try
            {
                var result = await attendanceService.CheckOutAsync(workshiftId, shiftId, staffId, date);
                TempData["success"] = $"Nhân viên {result.Staff?.FullName ?? ""} đã Check-Out thành công vào lúc {result.CheckOut?.ToString()}";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(CheckInPage), new { staffId, workshiftId, shiftId, date });
        }

        public IActionResult AttendanceDetail(int? month, int? year)
        {
            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;
            int? staffId = HttpContext.Session.GetInt32("StaffId");
            if(staffId == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var list = attendanceService.GetAttendanceByMonth(staffId, selectedMonth, selectedYear);
            int totalDays = 0;
            decimal? totalhour = 0;
            foreach (var a in list)
            {
                if (a.CheckIn != null && a.CheckOut != null)
                {
                    if(a.TotalHour >= 1)
                    {
                        totalDays++;
                        totalhour += a.TotalHour;
                    }
                }

            }
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.TotalDays = totalDays;
            ViewBag.TotalHours = totalhour;
            return View(list);
        }
    }
}
