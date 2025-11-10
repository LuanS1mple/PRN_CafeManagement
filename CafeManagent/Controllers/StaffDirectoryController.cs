using CafeManagent.dto.request;
using CafeManagent.dto.response;
using CafeManagent.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
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
        public async Task<IActionResult> Create(
            CreateStaffRequest req,
            [FromServices] IWebHostEnvironment env,
            CancellationToken ct)
        {
            // refill roles if redisplaying
            ViewBag.Roles = await _service.GetRolesSelectListAsync(ct);

            if (!ModelState.IsValid)
            {
                return View(req);
            }

            try
            {
                // Note: req.AvatarFile should be bound from <input type="file" name="AvatarFile" />
                var newId = await _service.CreateAsync(req, env.WebRootPath, ct);
                return RedirectToAction(nameof(Details), new { id = newId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(req);
            }
        }

        // ---------------- EDIT ----------------
        // GET /staffs/edit/123
        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var detail = await _service.GetDetailAsync(id, ct);
            if (detail is null) return NotFound();

            // Map StaffDetailDto -> UpdateStaffProfile (adjust fields as your DTO defines)
            var vm = new UpdateStaffProfile
            {
                StaffId = detail.StaffId,
                FullName = detail.FullName,
                Email = detail.Email,
                BirthDate = detail.BirthDate,
                Phone = detail.Phone,
                Address = detail.Address,
                //UserName = detail.UserName ?? "", // if your DTO has UserName
                //CreateAt = detail.CreateAt ?? DateTime.Now, // if detail contains CreateAt
                RoleId = null // optional: set if you include role in detail mapping
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
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var existing = await _service.GetDetailAsync(id, ct);
                ViewBag.Avatar = existing?.AvatarUrl ?? "/images/avatars/default.png";
                return View(input);
            }
        }
    }
}
