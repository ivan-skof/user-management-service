using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Core.DTOs;

public class UpdateUserRequest
{
    [StringLength(256)]
    public string? FullName { get; set; }
    
    [EmailAddress]
    [StringLength(256)]
    public string? Email { get; set; }

    [StringLength(16)] 
    public string? MobileNumber { get; set; }
    
    [StringLength(16)]
    public string? Language { get; set; }
    
    [StringLength(16)]
    public string? Culture { get; set; }
}