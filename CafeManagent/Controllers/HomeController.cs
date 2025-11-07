using System.Diagnostics;
using CafeManagent.Models;
using CafeManagent.Services;
using CafeManagent.Services.Imp;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStaffService staffService;

        public HomeController(IStaffService staffService)
        {
            this.staffService = staffService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }

       
        [HttpPost]
        public IActionResult ProcessLogin(string Username, string Password, bool RememberMe)
        {
            var staff = staffService.Authentication(Username, Password);

            if (staff != null)
            {
                HttpContext.Session.SetInt32("StaffId", staff.StaffId);
                HttpContext.Session.SetString("StaffName", staff.FullName ?? "");
                HttpContext.Session.SetString("StaffRole", staff.Role?.RoleName ?? "");


                // Nếu RememberMe được chọn, có thể set cookie (tùy bạn)
                if (RememberMe)
                {
                    Response.Cookies.Append("RememberUsername", Username, new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7)
                    });
                }

                if(staff.Role.RoleName.Equals("Branch Manager"))
                {
                    return RedirectToAction("Index", "BranchManager");
                }else if (staff.Role.RoleName.Equals("Serve"))
                {
                    return RedirectToAction("Index", "Serve");
                }
                else if(staff.Role.RoleName.Equals("Cashier"))
                {
                    return RedirectToAction("waiter", "Order");
                }
                else
                {
                    return RedirectToAction("Bartender", "Order");
                }
                
            }
            ViewBag.ErrorMessage = "Email hoặc mật khẩu không đúng!";
            return View("Login");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
