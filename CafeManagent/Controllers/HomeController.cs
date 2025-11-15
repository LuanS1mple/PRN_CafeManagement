using System.Diagnostics;
using CafeManagent.Models;
using CafeManagent.Services.Imp;
using CafeManagent.Services.Interface.AuthenticationModule;
using CafeManagent.Services.Interface.StaffModule;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStaffService staffService;
        private readonly IAuthenticationService authenticationService;

        public HomeController(IStaffService staffService, IAuthenticationService authenticationService)
        {
            this.staffService = staffService;
            this.authenticationService = authenticationService;
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
        public async Task<IActionResult> ProcessLogin(string Username, string Password, bool RememberMe)
        {
            var staff = staffService.Authentication(Username, Password);

            if (staff != null)
            {
                HttpContext.Session.SetInt32("StaffId", staff.StaffId);
                HttpContext.Session.SetString("StaffName", staff.FullName ?? "");
                HttpContext.Session.SetString("StaffRole", staff.Role?.RoleName ?? "");
                //await CreateToken(staff, HttpContext);

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
                    return RedirectToAction("Waiter", "Order");
                }
                else
                {
                    return RedirectToAction("Bartender", "Order");
                }
                
            }
            ViewBag.ErrorMessage = "Username hoặc mật khẩu không đúng!";
            return View("Login");
        }

        //private async System.Threading.Tasks.Task CreateToken(Models.Staff staff, HttpContext context)
        //{
        //    //Vo hieu hoa cac token cu
        //    authenticationService.DisableRefreshToken(staff.StaffId);
        //    //gan vao cookie 
        //    string accessToken = authenticationService.CreateAccessToken(staff);
        //    string refreshToken =await authenticationService.CreateRefreshToken(staff);
        //    context.Response.Cookies.Append("RefreshToken", refreshToken);
        //    context.Response.Cookies.Append("AccessToken", accessToken);
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            if (Request.Cookies["RememberUsername"] != null)
            {
                Response.Cookies.Delete("RememberUsername");
            }
            return RedirectToAction("Login", "Home");
        }
    }
}
