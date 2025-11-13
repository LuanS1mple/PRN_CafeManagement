using CafeManagent.Models;
using CafeManagent.Services.Interface.AuthenticationModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CafeManagent.Services.Imp.AuthenticationModule
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly string jwt_signer_key;
        private readonly int access_token_duration;
        private readonly int refresh_token_duration;
        private readonly CafeManagementContext _context;
        public AuthenticationService(CafeManagementContext context, IConfiguration configuration)
        {
            jwt_signer_key = configuration["jwt_signer_key"];
            access_token_duration = int.Parse(configuration["access_token_duration"] ?? "3600");
            refresh_token_duration = int.Parse(configuration["refresh_token_duration"] ?? "604800");
            _context = context;
        }
        public string CreateAccessToken(string refreshToken)
        {
            Staff staff = _context.RefreshTokens.Include(r => r.Staff).ThenInclude(s => s.Role).Where(s => s.Token.Equals(refreshToken)).FirstOrDefault().Staff;
            return CreateAccessToken(staff);
        }

        public async Task<string> CreateRefreshToken(string refreshToken)
        {
            var staff = GetByRefreshToken(refreshToken);
            string token = null;
            do
            {
                token = Guid.NewGuid().ToString().Substring(0, 8);
            }
            while (await _context.RefreshTokens.AnyAsync(s => s.Token.Equals(token)));
            RefreshToken newRefreshToken = new RefreshToken
            {
                Token = token,
                ExpireTime = DateTime.UtcNow.AddSeconds(refresh_token_duration),
                IsEnable = true,
                StaffId = staff.StaffId
            };
            _context.RefreshTokens.AddAsync(newRefreshToken);
            _context.SaveChangesAsync();
            return token;
        }

        private RefreshToken GetByRefreshToken(string oldToken)
        {
            return _context.RefreshTokens.Where(r => r.Token!.Equals(oldToken)).FirstOrDefault()!;
        }

        public ClaimsPrincipal GetClaims(string accessToken)
        {
            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "cafe-task",
                ValidateAudience = true,
                ValidAudience = "member",
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_signer_key)),
                ClockSkew = TimeSpan.Zero,
            };
            try
            {
                SecurityToken? token;
                return securityTokenHandler.ValidateToken(accessToken, parameters, out token);
            }
            catch
            {
                return null;
            }
        }

        public bool IsValidAccessToken(string accessToken)
        {
            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "cafe-task",
                ValidateAudience = true,
                ValidAudience = "member",
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_signer_key)),
                ClockSkew = TimeSpan.Zero,
            };
            try
            {
                SecurityToken? token;
                ClaimsPrincipal claims = securityTokenHandler.ValidateToken(accessToken, parameters, out token);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool IsValidRefreshToken(string refreshToken)
        {
            RefreshToken token = _context.RefreshTokens.Where(r => r.Token.Equals(refreshToken)).FirstOrDefault();
            if (token == null)
            {
                return false;
            }
            if (!token.IsEnable.GetValueOrDefault() || token.ExpireTime == null || token.ExpireTime.Value < DateTime.UtcNow)
            {
                return false;
            }
            return true;
        }

        public async Task<string> CreateRefreshToken(Staff staff)
        {
            string token = null;
            do
            {
                token = Guid.NewGuid().ToString().Substring(0, 8);
            }
            while (await _context.RefreshTokens.Where(s => s.Token.Equals(token)).AnyAsync());
            RefreshToken newAccessToken = new RefreshToken
            {
                Token = token,
                ExpireTime = DateTime.UtcNow.AddSeconds(refresh_token_duration),
                IsEnable = true,
                StaffId = staff.StaffId
            };
            _context.RefreshTokens.AddAsync(newAccessToken);
            _context.SaveChangesAsync();
            return token;
        }

        public string CreateAccessToken(Staff staff)
        {
            var signKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_signer_key));
            var creds = new SigningCredentials(signKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, staff.StaffId.ToString()),
                new Claim(ClaimTypes.Role, staff.Role.RoleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "cafe-task",
                audience: "member",
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(access_token_duration),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void DisableRefreshToken(string refreshToken)
        {
            RefreshToken token = _context.RefreshTokens.Where(r => r.Token.Equals(refreshToken)).FirstOrDefault()!;
            token.IsEnable = false;
            _context.RefreshTokens.Update(token);
            _context.SaveChanges();
        }

        public void DisableRefreshToken(int staffId)
        {
            _context.RefreshTokens.Where(r => r.StaffId == staffId).ExecuteUpdateAsync(setters => setters.SetProperty(r => r.IsEnable, false));
        }
    }
}
