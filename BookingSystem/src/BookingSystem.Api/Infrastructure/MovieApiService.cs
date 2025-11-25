
using BookingSystem.Api.Dtos;

namespace BookingSystem.Api.Infrastructure;

public class MovieApiService : IMovieApiService
{
    private readonly HttpClient _client;

    public MovieApiService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("MovieSystem");
    }

    public async Task<ShowtimeInfoDto?> GetShowtimeAsync(long showtimeId)
    {
        try
        {
            var response = await _client.GetFromJsonAsync<ShowtimeInfoDto>($"api/showtimes/{showtimeId}");
            return response;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ValidateShowtime(long showtimeId, long seatId, decimal TotalPrice)
    {
        var response = await _client.GetAsync($"api/showtimes/{showtimeId}/seats/{seatId}/validate");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<bool>();
    }
}
