using CafeManagent.dto.Task;

namespace CafeManagent.Services
{
    public interface ITaskReportService
    {
        Task<Dictionary<int, int>> GetTaskSummaryAsync();
        Task<List<TaskReportDTO>> GetTasksByStatusAsync(int statusId);
    }
}
