using CafeManagent.Hubs;
using CafeManagent.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;
using CafeManagent.Services.Interface.TaskModule;

namespace CafeManagent.Services.Imp.TaskModule
{
    public class TaskService : ITaskService
    {
        private readonly CafeManagementContext _context;
        private readonly IHubContext<TaskHub> _taskHub;
        private readonly IHubContext<NotifyHub> _notifyHub;

        public TaskService(CafeManagementContext context,
                           IHubContext<TaskHub> taskHub,
                           IHubContext<NotifyHub> notifyHub)
        {
            _context = context;
            _taskHub = taskHub;
            _notifyHub = notifyHub;
        }

        private async Task SendTaskUpdateAsync(Models.Task task)
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

            // Gửi cập nhật Task realtime cho tất cả client
            await _taskHub.Clients.All.SendAsync("ReceiveTaskUpdate", taskDto);

            // Gửi thông báo cho Staff nếu có
            if (taskFull.StaffId.HasValue)
            {
                var notify = new
                {
                    IsSuccess = true,
                    Message = $"Công việc [{taskFull.Tasktype?.TaskName}] đã được cập nhật trạng thái."
                };
                await _notifyHub.Clients.User(taskFull.StaffId.Value.ToString())
                    .SendAsync("ReceiveNotify", notify);
            }
        }

        public async Task<IEnumerable<Models.Task>> GetTasksAsync(string searchString, string statusFilter, string startDate, string endDate)
        {
            var query = _context.Tasks
                .Include(t => t.Staff)
                .Include(t => t.Manager)
                .Include(t => t.Tasktype)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(t =>
                    (t.Staff != null && t.Staff.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Manager != null && t.Manager.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Tasktype != null && t.Tasktype.TaskName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                );

            if (!string.IsNullOrEmpty(statusFilter) && int.TryParse(statusFilter, out int statusValue))
                query = query.Where(t => t.Status == statusValue);

            if (DateTime.TryParse(startDate, out DateTime sDate))
                query = query.Where(t => t.AssignTime.HasValue && t.AssignTime.Value.Date >= sDate.Date);

            if (DateTime.TryParse(endDate, out DateTime eDate))
                query = query.Where(t => t.AssignTime.HasValue && t.AssignTime.Value.Date <= eDate.Date);

            return await query.OrderByDescending(t => t.AssignTime).ToListAsync();
        }

        public async Task<bool> UpdateTaskAsync(Models.Task updateTask)
        {
            var taskToUpdate = await _context.Tasks.FindAsync(updateTask.TaskId);
            if (taskToUpdate == null) return false;

            taskToUpdate.TasktypeId = updateTask.TasktypeId;
            taskToUpdate.ManagerId = updateTask.ManagerId;
            taskToUpdate.DueTime = updateTask.DueTime;
            taskToUpdate.StaffId = updateTask.StaffId == 0 ? null : updateTask.StaffId;
            taskToUpdate.AssignTime = updateTask.AssignTime;
            taskToUpdate.Status = updateTask.Status;

            await _context.SaveChangesAsync();
            _context.Entry(taskToUpdate).State = EntityState.Detached;

            await SendTaskUpdateAsync(taskToUpdate);
            return true;
        }

        public async Task CreateTasksAsync(Models.Task task, string currentManagerId)
        {
            if (!int.TryParse(currentManagerId, out int managerId))
                throw new ArgumentException("Không thể phân tích ManagerId từ claim của người dùng.", nameof(currentManagerId));

            task.ManagerId = managerId;
            task.AssignTime = DateTime.Now;
            task.Status = 0;
            task.StaffId = task.StaffId == 0 ? null : task.StaffId;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            await SendTaskUpdateAsync(task);
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, int newStatus, int staffId)
        {
            var taskToUpdate = await _context.Tasks.FindAsync(taskId);
            if (taskToUpdate == null || taskToUpdate.StaffId != staffId) return false;

            taskToUpdate.Status = newStatus;
            await _context.SaveChangesAsync();
            _context.Entry(taskToUpdate).State = EntityState.Detached;

            await SendTaskUpdateAsync(taskToUpdate);
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
            var query = _context.Tasks
                .Include(t => t.Staff)
                .Include(t => t.Manager)
                .Include(t => t.Tasktype)
                .Where(t => t.StaffId == staffId);

            return await query.OrderByDescending(t => t.AssignTime).ToListAsync();
        }

        public async Task<IEnumerable<Staff>> GetManagersAsync()
        {
            return await _context.Staff.Where(s => s.RoleId == 1).OrderBy(s => s.FullName).ToListAsync();
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
