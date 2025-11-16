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

        //#region Task Management

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
                return View(task);
            }

            await _taskService.CreateTasksAsync(task, managerId.Value.ToString());

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

            await _taskService.UpdateTaskAsync(updateTask);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, int newStatus)
        {
            var (staffId, errorResult) = GetCurrentUserId();
            if (!staffId.HasValue) return errorResult;

            await _taskService.UpdateTaskStatusAsync(taskId, newStatus);

            return RedirectToAction("Detail");
        }

        [Authorize(Roles = "Cashier, Barista")]
        public async Task<IActionResult> Detail()
        {
            var (staffId, errorResult) = GetCurrentUserId();
            if (!staffId.HasValue) return errorResult;

            var tasks = await _taskService.GetTasksByStaffIdAsync(staffId.Value);
            ViewData["Title"] = "Công việc của tôi";
            return View("Detail", tasks);
        }

        [HttpGet("TaskReport")]
        [Authorize(Roles = "Branch Manager")]
        public async Task<IActionResult> TaskReport()
        {
            // Lấy role từ session
            var role = HttpContext.Session.GetString("StaffRole");
            ViewBag.StaffRole = role;

            var summaryDictionary = await _taskReportService.GetTaskSummaryAsync();
            ViewBag.TaskSummary = summaryDictionary;

            return View();
        }

        [HttpGet("GetTasksByStatus")]
        public async Task<IActionResult> GetTasksByStatus(int statusId)
        {
            var tasks = await _taskReportService.GetTasksByStatusAsync(statusId);
            return Json(tasks);
        }

        [HttpGet("ExportTaskReport")]
        [Authorize(Roles = "Branch Manager")]
        public async Task<IActionResult> ExportTaskReport()
        {
            try
            {
                var fileContent = await _taskReportService.GenerateExcelReportAsync();
                var fileName = $"TaskReport-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(fileContent,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName);
            }
            catch
            {
                TempData["Error"] = "Không thể xuất file Excel lúc này.";
                return RedirectToAction("TaskReport");
            }
        }
    }
}

