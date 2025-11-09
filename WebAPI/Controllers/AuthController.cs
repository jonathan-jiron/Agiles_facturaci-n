using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AllowAnonymous = Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute;
using System.ComponentModel.DataAnnotations;

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

        public class LoginRequest
        {
            [Required, MinLength(3)] public string Username { get; set; } = "";
            [Required, MinLength(6)] public string Password { get; set; } = "";
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Datos inválidos", details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var username = req.Username.Trim().ToLowerInvariant();
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username.ToLower() == username);
            if (user == null)
                return Unauthorized(new { error = "Credenciales incorrectas" });

            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return Unauthorized(new { error = "Credenciales incorrectas" });

            var token = GenerateJwt(user);

            return Ok(new
            {
                token,
                usuario = new
                {
                    id = user.Id,
                    username = user.Username,
                    rol = user.Rol
                }
            });
        }

        private string GenerateJwt(Usuario user)
        {
            var jwtCfg = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCfg["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtCfg["ExpiresMinutes"]!));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: jwtCfg["Issuer"],
                audience: jwtCfg["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

#if DEBUG
        [HttpGet("dev/hash")]
        [AllowAnonymous]
        public IActionResult GetHash([FromQuery] string pwd) =>
            Ok(BCrypt.Net.BCrypt.HashPassword(pwd, workFactor: 11));
#endif
    }
}
