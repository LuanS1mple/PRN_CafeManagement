
using CafeManagent.Services;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using System.Security.Claims;

namespace CafeManagent.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        //private readonly IAuthenticationService authenticationService;
        public AuthenticationMiddleware(RequestDelegate next /*, IAuthenticationService authenticationService*/)
        {
            _next = next;
            //this.authenticationService = authenticationService;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var excludedPaths = new[]
            {
                "/Home/Login",
                "/Home/ProcessLogin",
            };

            if (excludedPaths.Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }
            var authenticationService =context.RequestServices.GetRequiredService<IAuthenticationService>();

            string accessToken = context.Request.Cookies["AccessToken"];
            string refreshToken = context.Request.Cookies["RefreshToken"];
            //accesstoken co
            if (!string.IsNullOrEmpty(accessToken) && authenticationService.IsValidAccessToken(accessToken))
            {
                //gan quyen va thong tin vao User
                ClaimsPrincipal userInfo = authenticationService.GetClaims(accessToken);
                context.User = userInfo;
            }
            //neu at het han hoac k co, check refresh
            else if (!string.IsNullOrEmpty(refreshToken) && !authenticationService.IsValidAccessToken(accessToken))
            {
                //check refreshToken
                if (authenticationService.IsValidRefreshToken(refreshToken))
                {
                    //disable refreshToken cu
                    authenticationService.DisableRefreshToken(refreshToken);
                    //tao moi accessToken, refreshToken
                    string newAccessToken = authenticationService.CreateAccessToken(refreshToken);
                    string newRefreshToken =await authenticationService.CreateRefreshToken(refreshToken);
                    ClaimsPrincipal userInfo = authenticationService.GetClaims(accessToken);
                    context.User = userInfo;
                    //them vao cookies
                    AddTokenToCookies(newRefreshToken, newAccessToken, context);
                }
                else
                {
                    context.Response.Redirect("/home/Login");
                    return;
                }
            }
            //co co token nao thoa mans
            else
            {
                context.Response.Redirect("/home/Login");
                return;
            }
            await _next(context);
        }
        private void AddTokenToCookies(string refreshToken, string accessToken, HttpContext context)
        {
            context.Response.Cookies.Append("RefreshToken", refreshToken);
            context.Response.Cookies.Append("AccessToken", accessToken);
        }
    }
}
