using CafeManagent.Models;
using System.Security.Claims;

namespace CafeManagent.Services.Interface.AuthenticationModule
{
    public interface IAuthenticationService
    {
        string CreateAccessToken(string refreshToken);
        string CreateAccessToken(Staff staff);
        Task<string> CreateRefreshToken(string refreshToken);
        Task<string> CreateRefreshToken(Staff staff);
        void DisableRefreshToken(string refreshToken);
        void DisableRefreshToken(int staffId);
        ClaimsPrincipal GetClaims(string accessToken);
        ClaimsPrincipal GetClaimsIgnoreTime(string accessToken);
        public bool IsValidAccessToken(string refreshToken);
        public bool IsValidRefreshToken(string accessToken, ClaimsPrincipal userInfo);
    }
}
