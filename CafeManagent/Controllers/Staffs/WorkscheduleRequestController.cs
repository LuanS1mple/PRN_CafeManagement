using CafeManagent.dto.request;
using CafeManagent.dto.response;
using CafeManagent.Enums;
using CafeManagent.Hubs;
using CafeManagent.mapper;
using CafeManagent.Models;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CafeManagent.Controllers.Staffs
{
    public class WorkscheduleRequestController : Controller
    {
        private readonly IWorkScheduleService workScheduleService;
        private readonly IRequestService requestService;
        //hub
        private readonly IHubContext<ResponseHub> hubContext;
        public WorkscheduleRequestController(IWorkScheduleService workShiftService, IRequestService requestService,
            IHubContext<ResponseHub> hubContext)
        {
            this.workScheduleService = workShiftService;
            this.requestService = requestService;
            this.hubContext = hubContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Init()
        {
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            List<WorkScheduleBasicDTO> data = MapperHelper.FromWorkSchedule(workScheduleService.Get(staffId));
            ViewBag.WorkSchedules = data;
            return View();
        }
        [HttpGet]
        public IActionResult GetWorkSchedule(int id)
        {
            WorkScheduleDetailDTO schedule = MapperHelper.FromWorkSchedule(workScheduleService.GetById(id));
            return Json(schedule);
        }
        public IActionResult SaveWorkScheduleRequest(WorkScheduleRequestDTO requestDTO)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            try
            {
                Request request = MapperHelper.FromWorkScheduleRequestDTO(requestDTO, staffId);
                requestService.Add(request);
                SystemNotify systemNotify = new SystemNotify() {
                    IsSuccess = true,
                    Message = NotifyMessage.TAO_REQUEST_THANH_CONG.Message,
                };
                ResponseHub.SetNotify(staffId, systemNotify);
            }
            catch (Exception ex)
            {
                SystemNotify systemNotify = new SystemNotify()
                {
                    IsSuccess = false,
                    Message = NotifyMessage.TAO_REQUEST_THAT_BAI.Message,
                };
                ResponseHub.SetNotify(staffId, systemNotify);

            }
            return RedirectToAction("Init");
        }
      
    }
}
