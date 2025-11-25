using BookingSystem.Api.Dtos;

namespace BookingSystem.Api.Infrastructure;

public interface IMovieApiService
{
    public Task<bool> ValidateShowtime(long showtimeId, long seatId, decimal TotalPrice);
    public Task<ShowtimeInfoDto?> GetShowtimeAsync(long showtimeId);
}