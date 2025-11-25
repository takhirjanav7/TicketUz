using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Api.Entities.Requests;

public class RegisterRequest
{
    [Required] public string Name { get; set; } = null!;
    [Required, EmailAddress] public string Email { get; set; } = null!;
    [Required, MinLength(6)] public string Password { get; set; } = null!;
}

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = null!;
    [Required] public string Password { get; set; } = null!;
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress] public string Email { get; set; } = null!;
}