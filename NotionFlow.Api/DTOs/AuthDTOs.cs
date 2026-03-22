namespace NotionFlow.Api.DTOs
{
    public record RegisterDto(
        string Name,
        string Email,
        string Password,
        string Role,
        string Token
    );

    public record LoginDto(
        string Email,
        string Password
    );

    public record AuthResponseDto(
        string Token,
        string Name,
        string Email,
        string Role,
        string Id
    );
}