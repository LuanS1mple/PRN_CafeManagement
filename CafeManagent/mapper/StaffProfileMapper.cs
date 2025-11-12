using CafeManagent.dto.request;
using CafeManagent.dto.response;
using CafeManagent.Models;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagent.mapper
{
    public static class StaffProfileMapper
    {
        private const string DefaultAvatar = "/images/avatars/default.png";

        public static StaffProfile ToDto(Staff s, string? roleName)
        {
            if (s is null) throw new ArgumentNullException(nameof(s));

            return new StaffProfile(
                s.StaffId,
                s.RoleId,
                roleName,           
                s.FullName,
                s.Gender,
                s.BirthDate,
                s.Address,
                s.Phone,
                s.Email,
                s.UserName,
                s.CreateAt,
                string.IsNullOrWhiteSpace(s.Img) ? DefaultAvatar : s.Img
            );
        }


        public static void MapUpdate(UpdateStaffProfile src, Staff dest)
        {
            if (src is null) throw new ArgumentNullException(nameof(src));
            if (dest is null) throw new ArgumentNullException(nameof(dest));
            if (dest.StaffId != src.StaffId)
                throw new InvalidOperationException("Mismatched StaffId.");

            dest.FullName = TrimOrNull(src.FullName);
            dest.Gender = src.Gender;
            dest.BirthDate = src.BirthDate;
            dest.Address = TrimOrNull(src.Address);
            dest.Phone = TrimOrNull(src.Phone);
            dest.Email = TrimOrNull(src.Email);
            //if (!string.IsNullOrWhiteSpace(src.Password))
            //    dest.Password = src.Password; // TODO: hash nếu có auth
        }

        public static void SetAvatarPath(Staff dest, string? publicPath)
        {
            dest.Img = string.IsNullOrWhiteSpace(publicPath) ? DefaultAvatar : publicPath;
        }

        private static string? TrimOrNull(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
