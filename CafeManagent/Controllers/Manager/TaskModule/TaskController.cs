using Microsoft.AspNetCore.Mvc;
using CafeManagent.Models;
using System.Threading.Tasks;
using System;
using CafeManagent.Services.Interface.TaskModule;
using System.Security.Claims;
using CafeManagent.Hubs;
using CafeManagent.dto.response.NotifyModuleDTO;
using CafeManagent.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Controllers.Manager.TaskModule
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly ITaskReportService _taskReportService;
        private readonly IHubContext<TaskHub> _taskHub;
        private readonly IHubContext<NotifyHub> _notifyHub;
        private readonly IHubContext<ResponseHub> _responseHub;

        public TaskController(
            ITaskService taskService,
            ITaskReportService taskReportService,
            IHubContext<TaskHub> taskHub,
            IHubContext<NotifyHub> notifyHub,
            IHubContext<ResponseHub> responseHub)
        {
            _taskService = taskService;
            _taskReportService = taskReportService;
            _taskHub = taskHub;
            _notifyHub = notifyHub;
            _responseHub = responseHub;
        }

        private (int? UserId, IActionResult ErrorResult) GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return (null, Unauthorized("Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            if (!int.TryParse(userIdClaim, out int userIdInt))
                return (null, Unauthorized("Không thể xác nhận người dùng - Sai định dạng ID."));
            return (userIdInt, null);
        }

        [Authorize(Roles = "Branch Manager")]
        public async Task<IActionResult> Index(string searchString, string statusFilter, string startDate, string endDate)
        {
            var tasks = await _taskService.GetTasksAsync(searchString, statusFilter, startDate, endDate);
            return View(tasks);
        }

        [HttpGet]
        [Authorize(Roles = "Branch Manager")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Staffs = await _taskService.GetAllStaffAsync();
            ViewBag.TaskTypes = await _taskService.GetTaskTypesAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Branch Manager")]
        public async Task<IActionResult> Create(Models.Task task)
        {
            var (managerId, errorResult) = GetCurrentUserId();
            if (!managerId.HasValue) return errorResult;

            if (!ModelState.IsValid)
            {
                ViewBag.Staffs = await _taskService.GetAllStaffAsync();
                ViewBag.TaskTypes = await _taskService.GetTaskTypesAsync();

                SystemNotify notify = new SystemNotify()
                {
                    IsSuccess = false,
                    Message = NotifyMessage.TASK_CREATED_FAIL.Message
                };

                ResponseHub.SetNotify(managerId.Value, notify);

                return View(task);
            }

            try
            {
                await _taskService.CreateTasksAsync(task, managerId.Value.ToString());

                var createdTask = await _taskService.GetTaskByIdAsync(task.TaskId);
                if (createdTask != null)
                {
                    await _taskHub.Clients.All.SendAsync("ReceiveTaskUpdate", new
                    {
                        createdTask.TaskId,
                        createdTask.TasktypeId,
                        tasktypeName = createdTask.Tasktype?.TaskName,
                        description = createdTask.Tasktype?.Description,
                        createdTask.ManagerId,
                        managerName = createdTask.Manager?.FullName,
                        createdTask.StaffId,
                        staffName = createdTask.Staff?.FullName,
                        assignTime = createdTask.AssignTime,
                        dueTime = createdTask.DueTime,
                        status = createdTask.Status
                    });
                }

                SystemNotify notify = new SystemNotify()
                {
                    IsSuccess = true,
                    Message = NotifyMessage.TASK_CREATED_SUCCESS.Message
                };

                ResponseHub.SetNotify(managerId.Value, notify);

                SystemNotify notify1 = new SystemNotify()
                {
                    IsSuccess = true,
                    Message = NotifyMessage.TASK_CREATED_SUCCESS.Message
                };

                if (task.StaffId.HasValue)
                {
                    ResponseHub.SetNotify(task.StaffId.Value, notify1);
                }
            }
            catch
            {
                SystemNotify notify = new SystemNotify()
                {
                    IsSuccess = false,
                    Message = NotifyMessage.TASK_CREATED_FAIL.Message
                };
                ResponseHub.SetNotify(managerId.Value, notify);
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Branch Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            ViewBag.Managers = await _taskService.GetManagersAsync();
            ViewBag.Staffs = await _taskService.GetAllStaffAsync();
            ViewBag.TaskTypes = await _taskService.GetTaskTypesAsync();
            return View(task);
        }

        [HttpPost]
        [Authorize(Roles = "Branch Manager")]
        public async Task<IActionResult> Edit(Models.Task updateTask)
        {
            var (managerId, errorResult) = GetCurrentUserId();
            if (!managerId.HasValue) return errorResult;

            if (!ModelState.IsValid)
            {
                ViewBag.Managers = await _taskService.GetManagersAsync();
                ViewBag.Staffs = await _taskService.GetAllStaffAsync();
                ViewBag.TaskTypes = await _taskService.GetTaskTypesAsync();
                return View(updateTask);
            }

            try
            {
                var success = await _taskService.UpdateTaskAsync(updateTask);
                if (success)
                {
                    var updatedTask = await _taskService.GetTaskByIdAsync(updateTask.TaskId);

                    // gửi Task realtime
                    await _taskHub.Clients.All.SendAsync("ReceiveTaskUpdate", new
                    {
                        updatedTask.TaskId,
                        updatedTask.TasktypeId,
                        tasktypeName = updatedTask.Tasktype?.TaskName,
                        description = updatedTask.Tasktype?.Description,
                        updatedTask.ManagerId,
                        managerName = updatedTask.Manager?.FullName,
                        updatedTask.StaffId,
                        staffName = updatedTask.Staff?.FullName,
                        assignTime = updatedTask.AssignTime,
                        dueTime = updatedTask.DueTime,
                        status = updatedTask.Status
                    });

                    // thông báo manager
                    SystemNotify notify = new SystemNotify()
                    {
                        IsSuccess = true,
                        Message = NotifyMessage.TASK_UPDATED_SUCCESS.Message
                    };
                    ResponseHub.SetNotify(managerId.Value, notify);

                    // thông báo nhân viên
                    SystemNotify notify1 = new SystemNotify()
                    {
                        IsSuccess = true,
                        Message = NotifyMessage.TASK_UPDATED_SUCCESS.Message
                    };
                    if (updateTask.StaffId.HasValue)
                    {
                        ResponseHub.SetNotify(updateTask.StaffId.Value, notify1);
                    }
                }
                else
                {
                    SystemNotify notify = new SystemNotify()
                    {
                        IsSuccess = false,
                        Message = NotifyMessage.TASK_UPDATED_FAIL.Message
                    };
                    ResponseHub.SetNotify(managerId.Value, notify);
                }
            }
            catch
            {
                SystemNotify notify = new SystemNotify()
                {
                    IsSuccess = false,
                    Message = NotifyMessage.TASK_UPDATED_FAIL.Message
                };
                ResponseHub.SetNotify(managerId.Value, notify);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, int newStatus)
        {
            var (staffId, errorResult) = GetCurrentUserId();
            if (!staffId.HasValue) return errorResult;

            try
            {
                var success = await _taskService.UpdateTaskStatusAsync(taskId, newStatus);
                if (!success)
                {
                    SystemNotify notify1 = new SystemNotify()
                    {
                        IsSuccess = false,
                        Message = NotifyMessage.TASK_ERROR.Message
                    };
                    return RedirectToAction("Detail");
                }

                var task = await _taskService.GetTaskByIdAsync(taskId);

                // gửi Task realtime
                await _taskHub.Clients.All.SendAsync("ReceiveTaskUpdate", new
                {
                    task.TaskId,
                    task.TasktypeId,
                    tasktypeName = task.Tasktype?.TaskName,
                    description = task.Tasktype?.Description,
                    task.ManagerId,
                    managerName = task.Manager?.FullName,
                    task.StaffId,
                    staffName = task.Staff?.FullName,
                    assignTime = task.AssignTime,
                    dueTime = task.DueTime,
                    status = task.Status
                });

                NotifyMessage message = newStatus switch
                {
                    2 => NotifyMessage.TASK_COMPLETED,
                    3 => NotifyMessage.TASK_CANCELLED,
                    _ => NotifyMessage.TASK_UPDATED_SUCCESS
                };

                // thông báo cho staff
                SystemNotify notify = new SystemNotify()
                {
                    IsSuccess = true,
                    Message = message.Message
                };
                ResponseHub.SetNotify(staffId.Value, notify);

                // thông báo cho manager
                if (task.ManagerId.HasValue)
                {
                    SystemNotify notify1 = new SystemNotify()
                    {
                        IsSuccess = true,
                        Message = message.Message
                    };
                    ResponseHub.SetNotify(task.ManagerId.Value, notify1);
                }
            }
            catch
            {
                SystemNotify notify = new SystemNotify()
                {
                    IsSuccess = false,
                    Message = NotifyMessage.TASK_ERROR.Message
                };
                ResponseHub.SetNotify(staffId.Value, notify);
            }

            return RedirectToAction("Detail");
        }

        public async Task<IActionResult> Detail()
        {
            var (staffId, errorResult) = GetCurrentUserId();
            if (!staffId.HasValue) return errorResult;

            var tasks = await _taskService.GetTasksByStaffIdAsync(staffId.Value);
            ViewData["Title"] = "Công việc của tôi";
            return View("Detail", tasks);
        }
    }
}
