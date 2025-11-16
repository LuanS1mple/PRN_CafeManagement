using System.ComponentModel.DataAnnotations;
using CafeManagent.CustomValidation;

namespace CafeManagent.dto.request.WorkShiftModuleDTO
{
    [ValidateUpdateWorkShift] 
    public class UpdateWorkShiftDTO
    {
        [Required]
        public int ShiftId { get; set; }

        [Required]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Không được chứa số")]
        public string EmployeeName { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Không được chứa số")]
        public string ShiftType { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }
    }
}
