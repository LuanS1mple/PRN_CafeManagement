using CafeManagent.dto.response.attendance;
using CafeManagent.dto.response.NotifyModuleDTO;
using CafeManagent.Enums;
using CafeManagent.Hubs;
using CafeManagent.Models;
using CafeManagent.Services.Imp;
using CafeManagent.Services.Interface.AttendanceModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Controllers.Manager.AttendanceModule
{
    public class AttendanceController : Controller
    {
        private IAttendanceService attendanceService;
        private readonly IHubContext<ResponseHub> _hub;
        public AttendanceController(IAttendanceService attendanceService, IHubContext<ResponseHub> hub)
        {
            this.attendanceService = attendanceService;
            _hub = hub;
        }
        [Authorize(Roles = "Branch Manager")]
        public IActionResult ViewAllAttendance(string? keyword, DateTime? fromDate, DateTime? toDate)
        {
            DateOnly? from = fromDate.HasValue ? DateOnly.FromDateTime(fromDate.Value) : null;
            DateOnly? to = toDate.HasValue ? DateOnly.FromDateTime(toDate.Value) : null;

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
        [Authorize(Roles = "Branch Manager")]
        public async Task<IActionResult> CheckInPage(int? staffId, int? workshiftId, int? shiftId, DateOnly date)
        {
            int userId = HttpContext.Session.GetInt32("StaffId").Value;
            if (staffId == null || workshiftId == null || shiftId == null)
            {
                SystemNotify notify = new SystemNotify
                {
                    IsSuccess = false,
                    Message = NotifyMessage.PHAN_HOI_THIEU_THONG_TIN_CA_LAM.Message
                };
                
                ResponseHub.SetNotify(userId, notify);
                return RedirectToAction("WorkScheduleToday", "WorkSchedule");
            }

            var attendance = await attendanceService.GetAttendanceWithShiftAsync(workshiftId.Value, staffId.Value, DateOnly.FromDateTime(DateTime.Now), shiftId.Value);

            if (attendance == null)
            {
                SystemNotify notify = new SystemNotify
                {
                    IsSuccess = false,
                    Message = NotifyMessage.PHAN_HOI_THIEU_THONG_TIN_NHAN_VIEN.Message
                };
                ResponseHub.SetNotify(userId, notify);
                return RedirectToAction("WorkScheduleToday", "WorkSchedule");
            }

            return View(attendance);
        }
        [Authorize(Roles = "Branch Manager")]
        [HttpPost]
        public async Task<IActionResult> CheckIn(int staffId, int shiftId, int workshiftId, DateOnly date)
        {
            int userId = HttpContext.Session.GetInt32("StaffId").Value;
            try
            {
                var result = await attendanceService.CheckInAsync(workshiftId, shiftId, staffId, date);
                SystemNotify notify = new SystemNotify
                {
                    IsSuccess = true,
                    Message = NotifyMessage.CHECK_IN_THANH_CONG.Message + $"{result.Staff?.FullName} vào lúc {result.CheckIn?.ToString()}"
                };
                ResponseHub.SetNotify(userId, notify);
            }
            catch (Exception ex)
            {
                SystemNotify notify = new SystemNotify
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
                ResponseHub.SetNotify(userId, notify);
            }

            return RedirectToAction(nameof(CheckInPage), new { staffId, workshiftId, shiftId, date });
        }
        [Authorize(Roles = "Branch Manager")]
        [HttpPost]
        public async Task<IActionResult> CheckOut(int staffId, int shiftId, int workshiftId, DateOnly date)
        {
            int userId = HttpContext.Session.GetInt32("StaffId").Value;
            try
            {
                var result = await attendanceService.CheckOutAsync(workshiftId, shiftId, staffId, date);
                SystemNotify notify = new SystemNotify
                {
                    IsSuccess = true,
                    Message = NotifyMessage.CHECK_OUT_THANH_CONG.Message + $"{result.Staff?.FullName ?? ""} vào lúc {result.CheckOut?.ToString()}"
                };
                ResponseHub.SetNotify(userId , notify);
            }
            catch (Exception ex)
            {
                SystemNotify notify = new SystemNotify
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
                ResponseHub.SetNotify(userId, notify);
            }

            return RedirectToAction(nameof(CheckInPage), new { staffId, workshiftId, shiftId, date });
        }
        [Authorize(Roles = "Cashier,Barista")]
        public IActionResult AttendanceDetail(int? month, int? year)
        {
            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;
            int? staffId = HttpContext.Session.GetInt32("StaffId");
            if (staffId == null)
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
                    if (a.TotalHour >= 1)
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
