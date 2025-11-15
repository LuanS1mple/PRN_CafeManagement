using CafeManagent.dto.request.Task;
using CafeManagent.Models;
using CafeManagent.Services.Interface.TaskModule;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp.TaskModule
{
    public class TaskReportService : ITaskReportService
    {
        private readonly CafeManagementContext _context;

        public TaskReportService(CafeManagementContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<int, int>> GetTaskSummaryAsync()
        {
            try
            {
                var taskSummary = await _context.Tasks
                .Where(t => t.Status.HasValue)
                .GroupBy(t => t.Status.Value)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

                return taskSummary.ToDictionary(x => x.Status, x => x.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new Dictionary<int, int>();
            }
        }

        public async Task<List<TaskReportDTO>> GetTasksByStatusAsync(int statusId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.Staff)
                .Include(t => t.Manager)
                .Include(t => t.Tasktype)
                .Where(t => t.Status == statusId)
                .Select(t => new TaskReportDTO
                {
                    TaskId = t.TaskId,
                    TasktypeName = t.Tasktype.TaskName,
                    Description = t.Tasktype.Description,
                    StaffName = t.Staff.FullName,
                    ManagerName = t.Manager.FullName,
                    AssignTime = t.AssignTime,
                    DueTime = t.DueTime,
                    Status = t.Status
                })
                .ToListAsync();

            return tasks;
        }
        public async Task<byte[]> GenerateExcelReportAsync()
        {
            // Lấy toàn bộ dữ liệu thống kê tổng hợp nhóm theo trạng thái
            var summaryData = await GetTaskSummaryAsync();

            using (var workbook = new XLWorkbook())
            {
                // SHEET 1: BẢNG TỔNG HỢP SỐ LƯỢNG THEO TRẠNG THÁI
                var wsSummary = workbook.Worksheets.Add("Tổng hợp");
                wsSummary.Cell("A1").Value = "Trạng Thái";
                wsSummary.Cell("B1").Value = "Số Lượng";
                wsSummary.Row(1).Style.Font.Bold = true;

                int summaryRow = 2;
                foreach (var item in summaryData.OrderBy(kv => kv.Key))
                {
                    wsSummary.Cell(summaryRow, 1).Value = GetStatusLabel(item.Key);
                    wsSummary.Cell(summaryRow, 2).Value = item.Value;
                    summaryRow++;
                }
                wsSummary.Columns().AdjustToContents();

                // CÁC SHEET CHI TIẾT: MỖI TRẠNG THÁI MỘT SHEET
                foreach (var item in summaryData)
                {
                    int statusId = item.Key;
                    string sheetName = GetStatusLabel(statusId);

                    var tasks = await GetTasksByStatusAsync(statusId);

                    var wsDetail = workbook.Worksheets.Add(sheetName);

                    // Tiêu đề cho bảng chi tiết
                    wsDetail.Cell("A1").Value = "ID";
                    wsDetail.Cell("B1").Value = "Tên Công Việc";
                    wsDetail.Cell("C1").Value = "Mô Tả";
                    wsDetail.Cell("D1").Value = "Nhân Viên";
                    wsDetail.Cell("E1").Value = "Quản Lý";
                    wsDetail.Cell("F1").Value = "Bắt đầu";
                    wsDetail.Cell("G1").Value = "Thời Hạn";
                    wsDetail.Cell("H1").Value = "Trạng Thái";
                    wsDetail.Row(1).Style.Font.Bold = true;

                    // Đổ dữ liệu chi tiết
                    int detailRow = 2;
                    foreach (var task in tasks)
                    {
                        wsDetail.Cell(detailRow, 1).Value = task.TaskId;
                        wsDetail.Cell(detailRow, 2).Value = task.TasktypeName;
                        wsDetail.Cell(detailRow, 3).Value = task.Description;
                        wsDetail.Cell(detailRow, 4).Value = task.StaffName;
                        wsDetail.Cell(detailRow, 5).Value = task.ManagerName;
                        wsDetail.Cell(detailRow, 6).Value = task.AssignTime;
                        wsDetail.Cell(detailRow, 7).Value = task.DueTime;
                        wsDetail.Cell(detailRow, 8).Value = GetStatusLabel(task.Status ?? -1);

                        // Format ngày giờ
                        wsDetail.Cell(detailRow, 6).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                        wsDetail.Cell(detailRow, 7).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                        detailRow++;
                    }
                    wsDetail.Columns().AdjustToContents();
                }

                // XUẤT FILE EXCEL VÀ TRẢ KẾT QUẢ DƯỚI DẠNG BYTE[]
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        private string GetStatusLabel(int status)
        {
            return status switch
            {
                0 => "Chưa bắt đầu",
                1 => "Đang tiến hành",
                2 => "Hoàn thành",
                3 => "Đã hủy",
                4 => "Hết hạn",
                5 => "Làm lại",
                6 => "Chờ xác nhận",
                _ => "Không xác định",
            };
        }

    }
}
