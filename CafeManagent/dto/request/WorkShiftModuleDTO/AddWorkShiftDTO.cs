using CafeManagent.CustomValidation;
using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.WorkShiftModuleDTO
{
    [ValidateWorkShift]  
    public class AddWorkShiftDTO
    {
        [Required]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Tên không được chứa số")]
        public string EmployeeName { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Tên ca làm không được chứa số")]
        public string ShiftType { get; set; }
        [Required]
        public int ManagerId { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }
    }
}
