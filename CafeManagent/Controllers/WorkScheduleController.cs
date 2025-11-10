using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
{
    public class WorkScheduleController : Controller
    {
        private IWorkScheduleService scheduleService;
        public WorkScheduleController(IWorkScheduleService scheduleService)
        {
            this.scheduleService = scheduleService;
        }
        public IActionResult WorkScheduleToday()
        {
            var schedule = scheduleService.GetWorkSchedulesToday();
            return View(schedule);
        }
    }
}
