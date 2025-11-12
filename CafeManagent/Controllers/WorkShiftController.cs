using CafeManagent.dto.request;
using CafeManagent.Models;
using CafeManagent.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Controllers
{
    public class WorkShiftController : Controller
    {
        private readonly IWorkShiftService _service;

        public WorkShiftController(IWorkShiftService service)
        {
            _service = service;
        }

        public IActionResult Index(int page = 1, int pageSize = 6)
        {
            int totalItems;
            var shifts = _service.GetPaged(page, pageSize, out totalItems);
            _service.GetFilterData(out var positions, out var shiftTypes, out var employees);

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Positions = positions;
            ViewBag.ShiftTypes = shiftTypes;
            ViewBag.Employees = employees;

            return View(shifts);
        }

        [HttpPost]
        public IActionResult FilterWorkShift(FilterWorkShiftDTO filter, int page = 1, int pageSize = 6)
        {
            int totalItems;
            var shifts = _service.Filter(filter, page, pageSize, out totalItems);
            _service.GetFilterData(out var positions, out var shiftTypes, out var employees);

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Positions = positions;
            ViewBag.ShiftTypes = shiftTypes;
            ViewBag.Employees = employees;

            return View("Index", shifts);
        }

        [HttpPost]
        public IActionResult AddWorkShift(AddWorkShiftDTO dto)
        {
            _service.Add(dto, out bool success, out string message);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateWorkShift(UpdateWorkShiftDTO dto)
        {
            _service.Update(dto, out bool success, out string message);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteWorkShift(int id)
        {
            _service.Delete(id, out bool success, out string message);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index");
        }
    }
}
