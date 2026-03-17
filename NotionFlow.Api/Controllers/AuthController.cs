using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NotionFlow.Api.DTOs;
using NotionFlow.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NotionFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IConfiguration _config;
        private const string AdminToken = "ADMIN";

        public AuthController(UserManager<Usuario> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto.Rol == "Admin" && dto.Token != AdminToken)
                return BadRequest("Token de administrador inválido");

            if (dto.Rol == "Profesor" && dto.Token != AdminToken)
                return BadRequest("Solo un administrador puede crear profesores");

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                UserName = dto.Email,
                Rol = dto.Rol
            };

            var result = await _userManager.CreateAsync(usuario, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(usuario, dto.Rol);

            return Ok("Usuario registrado correctamente");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var usuario = await _userManager.FindByEmailAsync(dto.Email);

            if (usuario == null || !await _userManager.CheckPasswordAsync(usuario, dto.Password))
                return Unauthorized("Credenciales inválidas");

            var token = GenerarToken(usuario);

            return Ok(new AuthResponseDto(
                token, usuario.Nombre, usuario.Email!, usuario.Rol, usuario.Id));
        }

        [HttpGet("usuarios")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObtenerUsuariosPorRol([FromQuery] string rol)
        {
            var usuarios = await _userManager.GetUsersInRoleAsync(rol);
            return Ok(usuarios.Select(u => new AuthResponseDto(
                string.Empty, u.Nombre, u.Email!, u.Rol, u.Id)));
        }

        private string GenerarToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(ClaimTypes.Email, usuario.Email!),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}