using CafeManagent.dto.request.MailWorkShiftDTO;
using System.Net.Mail;
using System.Net;
using CafeManagent.dto.request.WorkShiftModuleDTO;
using CafeManagent.Models;
using CafeManagent.Services.Interface.WorkShiftModule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeManagent.dto.response.NotifyModuleDTO;
using CafeManagent.Hubs;
using CafeManagent.Enums;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.SignalR;
using CafeManagent.dto.response.WorkShiftDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CafeManagent.Controllers.Manager.WorkShiftModule
{
    public class WorkShiftController : Controller
    {
        private readonly IWorkShiftService _service;
        private readonly CafeManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<WorkShiftHub> _hub;

        public WorkShiftController(IWorkShiftService service, CafeManagementContext context, IConfiguration configuration, IHubContext<WorkShiftHub> hub)
        {
            _service = service;
            _context = context;
            _configuration = configuration;
            _hub = hub;
        }

        [Authorize(Roles = "Branch Manager")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 6)
        {
            var (shifts, totalItems) = await _service.GetPagedWorkShiftsAsync(page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.Positions = await _context.Roles
                .Where(r => r.RoleId != 1)
                .Select(r => r.RoleName)
                .Distinct()
                .Where(rn => !string.IsNullOrEmpty(rn))
                .ToListAsync();

            ViewBag.ShiftTypes = await _context.WorkShifts
                .Select(s => s.ShiftName)
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s))
                .ToListAsync();

            ViewBag.Employees = await _context.Staff
                .Where(s => s.RoleId != 1)
                .Select(s => s.FullName)
                .Distinct()
                .Where(n => !string.IsNullOrEmpty(n))
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Now);
            ViewBag.TotalShifts = shifts.Count;
            ViewBag.TotalEmployees = shifts.Select(s => s.Employee).Distinct().Count();
            ViewBag.TodayShifts = shifts.Count(s => s.Date == today);
            ViewBag.TotalHours = shifts.Sum(s => s.TotalHours);

            return View(shifts);
        }

        [Authorize(Roles = "Branch Manager")]
        [HttpPost]
        public async Task<IActionResult> FilterWorkShift([FromForm] FilterWorkShiftDTO filter, int page = 1, int pageSize = 6)
        {
            var (shifts, totalItems) = await _service.FilterPagedWorkShiftsAsync(filter, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.Positions = await _context.Roles
                .Where(r => r.RoleId != 1)
                .Select(r => r.RoleName)
                .Distinct()
                .Where(rn => !string.IsNullOrEmpty(rn))
                .ToListAsync();

            ViewBag.ShiftTypes = await _context.WorkShifts
                .Select(s => s.ShiftName)
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s))
                .ToListAsync();

            ViewBag.Employees = await _context.Staff
                .Where(s => s.RoleId != 1)
                .Select(s => s.FullName)
                .Distinct()
                .Where(n => !string.IsNullOrEmpty(n))
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Now);
            ViewBag.TotalShifts = shifts.Count;
            ViewBag.TotalEmployees = shifts.Select(s => s.Employee).Distinct().Count();
            ViewBag.TodayShifts = shifts.Count(s => s.Date == today);
            ViewBag.TotalHours = shifts.Sum(s => s.TotalHours);

            return View("Index", shifts);
        }

        [Authorize(Roles = "Branch Manager")]
        [HttpPost]
        public async Task<IActionResult> AddWorkShift([FromForm] AddWorkShiftDTO dto)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId") ?? 0;


            if (!ModelState.IsValid)
            {
            
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var errorMessage = string.Join(". ", errorMessages);

                ResponseHub.SetNotify(staffId, new SystemNotify()
                {
                    IsSuccess = false,
                    Message = errorMessage 
                });

                return RedirectToAction("Index");
            }

            bool success = await _service.AddWorkShiftAsync(dto);

            ResponseHub.SetNotify(staffId, new SystemNotify()
            {
                IsSuccess = success,
                Message = success
                    ? NotifyMessage.THEM_CA_LAM_OK.Message
                    : NotifyMessage.THEM_CA_THAT_BAI.Message
            });

            if (success)
            {
                var schedule = await _context.WorkSchedules
                    .Include(ws => ws.Staff)
                    .Include(ws => ws.Workshift)
                    .OrderByDescending(ws => ws.Date)
                    .FirstOrDefaultAsync(); // hoặc lấy entity vừa thêm

                var staff = schedule.Staff;
                var workShift = schedule.Workshift;

                await _hub.Clients.All.SendAsync("ReceiveWorkShiftUpdate", new
                {
                    Date = schedule.Date.HasValue ? schedule.Date.Value.ToString("yyyy-MM-dd") : "",
                    Position = staff.Role?.RoleName ?? "",
                    ShiftType = schedule.ShiftName,
                    StartTime = workShift.StartTime.HasValue ? workShift.StartTime.Value.ToString(@"hh\:mm") : "",
                    EndTime = workShift.EndTime.HasValue ? workShift.EndTime.Value.ToString(@"hh\:mm") : "",
                    Description = schedule.Description ?? ""

                });
            }


            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Branch Manager")]
        [HttpPost]
        public async Task<IActionResult> DeleteWorkShift(int id)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId") ?? 0;

            var success = await _service.DeleteWorkShiftAsync(id);

            ResponseHub.SetNotify(staffId, new SystemNotify()
            {
                IsSuccess = success,
                Message = success
                    ? NotifyMessage.XOA_CA_LAM_OK.Message
                    : NotifyMessage.XOA_CA_THAT_BAI.Message
            });

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Branch Manager")]
        [HttpPost]
        public async Task<IActionResult> UpdateWorkShift([FromForm] UpdateWorkShiftDTO dto)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId") ?? 0;

            if (!ModelState.IsValid)
            {

                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var errorMessage = string.Join(". ", errorMessages);

                ResponseHub.SetNotify(staffId, new SystemNotify()
                {
                    IsSuccess = false,
                    Message = errorMessage
                });

                return RedirectToAction("Index");
            }

            bool success = await _service.UpdateWorkShiftAsync(dto);

            ResponseHub.SetNotify(staffId, new SystemNotify()
            {
                IsSuccess = success,
                Message = success
                    ? NotifyMessage.CAP_NHAT_CA_OK.Message
                    : NotifyMessage.CAP_NHAT_CA_THAT_BAI.Message
            });

            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Branch Manager")]
        [HttpGet]
        public IActionResult SendMail(string email)
        {
            var model = new SendMailDTO
            {
                ToEmail = email
            };
            return View(model);
        }

        [Authorize(Roles = "Branch Manager")]
        [HttpPost]
        public async Task<IActionResult> SendMail(SendMailDTO dto)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId") ?? 0;
            if (!ModelState.IsValid)
            {

                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var errorMessage = string.Join(". ", errorMessages);

                ResponseHub.SetNotify(staffId, new SystemNotify()
                {
                    IsSuccess = false,
                    Message = errorMessage
                });

                return RedirectToAction("Index");
            }

            try
            {
                var host = _configuration["Email:Host"];
                var port = int.Parse(_configuration["Email:Port"] ?? "587");
                var fromAddress = _configuration["Email:Address"];
                var appPassword = _configuration["Email:AppPassword"];

                using var msg = new MailMessage(new MailAddress(fromAddress, "Cafe Management"),
                                                 new MailAddress(dto.ToEmail))
                {
                    Subject = dto.Subject,
                    Body = dto.Body,
                    IsBodyHtml = false
                };

                using var smtp = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromAddress, appPassword)
                };

                await smtp.SendMailAsync(msg);

                ResponseHub.SetNotify(staffId, new SystemNotify()
                {
                    IsSuccess = true,
                    Message = NotifyMessage.MAIL_THANH_CONG.Message
                });
            }
            catch (Exception ex)
            {
                ResponseHub.SetNotify(staffId, new SystemNotify()
                {
                    IsSuccess = false,
                    Message = $"{NotifyMessage.MAIL_THAT_BAI.Message} {ex.Message}"
                });
            }

            return RedirectToAction("Index");
        }

    }
}
