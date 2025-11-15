using CafeManagent.Hubs;
using CafeManagent.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using CafeManagent.Services.Interface.TaskModule;
using Task = System.Threading.Tasks.Task;
using CafeManagent.dto.response.TaskModuleDTO;

namespace CafeManagent.Services.Imp.TaskModule
{
    public class TaskService : ITaskService
    {
        private readonly CafeManagementContext _context;
        private readonly IHubContext<TaskHub> _taskHub;
        private readonly IHubContext<NotifyHub> _notifyHub;
        private readonly IHttpContextAccessor _httpContext;

        public TaskService(
            CafeManagementContext context,
            IHubContext<TaskHub> taskHub,
            IHubContext<NotifyHub> notifyHub,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _taskHub = taskHub;
            _notifyHub = notifyHub;
            _httpContext = httpContextAccessor;
        }

        private async System.Threading.Tasks.Task SendTaskUpdateAsync(Models.Task task)
        {
            var taskFull = await _context.Tasks
                .Include(t => t.Tasktype)
                .Include(t => t.Manager)
                .Include(t => t.Staff)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskId == task.TaskId);

            if (taskFull == null) return;

            var taskDto = new
            {
                taskFull.TaskId,
                taskFull.TasktypeId,
                tasktypeName = taskFull.Tasktype?.TaskName,
                description = taskFull.Tasktype?.Description,
                taskFull.ManagerId,
                managerName = taskFull.Manager?.FullName,
                taskFull.StaffId,
                staffName = taskFull.Staff?.FullName,
                assignTime = taskFull.AssignTime,
                dueTime = taskFull.DueTime,
                status = taskFull.Status
            };

            await _taskHub.Clients.All.SendAsync("ReceiveTaskUpdate", taskDto);

            if (taskFull.StaffId.HasValue)
            {
                var notify = new
                {
                    IsSuccess = true,
                    Message = $"Công việc [{taskFull.Tasktype?.TaskName}] đã được cập nhật."
                };

                await _notifyHub.Clients.User(taskFull.StaffId.Value.ToString())
                    .SendAsync("ReceiveNotify", notify);
            }
        }

        public async Task<IEnumerable<Models.Task>> GetTasksAsync(
            string searchString, string statusFilter, string startDate, string endDate)
        {
            var query = _context.Tasks
                .Include(t => t.Staff)
                .Include(t => t.Manager)
                .Include(t => t.Tasktype)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t =>
                    (t.Staff != null && t.Staff.FullName.Contains(searchString)) ||
                    (t.Manager != null && t.Manager.FullName.Contains(searchString)) ||
                    (t.Tasktype != null && t.Tasktype.TaskName.Contains(searchString))
                );
            }

            if (!string.IsNullOrEmpty(statusFilter) && int.TryParse(statusFilter, out int s))
                query = query.Where(t => t.Status == s);

            if (DateTime.TryParse(startDate, out DateTime sd))
                query = query.Where(t => t.AssignTime >= sd);

            if (DateTime.TryParse(endDate, out DateTime ed))
                query = query.Where(t => t.AssignTime <= ed);

            return await query.OrderByDescending(t => t.AssignTime).ToListAsync();
        }

        public async Task<bool> UpdateTaskAsync(Models.Task updateTask)
        {
            var task = await _context.Tasks.FindAsync(updateTask.TaskId);
            if (task == null) return false;

            task.TasktypeId = updateTask.TasktypeId;
            task.DueTime = updateTask.DueTime;
            task.StaffId = updateTask.StaffId == 0 ? null : updateTask.StaffId;
            task.Status = updateTask.Status;

            await _context.SaveChangesAsync();
            _context.Entry(task).State = EntityState.Detached;

            await SendTaskUpdateAsync(task);
            return true;
        }

        public async Task CreateTasksAsync(Models.Task task, string currentManagerId)
        {
            if (!int.TryParse(currentManagerId, out int managerId))
                throw new Exception("Không thể xác định ManagerId từ tham số truyền vào.");

            task.ManagerId = managerId;
            task.AssignTime = DateTime.Now;
            task.Status = 0;
            task.StaffId = task.StaffId == 0 ? null : task.StaffId;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            await SendTaskUpdateAsync(task);
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, int newStatus)
        {
            int? staffId = _httpContext.HttpContext?.Session.GetInt32("StaffId");
            if (staffId == null) return false;

            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null || task.StaffId != staffId) return false;

            task.Status = newStatus;

            await _context.SaveChangesAsync();
            _context.Entry(task).State = EntityState.Detached;

            await SendTaskUpdateAsync(task);
            return true;
        }

        public async Task<Models.Task> GetTaskByIdAsync(int taskId)
        {
            return await _context.Tasks
                .Include(t => t.Staff)
                .Include(t => t.Manager)
                .Include(t => t.Tasktype)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskId == taskId);
        }

        public async Task<IEnumerable<Models.Task>> GetTasksByStaffIdAsync(int staffId)
        {
            return await _context.Tasks
                .Include(t => t.Staff)
                .Include(t => t.Manager)
                .Include(t => t.Tasktype)
                .Where(t => t.StaffId == staffId)
                .OrderByDescending(t => t.AssignTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Staff>> GetManagersAsync()
        {
            return await _context.Staff
                .Where(s => s.RoleId == 1)
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Staff>> GetAllStaffAsync()
        {
            return await _context.Staff.OrderBy(s => s.FullName).ToListAsync();
        }

        public async Task<IEnumerable<TaskType>> GetTaskTypesAsync()
        {
            return await _context.TaskTypes.OrderBy(t => t.TaskName).ToListAsync();
        }
    }
}
