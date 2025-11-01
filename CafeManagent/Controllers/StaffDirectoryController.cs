using CafeManagent.dto.response;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
{
    public class StaffDirectoryController : Controller
    {
        [HttpGet("/staffs")]
        public async Task<IActionResult> Index([FromQuery] StaffListQuery q,
                [FromServices] IStaffDirectoryService service)
        {
            var data = await service.GetPagedAsync(q);
            ViewBag.Query = q;
            return View(data);
        }

        [HttpGet("/staffs/{id:int}")]
        public async Task<IActionResult> Details(int id, [FromServices] IStaffDirectoryService service)
        {
            var dto = await service.GetDetailAsync(id);
            if (dto is null) return NotFound();
            return View(dto);
        }
    }
}
