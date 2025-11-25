
using BookingSystem.Api.Dtos;
using System.Net.Http;

namespace BookingSystem.Api.Infrastructure;

public class AuthApiService : IAuthApiService
{
    private readonly HttpClient _client;

    public AuthApiService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("AuthSystem");
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(long userId)
    {
        var response = await _client.GetAsync($"api/users/{userId}");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<UserInfoDto>();
    }

    public async Task<bool> ValidateUser(long userId)
    {
        var response = await _client.GetAsync($"api/users/exists/{userId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<bool>();
    }
}
