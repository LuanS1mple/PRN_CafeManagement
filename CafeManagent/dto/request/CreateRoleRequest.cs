using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request
{
    public class CreateRoleRequest
    {
        [Required] public string RoleName { get; set; } = "";
        public string? Description { get; set; }
    }
}
