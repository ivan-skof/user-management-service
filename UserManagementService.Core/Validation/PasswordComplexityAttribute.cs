using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Core.Validation;
public class PasswordComplexityAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = value as string;

        if (string.IsNullOrWhiteSpace(password))
            return new ValidationResult("Password is required.");

        var errors = new List<string>();

        if (!password.Any(char.IsUpper))
            errors.Add("at least one uppercase letter");

        if (!password.Any(char.IsLower))
            errors.Add("at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            errors.Add("at least one digit");

        if (!password.Any(ch => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(ch)))
            errors.Add("at least one special character");

        if (errors.Any())
            return new ValidationResult($"Password must contain {string.Join(", ", errors)}.");

        return ValidationResult.Success;
    }
}