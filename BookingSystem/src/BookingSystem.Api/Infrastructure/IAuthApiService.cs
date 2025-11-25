using BookingSystem.Api.Dtos;

namespace BookingSystem.Api.Infrastructure;

public interface IAuthApiService
{
    public Task<bool> ValidateUser(long userId);
    Task<UserInfoDto?> GetUserByIdAsync(long userId);  
}