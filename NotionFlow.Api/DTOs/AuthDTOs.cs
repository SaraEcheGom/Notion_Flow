using System.Text.Json.Serialization;

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

    public class AuthResponseDto
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("institutionId")]
        public int InstitutionId { get; set; }

        public AuthResponseDto(string token, string name, string email, string role, string id, int institutionId)
        {
            Token = token;
            Name = name;
            Email = email;
            Role = role;
            Id = id;
            InstitutionId = institutionId;
        }
    }
}