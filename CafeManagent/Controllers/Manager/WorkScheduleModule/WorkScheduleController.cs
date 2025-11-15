using CafeManagent.Services.Interface.WorkScheduleModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers.Manager.WorkScheduleModule
{
    public class WorkScheduleController : Controller
    {
        private IWorkScheduleService scheduleService;
        public WorkScheduleController(IWorkScheduleService scheduleService)
        {
            this.scheduleService = scheduleService;
        }
        [Authorize(Roles = "Branch Manager")]
        public IActionResult WorkScheduleToday()
        {
            var schedule = scheduleService.GetWorkSchedulesToday();
            return View(schedule);
        }
    }
}
