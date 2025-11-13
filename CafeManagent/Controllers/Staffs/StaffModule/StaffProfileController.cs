using CafeManagent.dto.request.StaffModuleDTO;
using CafeManagent.dto.response;
using CafeManagent.Models;
using CafeManagent.Services.Interface.StaffModule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Controllers.Staffs.StaffModule
{
    public class StaffProfileController : Controller
    {

        [HttpGet("/staff/profile/{id:int}")]
        public async Task<IActionResult> Profile(
            int id,
            [FromServices] IStaffProfileService staffProfileService)
        {
            var dto = await staffProfileService.GetByIdAsync(id);
            if (dto is null) return NotFound();

            return View(dto);
        }

        // GET /staff/edit/1
        [HttpGet("/staff/edit/{id:int}")]
        public async Task<IActionResult> Edit(
            int id,
            [FromServices] IStaffProfileService staffProfileService)
        {
            var dto = await staffProfileService.GetByIdAsync(id);
            if (dto is null) return NotFound();

            var vm = new UpdateStaffProfile
            {
                StaffId = dto.StaffId,
                FullName = dto.FullName,
                Gender = dto.Gender,
                BirthDate = dto.BirthDate,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
            };

            ViewBag.Avatar = dto.Img ?? "/images/avatars/default.png";
            ViewBag.RoleName = dto.RoleName ?? "—";
            return View(vm);
        }

        [HttpPost("/staff/edit/{id:int}")]
        public async Task<IActionResult> Edit(
            int id,
            UpdateStaffProfile input,
            IFormFile? avatarFile,
            [FromServices] IStaffProfileService staffService,
            [FromServices] IWebHostEnvironment env)
        {
            if (id != input.StaffId) return BadRequest();

            try
            {
                var ok = await staffService.UpdateAsync(input, avatarFile, env.WebRootPath);
                if (!ok) return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Avatar = "/images/avatars/default.png"; // fallback
                return View(input);
            }

            return RedirectToAction(nameof(Profile), new { id = input.StaffId });
        }

    }
}

