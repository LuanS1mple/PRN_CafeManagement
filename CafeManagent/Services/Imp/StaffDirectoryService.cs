using CafeManagent.dto.response;
using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services.Imp
{
    public class StaffDirectoryService : IStaffDirectoryService
    {
        private readonly CafeManagementContext _db;
        private const string DefaultAvatar = "/images/avatars/default.png";
        public StaffDirectoryService(CafeManagementContext db) => _db = db;

        private static string StatusName(int? s) => s switch
        {
            1 => "Đang làm việc",
            2 => "Nghỉ phép",
            3 => "Nghỉ việc",
            _ => "Không rõ"
        };

        public async Task<PagedResult<StaffListItemDto>> GetPagedAsync(StaffListQuery q, CancellationToken ct = default)
{
    // Base query: no tracking + projection 1 lần
    var src = _db.Staff
        .AsNoTracking()
        .Select(s => new
        {
            s.StaffId,
            s.FullName,
            s.Email,
            Title = s.Contract != null ? s.Contract.Position : null,
            Status = StatusName(s.Status),                           // giữ nguyên mapping status
            JoinDate = s.Contract != null ? s.Contract.StartDate : null,
            ContractEndDate = s.Contract != null ? s.Contract.EndDate : null,
            AvatarUrl = string.IsNullOrWhiteSpace(s.Img) ? DefaultAvatar : s.Img
        });

    // SEARCH: tên / email (accent-insensitive nếu có collation AI)
    if (!string.IsNullOrWhiteSpace(q.Q))
    {
        var kw = q.Q.Trim();

        src = src.Where(x =>
            EF.Functions.Like(
                EF.Functions.Collate(x.FullName ?? "", "SQL_Latin1_General_CP1_CI_AI"),
                $"%{kw}%"
            )
            ||
            EF.Functions.Like(
                EF.Functions.Collate(x.Email ?? "", "SQL_Latin1_General_CP1_CI_AI"),
                $"%{kw}%"
            )
        );

        // --- Fallback nếu DB bạn KHÔNG có collation SQL_Latin1_General_CP1_CI_AI ---
        // var low = kw.ToLower();
        // src = src.Where(x =>
        //     (x.FullName ?? "").ToLower().Contains(low) ||
        //     (x.Email ?? "").ToLower().Contains(low));
    }

    // FILTER: theo mã status (GIỮ NGUYÊN)
    if (q.Status.HasValue)
    {
        var name = StatusName(q.Status.Value);
        src = src.Where(x => x.Status == name);
    }

    // SORT: mặc định theo họ tên
    src = src.OrderBy(x => x.FullName);

    // PAGING
    var total = await src.CountAsync(ct);
    var page  = Math.Max(1, q.Page);
    var size  = Math.Clamp(q.Size, 5, 100);

    var items = await src
        .Skip((page - 1) * size)
        .Take(size)
        .Select(x => new StaffListItemDto(
            x.StaffId,
            x.FullName ?? "",
            x.Email,
            x.Title,
            x.Status,
            x.JoinDate,
            x.ContractEndDate,
            x.AvatarUrl
        ))
        .ToListAsync(ct);

    return new PagedResult<StaffListItemDto>
    {
        Items = items,
        Page = page,
        Size = size,
        TotalItems = total
    };
}


        public async Task<StaffDetailDto?> GetDetailAsync(int id, CancellationToken ct = default)
        {
            return await _db.Staff
                .AsNoTracking()
                .Where(s => s.StaffId == id)
                .Select(s => new StaffDetailDto(
                    s.StaffId,
                    s.FullName,
                    s.Email,
                    s.Contract != null ? s.Contract.Position : null,
                    StatusName(s.Status),
                    s.BirthDate,
                    s.Phone,
                    s.Address,
                    s.Contract != null ? s.Contract.StartDate : null,
                    s.Contract != null ? s.Contract.EndDate : null,
                    string.IsNullOrWhiteSpace(s.Img) ? DefaultAvatar : s.Img,
                    s.Role != null ? s.Role.RoleName : null
                ))
                .SingleOrDefaultAsync(ct);
        }
    }
}
