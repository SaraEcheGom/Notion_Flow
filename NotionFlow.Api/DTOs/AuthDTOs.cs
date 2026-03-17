namespace NotionFlow.Api.DTOs
{
    public record RegisterDto(
        string Nombre,
        string Email,
        string Password,
        string Rol,
        string Token
    );

    public record LoginDto(
        string Email,
        string Password
    );

    public record AuthResponseDto(
        string Token,
        string Nombre,
        string Email,
        string Rol,
        string Id
    );
}