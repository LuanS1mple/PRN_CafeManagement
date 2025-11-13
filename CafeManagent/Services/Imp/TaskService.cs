using CafeManagent.Models;
using CafeManagent.Hubs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Services
{
    public class TaskService : ITaskService
    {
        private readonly CafeManagementContext _context;
        private readonly IHubContext<TaskHub> _taskHub;

        public TaskService(CafeManagementContext context, IHubContext<TaskHub> taskhub)
        {
            _context = context;
            _taskHub = taskhub;
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

            if (!string.IsNullOrEmpty(statusFilter) && int.TryParse(statusFilter, out int statusValue))
            {
                query = query.Where(t => t.Status == statusValue);
            }

            if (DateTime.TryParse(startDate, out DateTime sDate))
            {
                query = query.Where(t => t.AssignTime.HasValue && t.AssignTime.Value.Date >= sDate.Date);
            }

            if (DateTime.TryParse(endDate, out DateTime eDate))
            {
                query = query.Where(t => t.AssignTime.HasValue && t.AssignTime.Value.Date <= eDate.Date);
            }

            return await query.OrderByDescending(t => t.AssignTime).ToListAsync();
        }

        public async Task<bool> UpdateTaskAsync(Models.Task updateTask)
        {
            var taskToUpdate = await _context.Tasks.FindAsync(updateTask.TaskId);

            if (taskToUpdate == null)
            {
                return false;
            }

            taskToUpdate.TasktypeId = updateTask.TasktypeId;
            taskToUpdate.ManagerId = updateTask.ManagerId;
            taskToUpdate.DueTime = updateTask.DueTime;
            taskToUpdate.StaffId = (updateTask.StaffId == 0) ? null : updateTask.StaffId;

            taskToUpdate.Status = updateTask.Status;

            await _context.SaveChangesAsync();

            _context.Entry(taskToUpdate).State = EntityState.Detached;

            var updatedTask = await _context.Tasks
                .Include(t => t.Manager)
                .Include(t => t.Staff)
                .Include(t => t.Tasktype)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskId == taskToUpdate.TaskId);

            if (updatedTask == null)
            {
                return true;
            }

            var taskPayload = new
            {
                TaskId = updatedTask.TaskId,
                TasktypeId = updatedTask.TasktypeId,
                TasktypeName = updatedTask.Tasktype?.TaskName,
                Description = updatedTask.Tasktype?.Description, 
                StaffId = updatedTask.StaffId,
                StaffName = updatedTask.Staff?.FullName,
                ManagerId = updatedTask.ManagerId,
                ManagerName = updatedTask.Manager?.FullName,
                AssignTime = updatedTask.AssignTime,
                DueTime = updatedTask.DueTime,
                Status = updatedTask.Status
            };

            await _taskHub.Clients.All.SendAsync("TaskUpdated", taskPayload);

            return true;
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

        public async Task CreateTasksAsync(Models.Task task)
        {

            task.AssignTime = DateTime.Now;
            task.Status = 4;

            if (task.StaffId == 0)
            {
                task.StaffId = null;
            }

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var createdTask = await _context.Tasks.Include(t => t.Manager)
                                                  .Include(t => t.Staff)
                                                  .Include(t => t.Tasktype)
                                                  .FirstOrDefaultAsync(t => t.TaskId == task.TaskId);

            var taskPayload = new
            {
                TaskId = createdTask.TaskId,
                TasktypeId = createdTask.TasktypeId,
                TasktypeName = createdTask.Tasktype?.TaskName,
                StaffId = createdTask.StaffId,      
                StaffName = createdTask.Staff?.FullName,
                ManagerId = createdTask.ManagerId,   
                ManagerName = createdTask.Manager?.FullName,
                AssignTime = createdTask.AssignTime,
                DueTime = createdTask.DueTime,
                Status = createdTask.Status
            };

            await _taskHub.Clients.All.SendAsync("TaskCreated", taskPayload);
        }
    }
}