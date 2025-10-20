using CafeManagent.Models;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
{
    public class WorkShiftController : Controller
    {
        private readonly CafeManagementContext _context;
        public WorkShiftController(CafeManagementContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
