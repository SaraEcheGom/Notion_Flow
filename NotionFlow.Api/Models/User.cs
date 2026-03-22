using Microsoft.AspNetCore.Identity;

namespace NotionFlow.Api.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}