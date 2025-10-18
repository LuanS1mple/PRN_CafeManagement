using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
{
    public class AttendanceRequestController : Controller
    {
        public IActionResult Init()
        {
            return View();
        }
    }
}
