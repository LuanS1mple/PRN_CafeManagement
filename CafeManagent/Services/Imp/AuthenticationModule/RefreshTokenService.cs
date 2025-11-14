using CafeManagent.Models;
using CafeManagent.Services.Interface.AuthenticationModule;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp.AuthenticationModule
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly CafeManagementContext _context;
        public RefreshTokenService(CafeManagementContext cafeManagementContext)
        {
            _context = cafeManagementContext;
        }
        public async System.Threading.Tasks.Task Clear(DateTime expireTime)
        {
            await _context.RefreshTokens.Where(r => r.ExpireTime <= expireTime).ExecuteDeleteAsync();
        }
    }
}
