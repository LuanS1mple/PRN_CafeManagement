using CafeManagent.Models;

namespace CafeManagent.Services.Imp
{
    public class RequestService : IRequestService
    {
        private readonly CafeManagementContext _context;
        public RequestService(CafeManagementContext context)
        {
            _context = context;
        }
        public void Add(Request request)
        {
            _context.Requests.Add(request); 
            _context.SaveChanges();
        }
    }
}
