using CafeManagent.Models;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
namespace CafeManagent.Services.Interface.TaskModule
{
    public interface ITaskService
    {
        Task<IEnumerable<Models.Task>> GetTasksAsync(
            string searchString,
            string statusFilter,
            string startDate,
            string endDate);

        Task<bool> UpdateTaskAsync(Models.Task updateTask);
        Task<IEnumerable<Staff>> GetManagersAsync();
        Task<IEnumerable<Staff>> GetAllStaffAsync();
        Task<IEnumerable<TaskType>> GetTaskTypesAsync();
        Task CreateTasksAsync(Models.Task task, string currentManagerId);
        Task<Models.Task> GetTaskByIdAsync(int taskId);
        Task<IEnumerable<Models.Task>> GetTasksByStaffIdAsync(int staffId);
        Task<bool> UpdateTaskStatusAsync(int taskId, int newStatsu, int staffId);
    }
}