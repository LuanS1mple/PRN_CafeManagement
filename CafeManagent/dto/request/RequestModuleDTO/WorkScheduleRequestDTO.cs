using CafeManagent.CustomValidation;
using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.RequestModuleDTO
{
    public class WorkScheduleRequestDTO
    {
        [Required]
        public string Title { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Mô tả không được dài quá 500 ký tự")]
        public string Description { get; set; }

        [Required]
        public DateOnly DateChange { get; set; }

        [Required]
        [Range(1, 3)] 
        public int OldShiftId { get; set; }

        [Required]
        public string Require { get; set; }

        [Required]
        [NewShiftDifferenceOldShift("OldShiftId", "Ca mới phải khác ca cũ")]
        public int NewShiftId { get; set; }
    }
}
