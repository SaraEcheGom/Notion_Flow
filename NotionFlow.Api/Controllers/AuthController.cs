using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NotionFlow.Api.Data;
using NotionFlow.Api.DTOs;
using NotionFlow.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace NotionFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            IConfiguration config,
            AppDbContext context,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _config = config;
            _context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var role = dto.Role == "Teacher" ? "Professor" : dto.Role;

            // Token de registro de roles privilegiados viene de configuración
            var adminToken = _config["Auth:AdminRegistrationToken"];
            if (string.IsNullOrEmpty(adminToken))
            {
                _logger.LogError("Auth:AdminRegistrationToken no está configurado");
                return StatusCode(500, "Configuración de seguridad incompleta");
            }

            if (role == "Admin" && dto.Token != adminToken)
                return BadRequest("Token de administrador inválido");

            if (role == "Professor" && dto.Token != adminToken)
                return BadRequest("Solo un administrador puede crear profesores");

            int? institutionId = null;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var currentUser = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == currentUserId);
                institutionId = currentUser?.InstitutionId;
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                UserName = dto.Email,
                Role = role,
                InstitutionId = institutionId
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(user, role);
            _logger.LogInformation("Usuario registrado: {Email} con rol {Role}", dto.Email, role);

            return Ok("Usuario registrado exitosamente");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                _logger.LogWarning("Intento de login fallido para {Email}", dto.Email);
                return Unauthorized("Credenciales inválidas");
            }

            var token = GenerateToken(user);
            _logger.LogInformation("Login exitoso para {Email}", dto.Email);

            return Ok(new AuthResponseDto(
                token, user.Name, user.Email!, user.Role, user.Id, user.InstitutionId ?? 0));
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersByRole([FromQuery] string role)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Usuario no autenticado");

            var currentUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (currentUser == null)
                return NotFound("Usuario no encontrado");

            var users = await _userManager.GetUsersInRoleAsync(role);
            var institutionUsers = users
                .Where(u => u.InstitutionId == currentUser.InstitutionId)
                .ToList();

            return Ok(institutionUsers.Select(u => new AuthResponseDto(
                string.Empty, u.Name, u.Email!, u.Role, u.Id, u.InstitutionId ?? 0)));
        }

        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("InstitutionId", (user.InstitutionId ?? 0).ToString())
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
