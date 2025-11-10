using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request
{
    public class UpdateRoleRequest
    {
        public int RoleId { get; set; }
        [Required] public string RoleName { get; set; } = "";
        public string? Description { get; set; }
    }

}
