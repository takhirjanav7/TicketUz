namespace NotificationSystem.Api.Models;

public record NotificationMessage(
    string Type,
    object Data,
    DateTime Timestamp
);