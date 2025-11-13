using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using CafeManagent.Hubs;
using CafeManagent.Models;
using DbTask = CafeManagent.Models.Task;
using CafeManagent.Ulties;
using CafeManagent.dto.response;
using CafeManagent.Enums;

public class WorkScheduleBackgroundWorker : BackgroundService // Dịch vụ chạy nền
{
    private readonly ILogger<WorkScheduleBackgroundWorker> _logger;           // Ghi log để debug
    private readonly IServiceProvider _serviceProvider;                       // Tạo scope riêng khi cần truy cập DB hay Hub
    private readonly HashSet<string> _notifiedSchedules = new();              // Dùng key gồm ScheduleId + loại thông báo

    public WorkScheduleBackgroundWorker(ILogger<WorkScheduleBackgroundWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WorkScheduleBackgroundWorker started at {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope(); // tạo scope riêng
                var db = scope.ServiceProvider.GetRequiredService<CafeManagementContext>();
                var notifyUlti = scope.ServiceProvider.GetRequiredService<NotifyUlti>();

                var today = DateOnly.FromDateTime(DateTime.Now);
                var now = TimeOnly.FromDateTime(DateTime.Now);

    
                var schedules = await db.WorkSchedules
                    .Include(ws => ws.Staff)
                    .Include(ws => ws.Workshift)
                    .Where(ws => ws.Date == today || ws.Date == today.AddDays(1))
                    .ToListAsync(stoppingToken);

           
                var groupedSchedules = schedules
                    .Where(ws => ws.Workshift?.StartTime != null)
                    .GroupBy(ws => new { ws.Workshift.ShiftName, ws.Workshift.StartTime, ws.Date });

                foreach (var group in groupedSchedules)
                {
                    var wsSample = group.First(); 
                    var start = wsSample.Workshift.StartTime.Value;
                    double diff = (start.ToTimeSpan() - now.ToTimeSpan()).TotalMinutes;
                    string keyPrefix = $"{group.Key.ShiftName}_{group.Key.Date:yyyyMMdd}";

                    // 1️⃣ Thông báo trước 1 ngày (sau 19h)
                    if (group.Key.Date == today.AddDays(1) && DateTime.Now.Hour >= 19)
                    {
                        string key = $"{keyPrefix}_before1day";
                        if (!_notifiedSchedules.Contains(key))
                        {
                            notifyUlti.AddStaff(new Notify
                            {
                                Message = NotifyMessage.THONG_BAO_CA_LAM_TRC_1_DAY.Message,
                                Time = DateTime.Now
                            });
                            _logger.LogInformation("Add (1-day) notify for ca {ShiftName}", group.Key.ShiftName);
                            _logger.LogInformation("Notify key: {Key}", key);
                            _notifiedSchedules.Add(key);
                        }
                    }
                    // 2️⃣ Thông báo 30 phút trước
                    else if (diff <= 30 && diff > 5)
                    {
                        string key = $"{keyPrefix}_soon";
                        if (!_notifiedSchedules.Contains(key))
                        {
                            notifyUlti.AddStaff(new Notify
                            {
                                Message = NotifyMessage.THONG_BAO_CA_LAM_TRC_30_MIN.Message,
                                Time = DateTime.Now
                            });
                            _logger.LogInformation("Add (soon) notify for ca {ShiftName}", group.Key.ShiftName);
                            _logger.LogInformation("Notify key: {Key}", key);
                            _notifiedSchedules.Add(key);
                        }
                    }
                    // 3️⃣ Thông báo khi ca bắt đầu
                    else if (diff == 0)
                    {
                        string key = $"{keyPrefix}_start";
                        if (!_notifiedSchedules.Contains(key))
                        {
                            notifyUlti.AddStaff(new Notify
                            {
                                Message = NotifyMessage.THONG_BAO_CA_LAM.Message,
                                Time = DateTime.Now
                            });
                            _logger.LogInformation("Add (start) notify for ca {ShiftName}", group.Key.ShiftName);
                            _logger.LogInformation("Notify key: {Key}", key);
                            _notifiedSchedules.Add(key);
                        }
                    }
                }
                if (DateTime.Now.Hour == 0 && DateTime.Now.Minute < 2)
                {
                    _notifiedSchedules.Clear();
                    notifyUlti.ClearStaff();
                    notifyUlti.ClearManager();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WorkScheduleBackgroundWorker");
            }
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}