using CafeManagent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafeManagent.Controllers
{
    public class ReportController : Controller
    {
        private readonly CafeManagementContext _context; 
        // Giả định Status Codes:
        private const int STATUS_COMPLETED = 3;
        private const int STATUS_REFUNDED = -2;  

        public ReportController(CafeManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View("RevenueReport"); 
        }

  
        public class RevenueReportDto
        {
            public decimal TotalRevenue { get; set; }
            public int CompletedOrders { get; set; }
            public int RefundedOrders { get; set; }
            public decimal CompensationAmount { get; set; }
            public List<string> ChartLabels { get; set; } = new List<string>();
            public List<decimal> ChartData { get; set; } = new List<decimal>();
        }

        // API Endpoint để lấy dữ liệu báo cáo
        [HttpGet]
        public async Task<IActionResult> GetRevenueReportData(DateTime startDate, DateTime endDate)
        {
            // Đảm bảo thời gian kết thúc bao gồm cả cuối ngày
            endDate = endDate.Date.AddDays(1).AddSeconds(-1);

            // 1. Lấy dữ liệu Order trong khoảng thời gian
            var orders = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && (o.Status == STATUS_COMPLETED || o.Status == STATUS_REFUNDED))
                .ToListAsync();

            // 2. Tính toán KPI
            var completedOrders = orders.Where(o => o.Status == STATUS_COMPLETED).ToList();
            var refundedOrders = orders.Where(o => o.Status == STATUS_REFUNDED).ToList();

            decimal totalRevenue = completedOrders.Sum(o => o.OrderPrice ?? 0);
            int completedCount = completedOrders.Count;
            int refundedCount = refundedOrders.Count;

            // Giả định tiền đền bù chính là tổng giá trị các đơn hàng bị hoàn tiền
            decimal compensationAmount = refundedOrders.Sum(o => o.OrderPrice ?? 0);

            // 3. Xử lý dữ liệu cho Biểu đồ (Nhóm theo Ngày)
            var chartDataGrouped = orders
                .Where(o => o.Status == STATUS_COMPLETED)
                .GroupBy(o => o.OrderDate.Value.Date) // Nhóm theo ngày
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.OrderPrice ?? 0)
                })
                .OrderBy(x => x.Date)
                .ToList();


            var labels = chartDataGrouped.Select(x => x.Date.ToString("dd/MM")).ToList();
            var dataPoints = chartDataGrouped.Select(x => x.Revenue).ToList();

            var reportData = new RevenueReportDto
            {
                TotalRevenue = totalRevenue,
                CompletedOrders = completedCount,
                RefundedOrders = refundedCount,
                CompensationAmount = compensationAmount,
                ChartLabels = labels,
                ChartData = dataPoints
            };

            return Json(reportData);
        }
    }
}