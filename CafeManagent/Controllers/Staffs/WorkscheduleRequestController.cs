using CafeManagent.dto.response;
using CafeManagent.mapper;
using CafeManagent.Models;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers.Staffs
{
    public class WorkscheduleRequestController : Controller
    {
        private readonly IWorkScheduleService workShiftService;
        public WorkscheduleRequestController(IWorkScheduleService workShiftService)
        {
            this.workShiftService = workShiftService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Init()
        {
            //int staffId = int.Parse(HttpContext.Session.Get("StaffId"));
            int staffId = 1;
            List<WorkScheduleBasicDTO> data = MapperHelper.FromWorkSchedule(workShiftService.Get(staffId));
            ViewBag.WorkSchedules = data;
            return View();
        }
        [HttpGet]
        public IActionResult GetWorkSchedule(int id)
        {
            WorkScheduleDetailDTO schedule = MapperHelper.FromWorkSchedule(workShiftService.GetById(id));
            return Json(schedule);
        }
    }
}
