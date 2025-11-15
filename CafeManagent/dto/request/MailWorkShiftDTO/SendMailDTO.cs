using System.ComponentModel.DataAnnotations;

namespace CafeManagent.dto.request.MailWorkShiftDTO
{
    public class SendMailDTO
    {
        [Required, EmailAddress]
        public string ToEmail { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;
    }

}
