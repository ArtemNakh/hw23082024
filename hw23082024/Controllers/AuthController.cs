using hw23082024.DTOs;
using hw23082024.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hw23082024.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DBContextTask _context;
        private readonly IConfiguration _configuration;

        public AuthController(DBContextTask context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Username == model.Username);
            if (userExists)
                return BadRequest("User already exists");

            var user = new User
            {
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var jwt = JwtTokenHelper.GenerateToken(user, _configuration["Jwt:Secret"]);

            HttpContext.Session.SetInt32("id", user.Id);

            return Created("token", jwt);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Unauthorized();

            var token = JwtTokenHelper.GenerateToken(user, _configuration["Jwt:Secret"]);
            return Created("token", token);
        }


    }

}
