using CafeManagent.dto.request.WorkShiftModuleDTO;
using CafeManagent.Models;
using System.ComponentModel.DataAnnotations;

namespace CafeManagent.CustomValidation
{
    public class ValidateUpdateWorkShift : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var dto = (UpdateWorkShiftDTO)context.ObjectInstance;
            var db = (CafeManagementContext)context.GetService(typeof(CafeManagementContext));

            var schedule = db.WorkSchedules.FirstOrDefault(ws => ws.ShiftId == dto.ShiftId);
            if (schedule == null)
                return new ValidationResult("Không tìm thấy ca làm");

            if (dto.Date < DateOnly.FromDateTime(DateTime.Now))
                return new ValidationResult("Không thể sửa ca làm trong quá khứ");

            var staff = db.Staff.FirstOrDefault(s => s.FullName == dto.EmployeeName);
            if (staff == null)
                return new ValidationResult("Nhân viên không tồn tại");

            var shift = db.WorkShifts.FirstOrDefault(s => s.ShiftName == dto.ShiftType);
            if (shift == null)
                return new ValidationResult("Ca làm không tồn tại");

            bool existed = db.WorkSchedules.Any(ws =>
                ws.ShiftId != dto.ShiftId &&
                ws.Date == dto.Date &&
                ws.StaffId == staff.StaffId &&
                ws.WorkshiftId == shift.WorkshiftId);

            if (existed)
                return new ValidationResult("Nhân viên đã có ca làm trùng ngày/giờ");

            return ValidationResult.Success;
        }
    }
}
