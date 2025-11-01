using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Core.DTOs;

public class CreateUserRequest
{
    [Required]
    [StringLength(32)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(16)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(16)]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(16)]
    public string Culture { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
