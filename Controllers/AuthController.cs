using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalHub.Data;
using RentalHub.DTO;
using RentalHub.Helper;
using RentalHub.Services;

namespace RentalHub.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RentalHubDbContext _db;
        private readonly JwtService _jwt;

        public AuthController(RentalHubDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var user = await _db.Users
                        .FirstOrDefaultAsync(x => x.Username == request.Username && x.IsActive);

                if (user == null)
                    return Unauthorized("Invalid username or password");

                if (!PasswordHelper.Verify(request.Password, user.PasswordHash))
                    return Unauthorized("Invalid username or password");

                var token = _jwt.GenerateToken(user);

                return Ok(new
                {
                    token,
                    username = user.Username,
                    role = user.Role
                });
            }
            catch (Exception ex)
            {
                var user = await _db.Users
                                .FirstOrDefaultAsync(x => x.Username == request.Username && x.IsActive);

                if (user == null)
                    return Unauthorized("Invalid username or password");

                if (!PasswordHelper.Verify(request.Password, user.PasswordHash))
                    return Unauthorized("Invalid username or password");

                var token = _jwt.GenerateToken(user);

                return Ok(new
                {
                    token,
                    username = user.Username,
                    role = user.Role
                });
            }
        }
    }
}
