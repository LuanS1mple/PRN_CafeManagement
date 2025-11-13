using CafeManagent.Models;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task; 
namespace CafeManagent.Services
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
        Task CreateTasksAsync(Models.Task task);
    }
}