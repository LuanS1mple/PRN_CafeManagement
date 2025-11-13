using CafeManagent.dto.response;
using CafeManagent.dto.response.RequestModuleDTO;
using CafeManagent.Models;

namespace CafeManagent.Ulties
{
    public class PagingUlti
    {
        private static int NumberPerPage = 10;
        public static Paging<RequestBasic> PagingDoneRequest(List<Request> requests, int page)
        {
            if (page < 1) page = 1;

            int totalItems = requests.Count;

            int totalPages = (int)Math.Ceiling(totalItems / (double)NumberPerPage);

            var items = requests
                .Skip((page - 1) * NumberPerPage)
                .Take(NumberPerPage)
                .Select(r => new RequestBasic
                {
                    Id = r.ReportId,
                    Date = r.ReportDate,
                    Description = r.Description,
                    StaffName = r.Staff.FullName,
                    Title = r.Title
                })
                .ToList();

            return new Paging<RequestBasic>
            {
               Total=  totalItems,
               PageIndex = page,
               Data = items
            };
        }
    }
}
