using CafeManagent.Services;
using CafeManagent.Models;
using Microsoft.AspNetCore.Mvc;
using CafeManagent.dto.response;
using CafeManagent.Ulties;

namespace CafeManagent.Controllers.Staffs
{
    public class RequestPersonalController : Controller
    {
        private readonly IRequestService requestService;
        public RequestPersonalController(IRequestService requestService)
        {
            this.requestService = requestService;
        }
        public IActionResult Delete(int? id)
        {
            requestService.Delele(id);
            return RedirectToAction("Index");
        }
        public IActionResult Index(int? page)
        {
            int staffId = HttpContext.Session.GetInt32("StaffId").Value;
            List<Models.Request> requests = requestService.GetByStaffId(staffId);
            //paging
            int pageIndex = 1;
            if (page != null)
            {
                pageIndex = page.Value;
            }
            Paging<RequestBasic> paging = PagingUlti.PagingDoneRequest(requestService.GetDoneRequest(staffId), pageIndex);
            ViewBag.Paging = paging;
            return View(requests);
        }
    }
}
