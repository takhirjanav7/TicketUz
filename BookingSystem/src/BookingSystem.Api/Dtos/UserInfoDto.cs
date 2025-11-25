namespace BookingSystem.Api.Dtos;

public record UserInfoDto(long Id, string Email, string? FirstName, string? LastName);