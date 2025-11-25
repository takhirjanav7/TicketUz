namespace PaymentSystem.Api.Services;

// PaymentSystem.Api/Services/IAuthApiService.cs
public interface IAuthApiService
{
    Task<UserInfoDto?> GetUserByIdAsync(long userId);
}

public record UserInfoDto(long Id, string Email, string? FirstName = null, string? LastName = null);