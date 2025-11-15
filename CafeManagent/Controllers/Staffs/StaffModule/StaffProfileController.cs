using CafeManagent.dto.request.StaffModuleDTO;
using CafeManagent.dto.response;
using CafeManagent.Models;
using CafeManagent.Services.Interface.StaffModule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; 
using System.Linq;

namespace CafeManagent.Controllers.Staffs.StaffModule
{
    public class StaffProfileController : Controller
    {
        private const string SessionStaffIdKey = "StaffId";

        // Lấy StaffId hiện tại từ Session
        private int? GetCurrentStaffId()
        {
            return HttpContext.Session.GetInt32(SessionStaffIdKey);
        }

        private IActionResult? EnsureOwnerOrRedirect(int requestedId)
        {
            var currentId = GetCurrentStaffId();

            // chưa đăng nhập
            if (currentId is null)
            {
                return Redirect("/home/Login");
            }

            // đăng nhập nhưng không phải chủ nhân của profile
            if (currentId.Value != requestedId)
            {
                return Redirect($"/staff/profile/{currentId.Value}");
            }

            return null; // OK
        }

        // =========================
        // PROFILE
        // =========================
        [HttpGet("/staff/profile/{id:int}")]
        public async Task<IActionResult> Profile(
            int id,
            [FromServices] IStaffProfileService staffProfileService)
        {
            var auth = EnsureOwnerOrRedirect(id);
            if (auth != null) return auth;

            var dto = await staffProfileService.GetByIdAsync(id);
            if (dto is null) return NotFound();

            return View(dto); // View model là StaffProfile (dto.response)
        }

        // =========================
        // EDIT PROFILE - GET
        // =========================
        [HttpGet("/staff/edit/{id:int}")]
        public async Task<IActionResult> Edit(
            int id,
            [FromServices] IStaffProfileService staffProfileService)
        {
            var auth = EnsureOwnerOrRedirect(id);
            if (auth != null) return auth;

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
                // Các field khác (RoleId, Position, ContractEndDate, ...) không dùng ở màn này
            };

            ViewBag.Avatar = dto.Img ?? "/images/avatars/default.png";
            ViewBag.RoleName = dto.RoleName ?? "—";
            ViewBag.ActiveTab = "info"; // tab mặc định

            return View(vm); // View model là UpdateStaffProfile
        }

        // =========================
        // EDIT PROFILE - POST
        // =========================
        [HttpPost("/staff/edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            UpdateStaffProfile input,
            IFormFile? avatarFile,
            [FromServices] IStaffProfileService staffService,
            [FromServices] IWebHostEnvironment env)
        {
            var auth = EnsureOwnerOrRedirect(id);
            if (auth != null) return auth;

            if (id != input.StaffId) return BadRequest();

            // Các trường này dùng cho màn Edit Staff (manager) nên loại khỏi validate tại đây
            ModelState.Remove(nameof(input.Position));
            ModelState.Remove(nameof(input.ContractEndDate));
            ModelState.Remove(nameof(input.RoleId));

            // Validate bằng DataAnnotations
            if (!ModelState.IsValid)
            {
                var dto = await staffService.GetByIdAsync(id);
                ViewBag.Avatar = dto?.Img ?? "/images/avatars/default.png";
                ViewBag.RoleName = dto?.RoleName ?? "—";
                ViewBag.ActiveTab = "info";

                return View(input);
            }

            try
            {
                var ok = await staffService.UpdateAsync(input, avatarFile, env.WebRootPath);
                if (!ok) return NotFound();
            }
            catch (ValidationException ve)
            {
                var members = ve.ValidationResult?.MemberNames?.ToList() ?? new List<string>();
                var message = ve.ValidationResult?.ErrorMessage ?? ve.Message;

                if (members.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, message);
                }
                else
                {
                    foreach (var m in members)
                    {
                        ModelState.AddModelError(m, message);
                    }
                }

                var dto = await staffService.GetByIdAsync(id);
                ViewBag.Avatar = dto?.Img ?? "/images/avatars/default.png";
                ViewBag.RoleName = dto?.RoleName ?? "—";
                ViewBag.ActiveTab = "info";

                return View(input);
            }
            catch (InvalidOperationException ex)
            {
                // lỗi avatar (size, extension...)
                ModelState.AddModelError(string.Empty, ex.Message);

                var dto = await staffService.GetByIdAsync(id);
                ViewBag.Avatar = dto?.Img ?? "/images/avatars/default.png";
                ViewBag.RoleName = dto?.RoleName ?? "—";
                ViewBag.ActiveTab = "info";

                return View(input);
            }

            return RedirectToAction(nameof(Profile), new { id = input.StaffId });
        }

        // =========================
        // CHANGE PASSWORD - POST
        // =========================
        [HttpPost("/staff/change-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordRequest input,
            [FromServices] IStaffProfileService staffService,
            [FromServices] IStaffProfileService staffProfileServiceForAvatar)
        {
            var auth = EnsureOwnerOrRedirect(input.StaffId);
            if (auth != null) return auth;

            // validate model (mật khẩu không khớp, minlength, v.v.)
            if (!ModelState.IsValid)
            {
                var dto = await staffProfileServiceForAvatar.GetByIdAsync(input.StaffId);
                if (dto is null) return NotFound();

                var vm = new UpdateStaffProfile
                {
                    StaffId = dto.StaffId,
                    FullName = dto.FullName,
                    Gender = dto.Gender,
                    BirthDate = dto.BirthDate,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    Email = dto.Email
                };

                ViewBag.Avatar = dto.Img ?? "/images/avatars/default.png";
                ViewBag.RoleName = dto.RoleName ?? "—";
                ViewBag.ActiveTab = "security"; // mở tab Bảo mật

                return View("Edit", vm);
            }

            try
            {
                await staffService.ChangePasswordAsync(input);
                TempData["Success"] = "Đổi mật khẩu thành công.";
                return RedirectToAction(nameof(Edit), new { id = input.StaffId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                var dto = await staffProfileServiceForAvatar.GetByIdAsync(input.StaffId);
                if (dto is null) return NotFound();

                var vm = new UpdateStaffProfile
                {
                    StaffId = dto.StaffId,
                    FullName = dto.FullName,
                    Gender = dto.Gender,
                    BirthDate = dto.BirthDate,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    Email = dto.Email
                };

                ViewBag.Avatar = dto.Img ?? "/images/avatars/default.png";
                ViewBag.RoleName = dto.RoleName ?? "—";
                ViewBag.ActiveTab = "security";

                return View("Edit", vm);
            }
        }
    }
}
