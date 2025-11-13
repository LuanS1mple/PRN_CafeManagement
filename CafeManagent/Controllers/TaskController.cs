using Microsoft.AspNetCore.Mvc;
using CafeManagent.Models;
using System.Threading.Tasks;
using CafeManagent.Services;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CafeManagent.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public async Task<IActionResult> Index(
            string searchString,
            string statusFilter,
            string startDate,
            string endDate)
        {
            var tasks = await _taskService.GetTasksAsync(searchString, statusFilter, startDate, endDate);

            ViewBag.Managers = await _taskService.GetManagersAsync();
            ViewBag.Staffs = await _taskService.GetAllStaffAsync();
            ViewBag.TaskTypes = await _taskService.GetTaskTypesAsync();

            return View(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTask(CafeManagent.Models.Task updateTask)
        {
            try
            {
                var success = await _taskService.UpdateTaskAsync(updateTask);
                if (success)
                    TempData["SuccessMessage"] = "Cập nhật công việc thành công!";
                else
                    TempData["ErrorMessage"] = "Không tìm thấy công việc để cập nhật.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Create(CafeManagent.Models.Task task)
        {
            if (!ModelState.IsValid)
            {
                TempData["SelectedTaskTypeId"] = task.TasktypeId;
                TempData["SelectedStaffId"] = task.StaffId;
                TempData["DueTimeValue"] = task.DueTime?.ToString("yyyy-MM-ddTHH:mm");

                TempData["OpenModal"] = true;

                return RedirectToAction("Index");
            }

            await _taskService.CreateTasksAsync(task);

            return RedirectToAction("Index");
        }

        public IActionResult TaskReport()
        {
            return View("TaskReport");
        }
    }
}