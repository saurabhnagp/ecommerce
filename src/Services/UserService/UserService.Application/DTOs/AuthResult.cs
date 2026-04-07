namespace AmCart.UserService.Application.DTOs;

public class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public UserDto? User { get; set; }
    public TokenResponse? Tokens { get; set; }

    public static AuthResult Ok(UserDto user, TokenResponse? tokens, string? message = null) => new()
    {
        Success = true,
        User = user,
        Tokens = tokens,
        Message = message
    };

    public static AuthResult OkWithMessage(string message) => new()
    {
        Success = true,
        Message = message
    };

    public static AuthResult Fail(string errorCode, string message) => new()
    {
        Success = false,
        ErrorCode = errorCode,
        Message = message
    };
}
