using CafeManagent.dto.request.StaffModuleDTO;
using CafeManagent.dto.response.StaffModuleDTO;
using CafeManagent.Services.Interface.StaffModule;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CafeManagent.Controllers.Staffs.StaffModule
{
    [Route("staffs")]
    public class StaffDirectoryController : Controller
    {
        private readonly IStaffDirectoryService _service;

        public StaffDirectoryController(IStaffDirectoryService service)
        {
            _service = service;
        }

        // GET /staffs
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery(Name = "q")] StaffListQuery q, CancellationToken ct)
        {
            Console.WriteLine($"[QUERY] {Request.QueryString}");
            Console.WriteLine($"[BOUND] Q='{q?.Q}', Status={q?.Status}, Page={q?.Page}, Size={q?.Size}");
            q ??= new StaffListQuery(); // dự phòng
            var data = await _service.GetPagedAsync(q, ct);
            ViewBag.Query = q;
            return View(data);
        }

        // GET /staffs/123
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var dto = await _service.GetDetailAsync(id, ct);
            if (dto is null) return NotFound();
            return View(dto);
        }

        // ---------------- CREATE ----------------
        // GET /staffs/create
        [HttpGet("create")]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            ViewBag.Roles = await _service.GetRolesSelectListAsync(ct);
            return View(new CreateStaffRequest());
        }

        // POST /staffs/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStaffRequest req,
            [FromServices] IWebHostEnvironment env, CancellationToken ct)
        {
            // load lại dropdown khi trả view
            ViewBag.Roles = await _service.GetRolesSelectListAsync(ct);

            // DataAnnotations chưa qua => trả về kèm lỗi
            if (!ModelState.IsValid) return View(req);

            try
            {
                var newId = await _service.CreateAsync(req, env.WebRootPath, ct);
                return RedirectToAction(nameof(Details), new { id = newId });
            }
            catch (ValidationException ve)
            {
                // Ưu tiên map vào đúng field nếu có MemberNames
                var members = ve.ValidationResult?.MemberNames?.ToList() ?? new List<string>();
                if (members.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, ve.ValidationResult?.ErrorMessage ?? ve.Message);
                }
                else
                {
                    foreach (var m in members)
                        ModelState.AddModelError(m, ve.ValidationResult?.ErrorMessage ?? ve.Message);
                }
                return View(req);
            }
            catch (InvalidOperationException ex)
            {
                // fallback lỗi chung (nếu service còn ném kiểu này)
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(req);
            }
        }

        // ---------------- EDIT ----------------
        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var detail = await _service.GetDetailAsync(id, ct);
            if (detail is null) return NotFound();

            var vm = new UpdateStaffProfile
            {
                StaffId = detail.StaffId,
                FullName = detail.FullName,
                Email = detail.Email,
                BirthDate = detail.BirthDate,
                Phone = detail.Phone,
                Address = detail.Address,
                RoleId = detail.RoleId, // nếu bạn muốn set mặc định từ detail.RoleId hãy sửa chỗ này
                Gender = detail.Gender,
                Position = detail.Title, // map Contract.Position -> Title trong DTO detail
                ContractEndDate = detail.ContractEndDate
            };

            ViewBag.Avatar = detail.AvatarUrl ?? "/images/avatars/default.png";
            ViewBag.Roles = await _service.GetRolesSelectListAsync(ct);
            return View(vm);
        }

        // POST /staffs/edit/123
        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            UpdateStaffProfile input,
            IFormFile? avatarFile,
            [FromServices] IWebHostEnvironment env,
            CancellationToken ct)
        {
            if (id != input.StaffId) return BadRequest();

            ViewBag.Roles = await _service.GetRolesSelectListAsync(ct);

            if (!ModelState.IsValid)
            {
                var existing = await _service.GetDetailAsync(id, ct);
                ViewBag.Avatar = existing?.AvatarUrl ?? "/images/avatars/default.png";
                return View(input);
            }

            try
            {
                var ok = await _service.UpdateAsync(input, avatarFile, env.WebRootPath, ct);
                if (!ok) return NotFound();

                return RedirectToAction(nameof(Details), new { id = input.StaffId });
            }
            catch (ValidationException ve)
            {
                var members = ve.ValidationResult?.MemberNames?.ToList() ?? new List<string>();
                if (members.Count == 0)
                    ModelState.AddModelError(string.Empty, ve.ValidationResult?.ErrorMessage ?? ve.Message);
                else
                    foreach (var m in members)
                        ModelState.AddModelError(m, ve.ValidationResult?.ErrorMessage ?? ve.Message);

                var existing = await _service.GetDetailAsync(id, ct);
                ViewBag.Avatar = existing?.AvatarUrl ?? "/images/avatars/default.png";
                return View(input);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var existing = await _service.GetDetailAsync(id, ct);
                ViewBag.Avatar = existing?.AvatarUrl ?? "/images/avatars/default.png";
                return View(input);
            }
        }

        [HttpPost("status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus([FromForm] int id, [FromForm] int status, CancellationToken ct)
        {
            var (ok, sVal, name, badgeClass) = await _service.UpdateStatusAsync(id, status, ct);
            if (!ok) return NotFound(new { ok = false, message = "Không tìm thấy nhân viên." });

            // Trả JSON cho client gọi; các client khác sẽ nhận qua SignalR
            return Json(new { ok = true, status = sVal, name, badgeClass });
        }

    }
}
