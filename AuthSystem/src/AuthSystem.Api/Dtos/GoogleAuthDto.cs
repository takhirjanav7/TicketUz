namespace AuthSystem.Api.Dtos;

public class GoogleAuthDto
{
    public string IdToken { get; set; }
    public string Email { get; set; }
    public string? FirstName { get; set; }
}
