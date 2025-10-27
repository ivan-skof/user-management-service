using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Core.DTOs
{
    public class ValidatePasswordRequest
    {
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}