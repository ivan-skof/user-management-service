using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementService.Data.Entities;

[Index(nameof(ApiClientId), nameof(UserName), IsUnique = true)]  
[Index(nameof(ApiClientId), nameof(Email), IsUnique = true)]     
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

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
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public string PasswordSalt { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [ForeignKey("ApiClient")]
    public int ApiClientId { get; set; }

    public ApiClient ApiClient { get; set; } = null!;

}