using CafeManagent.Models;
using CafeManagent.Services.Interface.StaffWorkScheduleModule;
using CafeManagent.Services.Interface.WorkShiftModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers.Staffs.StaffWorkScheduleModule
{
    public class StaffWorkScheduleController : Controller
    {
        private readonly IStaffWorkScheduleService _service;
        private readonly CafeManagementContext _context;

        public StaffWorkScheduleController(IStaffWorkScheduleService service, CafeManagementContext context)
        {
            _service = service;
            _context = context;
        }
        [Authorize(Roles = "Cashier , Barista")]
        [HttpGet("StaffWorkSchedule")]
        public async Task<IActionResult> Index()
        {
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            var staffSchedule = await _service.GetWorkShiftsByStaffAsync(staffId);
            ViewBag.StaffId = staffId;
            return View(staffSchedule);
        }

    }
}
