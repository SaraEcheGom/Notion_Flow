using Microsoft.AspNetCore.Identity;

namespace NotionFlow.Api.Models
{
    public class Usuario : IdentityUser
    {
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}