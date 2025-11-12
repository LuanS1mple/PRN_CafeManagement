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

        public async Task<IActionResult> Index(int page = 1, int pageSize = 6)
        {
            var (shifts, totalItems) = await _service.GetPagedAsync(page, pageSize);
            var (positions, shiftTypes, employees) = await _service.GetFilterDataAsync();

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
        public async Task<IActionResult> FilterWorkShift(FilterWorkShiftDTO filter, int page = 1, int pageSize = 6)
        {
            var (shifts, totalItems) = await _service.FilterAsync(filter, page, pageSize);
            var (positions, shiftTypes, employees) = await _service.GetFilterDataAsync();

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
        public async Task<IActionResult> AddWorkShift(AddWorkShiftDTO dto)
        {
            var (success, message) = await _service.AddAsync(dto);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWorkShift(UpdateWorkShiftDTO dto)
        {
            var (success, message) = await _service.UpdateAsync(dto);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWorkShift(int id)
        {
            var (success, message) = await _service.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index");
        }


    }
}
