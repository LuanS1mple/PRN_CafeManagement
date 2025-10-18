using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
{
    public class RequestController : Controller
    {
        public IActionResult Index()
        {
            return View("Request_List");
        }
    }
}
