using CafeManagent.dto.request.Task;

namespace CafeManagent.Services.Interface.TaskModule
{
    public interface ITaskReportService
    {
        Task<Dictionary<int, int>> GetTaskSummaryAsync();
        Task<List<TaskReportDTO>> GetTasksByStatusAsync(int statusId);
        Task<byte[]> GenerateExcelReportAsync();
    }
}
