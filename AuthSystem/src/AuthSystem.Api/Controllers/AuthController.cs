using Microsoft.AspNetCore.Mvc;
using AuthSystem.Api.Dtos;
using AuthSystem.Api.Services;

namespace AuthSystem.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService AuthService;
    private readonly INotificationPublisher _notifier;
    public AuthController(IAuthService authService, INotificationPublisher notifier)
    {
        _notifier = notifier;
        AuthService = authService;
    }

    [HttpPost("register")]
    public async Task<long> Register(RegisterDto userCreateDto)
    {
        var userId = await AuthService.SignUpUserAsync(userCreateDto);

        await _notifier.PublishAsync("auth.registered", new
        {
            email = userCreateDto.Email,
            name = userCreateDto.FirstName ?? userCreateDto.Email.Split('@')[0]
        });

        return userId;
    }

    [HttpPost("login")]
    public async Task<LoginResponseDto> Login(LoginDto userLoginDto)
    {
        var result = await AuthService.LoginUserAsync(userLoginDto);

        await _notifier.PublishAsync("auth.login", new
        {
            emailOrUserName = userLoginDto.EmailOrUserName,
            loginTime = DateTime.UtcNow
        });

        return result;
    }

    [HttpPost("google/register")]
    public async Task<long> GoogleRegister(GoogleAuthDto googleAuthDto)
    {
        var userId = await AuthService.GoogleRegisterAsync(googleAuthDto);

        await _notifier.PublishAsync("auth.registered", new
        {
            email = googleAuthDto.Email,
            name = googleAuthDto.FirstName ?? googleAuthDto.Email.Split('@')[0],
            provider = "Google"
        });

        return userId;
    }

    [HttpPost("google/login")]
    public async Task<LoginResponseDto> GoogleLogin(GoogleAuthDto googleAuthDto)
    {
        var result = await AuthService.GoogleLoginAsync(googleAuthDto);

        await _notifier.PublishAsync("auth.login", new
        {
            email = googleAuthDto.Email,
            name = result.UserName,
            provider = "Google"
        });

        return result;
    }
}
