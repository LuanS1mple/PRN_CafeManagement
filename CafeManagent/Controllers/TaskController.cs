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
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.Tasks.
                        Include(t => t.Staff).
                        Include(t => t.Manager).
                        ToListAsync();

            return View(tasks);
        }
    }
}
