namespace BookingSystem.Api.Dtos;

public record ShowtimeInfoDto
(
    string MovieTitle,
    string CinemaName,
    DateTime ShowDate,
    string ShowTime,
    string HallName = "Unknown"
);