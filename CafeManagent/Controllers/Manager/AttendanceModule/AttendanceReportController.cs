using CafeManagent.dto.response.attendance;
using Microsoft.AspNetCore.Mvc;
using CafeManagent.Services.Interface.AttendanceModule;
using Microsoft.AspNetCore.Authorization;

namespace CafeManagent.Controllers.Manager.AttendanceModule
{
    public class AttendanceReportController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceReportController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [Authorize(Roles = "Branch Manager")]
        [HttpGet]
        public async Task<IActionResult> MonthlyAttendanceReport(int? month, int? year)
        {
            int reportMonth = month ?? DateTime.Now.Month;
            int reportYear = year ?? DateTime.Now.Year;

            // Truy vấn báo cáo
            var reports = await _attendanceService.GetMonthlyReportAsync(null, reportMonth, reportYear);

            ViewBag.SelectedMonth = reportMonth;
            ViewBag.SelectedYear = reportYear;

            return View(reports);
        }

        [Authorize(Roles = "Branch Manager")]
        [HttpGet]
        public async Task<IActionResult> ExportMonthlyReport(int? month, int? year)
        {
            int reportMonth = month ?? DateTime.Now.Month;
            int reportYear = year ?? DateTime.Now.Year;

            var reports = await _attendanceService.GetMonthlyReportAsync(null, reportMonth, reportYear);
            var file = await _attendanceService.ExportMonthlyReportToExcelAsync(reports);

            string fileName = $"BaoCaoThang_{reportMonth}_{reportYear}_TatCa.xlsx";

            return File(file,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }
    }
}
