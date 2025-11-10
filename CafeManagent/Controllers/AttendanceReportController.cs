using CafeManagent.Services;
using CafeManagent.dto.attendance;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
{
    public class AttendanceReportController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceReportController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

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
