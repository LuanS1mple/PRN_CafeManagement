using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CafeManagent.Hubs;
using CafeManagent.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace CafeManagent.BackgroundWorkers
{
    public class TaskStatusBackgroundWorker : BackgroundService
    {
        private readonly ILogger<TaskStatusBackgroundWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public TaskStatusBackgroundWorker(ILogger<TaskStatusBackgroundWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Task Status Worker running.");

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Task Status Worker is stopping.");
                    return;
                }

                await DoWorkAsync();
            }
        }

        private async Task DoWorkAsync()
        {
            try
            {
                _logger.LogInformation("Task Status Worker checking for updates...");
                var now = DateTime.Now;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<CafeManagementContext>();
                    var taskHub = scope.ServiceProvider.GetRequiredService<IHubContext<TaskHub>>();

                    var tasksToStart = await context.Tasks
                        .Where(t => t.Status == 0 && t.AssignTime.HasValue && t.AssignTime.Value <= now)
                        .ToListAsync();

                    foreach (var task in tasksToStart)
                        task.Status = 1;

                    var tasksToExpire = await context.Tasks
                        .Where(t => t.Status == 1 && t.DueTime.HasValue && t.DueTime.Value < now)
                        .ToListAsync();

                    foreach (var task in tasksToExpire)
                        task.Status = 4;

                    int changes = await context.SaveChangesAsync();

                    if (changes > 0)
                    {
                        _logger.LogInformation($"Task Status Worker updated {changes} tasks.");
                        var updatedTasks = tasksToStart.Concat(tasksToExpire).ToList();

                        foreach (var task in updatedTasks)
                        {
                            var taskPayload = await context.Tasks
                                .AsNoTracking()
                                .Include(t => t.Manager)
                                .Include(t => t.Staff)
                                .Include(t => t.Tasktype)
                                .Select(t => new
                                {
                                    t.TaskId,
                                    t.TasktypeId,
                                    TasktypeName = t.Tasktype.TaskName,
                                    t.Tasktype.Description,
                                    t.StaffId,
                                    StaffName = t.Staff.FullName,
                                    t.ManagerId,
                                    ManagerName = t.Manager.FullName,
                                    t.AssignTime,
                                    t.DueTime,
                                    t.Status
                                })
                                .FirstOrDefaultAsync(t => t.TaskId == task.TaskId);

                            if (taskPayload != null)
                                await taskHub.Clients.All.SendAsync("TaskUpdated", taskPayload);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Task Status Worker.");
            }
        }
    }
}
