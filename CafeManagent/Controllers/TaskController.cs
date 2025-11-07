using Microsoft.AspNetCore.Mvc;
using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CafeManagent.Controllers
{
    public class TaskController : Controller
    {
        private readonly CafeManagementContext _context;
        public TaskController(CafeManagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
    string searchStaff,
    string searchManager,
    DateTime? searchStartDate,
    DateTime? searchEndDate)
        {
            ViewData["CurrentStaffFilter"] = searchStaff;
            ViewData["CurrentManagerFilter"] = searchManager;
            if (searchStartDate.HasValue)
                ViewData["CurrentStartDateFilter"] = searchStartDate.Value.ToString("yyyy-MM-dd");
            if (searchEndDate.HasValue)
                ViewData["CurrentEndDateFilter"] = searchEndDate.Value.ToString("yyyy-MM-dd");

            var query = _context.Tasks
                                .Include(t => t.Staff)
                                .Include(t => t.Manager)
                                .AsQueryable();

            ViewBag.Staffs = _context.Staff.ToList();

            if (!string.IsNullOrEmpty(searchStaff))
                query = query.Where(t => t.Staff != null && t.Staff.FullName.Contains(searchStaff));

            if (!string.IsNullOrEmpty(searchManager))
                query = query.Where(t => t.Manager != null && t.Manager.FullName.Contains(searchManager));

            if (searchStartDate.HasValue)
                query = query.Where(t => t.AssignTime.HasValue && t.AssignTime.Value.Date >= searchStartDate.Value.Date);

            if (searchEndDate.HasValue)
                query = query.Where(t => t.AssignTime.HasValue && t.AssignTime.Value.Date <= searchEndDate.Value.Date);

            ViewBag.Managers = await _context.Staff.Where(s => s.RoleId == 1).ToListAsync();

            ViewBag.Staffs = await _context.Staff.ToListAsync();

            var tasks = await query.ToListAsync();
            return View(tasks);
        }


        //[HttpPost]
        //public async Task<IActionResult> Create(CafeManagent.Models.Task newTask)
        //{
        //    int? managerId = HttpContext.Session.GetInt32("StaffId");

        //    if (managerId == null)
        //    {
        //        return Unauthorized("You did not sign in!");
        //    }

        //    newTask.ManagerId = managerId;
        //    newTask.AssignTime = DateTime.Now;
        //    newTask.Status = 4;

        //    if (newTask.StaffId == 0)
        //    {
        //        newTask.StaffId = null;
        //    }

        //    _context.Tasks.Add(newTask);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public async Task<IActionResult> Create(CafeManagent.Models.Task newTask)
        {
            if (newTask.ManagerId == null || newTask.ManagerId == 0)
            {
                TempData["ModalErrorMessage"] = "Bạn phải chọn một Người quản lý.";
                TempData["OpenModal"] = true;
                return RedirectToAction("Index");
            }

            newTask.AssignTime = DateTime.Now;
            newTask.Status = 4;

            if (newTask.StaffId == 0)
            {
                newTask.StaffId = null;
            }

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã tạo công việc mới thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTask(CafeManagent.Models.Task updateTask)
        {
            var taskToUpdate = await _context.Tasks.FindAsync(updateTask.TaskId);

            if (taskToUpdate == null)
            {
                return RedirectToAction("Index");
            }

            taskToUpdate.TaskName = updateTask.TaskName;
            taskToUpdate.Description = updateTask.Description;
            taskToUpdate.ManagerId = updateTask.ManagerId;
            taskToUpdate.StaffId = updateTask.StaffId == 0 ? null : updateTask.StaffId;
            taskToUpdate.DueTime = updateTask.DueTime;

            if (updateTask.StaffId == 0)
            {
                updateTask.StaffId = null;
            }
            else
            {
                taskToUpdate.StaffId = updateTask.StaffId;
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật công việc thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật công việc: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}