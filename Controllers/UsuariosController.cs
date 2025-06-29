using Microsoft.AspNetCore.Mvc;
using MyApp.Business.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace MyApp.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Registrar([FromBody] RegisterDto dto)
        {
            var usuario = await _usuarioService.RegistrarUsuarioAsync(dto.Nombre, dto.Email, dto.Password);
            if (usuario == null)
                return BadRequest("El email ya está registrado.");

            return Ok(new { usuario.Id, usuario.Nombre, usuario.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var usuario = await _usuarioService.AutenticarAsync(dto.Email, dto.Password);
            if (usuario == null)
                return Unauthorized("Credenciales inválidas.");

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Name, usuario.Nombre),
        new Claim(ClaimTypes.Email, usuario.Email),
        new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "Usuario")
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MyUltraSecureJwtKey_ABC123456789XYZ!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                Token = tokenString,
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                Rol = usuario.Rol?.Nombre
            });
        


            return Ok(new { usuario.Id, usuario.Nombre, usuario.Email, Rol = usuario.Rol.Nombre });
        }
    }
    public record RegisterDto(string Nombre, string Email, string Password);
    public record LoginDto(string Email, string Password);
}

