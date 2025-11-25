namespace AuthSystem.Api.Configurations;

public class RabbitMqConfiguration
{
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}