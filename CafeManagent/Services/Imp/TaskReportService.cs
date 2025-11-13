using CafeManagent.dto.Task;
using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp
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
                Console.WriteLine($"Error: {ex.Message}" );
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

    }
}
