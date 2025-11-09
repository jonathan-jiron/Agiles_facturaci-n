using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AllowAnonymous = Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Buscar usuario
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (usuario == null)
                {
                    return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
                }

                // Verificar contraseña con BCrypt
                bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
                
                if (!isValid)
                {
                    return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
                }

                // Generar token JWT
                var token = GenerateJwtToken(usuario);

                return Ok(new
                {
                    token,
                    usuario = new
                    {
                        id = usuario.Id,
                        username = usuario.Username,
                        rol = usuario.Rol
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

#if DEBUG
        [HttpGet("dev/hash")]
        [AllowAnonymous]
        public IActionResult GetHash([FromQuery] string pwd) =>
            Ok(BCrypt.Net.BCrypt.HashPassword(pwd, workFactor: 11));
#endif
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
