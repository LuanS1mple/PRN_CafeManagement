namespace CafeManagent.dto.response.OrderModuleDTO
{
    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int CompletedOrders { get; set; }
        public int RefundedOrders { get; set; }
        public decimal CompensationAmount { get; set; }
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<decimal> ChartData { get; set; } = new List<decimal>();
    }
}
