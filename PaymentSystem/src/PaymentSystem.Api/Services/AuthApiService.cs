namespace PaymentSystem.Api.Services;

public class AuthApiService : IAuthApiService
{
    private readonly HttpClient _http;

    public AuthApiService(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("http://localhost:5001/"); // ← AuthSystem URL
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(long userId)
    {
        var response = await _http.GetAsync($"api/users/{userId}");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<UserInfoDto>();
    }
}