using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CafeManagent.Services.Interface.TaskModule;

public class TaskReportController : Controller
{
    private readonly ITaskReportService _taskReportService;

    public TaskReportController(ITaskReportService taskReportService)
    {
        _taskReportService = taskReportService;
    }

    public async Task<IActionResult> TaskReport()
    {
        var summaryDictionary = await _taskReportService.GetTaskSummaryAsync();

        ViewBag.TaskSummary = summaryDictionary;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetTasksByStatus(int statusId)
    {
        var tasks = await _taskReportService.GetTasksByStatusAsync(statusId);

        return Json(tasks);
    }
}