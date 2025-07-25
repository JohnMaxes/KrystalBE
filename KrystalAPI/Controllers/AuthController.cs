using KrystalAPI.Data;
using KrystalAPI.Models;
using KrystalAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KrystalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserContext _users;
        private readonly JwtService _jwt;
        private readonly PasswordHasher<object> hasher = new();
        public AuthController(UserContext u_context, JwtService jwt) {
            _users = u_context;
            _jwt = jwt;
        }

        [HttpPost("refresh")]
        public async Task <ActionResult<string>> RefreshToken()
        {
            var refToken = Request.Cookies["X-Krystal-RefToken"];
            var reqBody = await Request.ReadFromJsonAsync<JsonElement>();
            Console.WriteLine($"Cookies: {refToken}");
            Console.WriteLine($"Token: {reqBody.GetProperty("token").GetString()}");
            var decodedToken = _jwt.ReadJwt(reqBody.GetProperty("token").GetString()!);
            if (!string.IsNullOrEmpty(refToken) && decodedToken != null)
            {
                var matchingRefToken = await _users.RefreshTokens.Where(token => token.Token == refToken).FirstOrDefaultAsync();
                if (matchingRefToken != null) {

                    await _users.RefreshTokens.Where(token => token.Token == matchingRefToken.Token).ExecuteDeleteAsync();
                    var targetUser = await _users.Users
                        .Where(user => user.ID.ToString() == decodedToken.id)
                        .FirstOrDefaultAsync();
                    if (targetUser == null) return NotFound("User not found!");
                    // generate new token
                    var newToken = _jwt.GenerateToken(targetUser);

                    // generate new refresh token
                    var newRefToken = Guid.NewGuid();

                    await _users.RefreshTokens.AddAsync(new()
                        {
                            Token = newRefToken.ToString(),
                            Expiration = DateTime.Now.AddDays(30),
                            IsRevoked = false,
                            User = targetUser
                        }
                    );

                    await _users.SaveChangesAsync();

                    Response.Cookies.Append("X-Krystal-RefToken", newRefToken.ToString(),
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddDays(30),
                            Path = "/api/auth/refresh"
                        }
                    );
                    return Ok(newToken);
                }
            }
            return Unauthorized("Invalid refresh token.");
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> BasicLogin(LoginCredentials cred)
        {
            try {
                var targetUser = await _users.Users
                    .Where(user => user.Username == cred.Username)
                    .FirstOrDefaultAsync();

                if (targetUser != null &&
                    hasher.VerifyHashedPassword(cred.Username+"KrystalHash"+cred.Password, targetUser.Password, cred.Password) != PasswordVerificationResult.Failed
                ) {
                    var token = _jwt.GenerateToken(targetUser);
                    var refToken = Guid.NewGuid();
                    Response.Cookies.Append("X-Krystal-RefToken", refToken.ToString(),
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddDays(30),
                            Path = "/api/auth/refresh"
                        }
                    );
                    await _users.RefreshTokens.AddAsync(new RefreshToken()
                        {
                            Token = refToken.ToString(),
                            Expiration = DateTime.Now.AddDays(30),
                            IsRevoked = false,
                            User = targetUser,
                        }
                    );
                    return Ok(token);
                }
                else return Unauthorized("Invalid username or password.");
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                return StatusCode(500);
            }
        }

        [HttpPost("signup")] // basic signup
        public async Task<ActionResult<User>> BasicSignup(SignupCredentials cred)
        {
            if (await _users.Users.Where(user => user.Username == cred.Username).FirstOrDefaultAsync() == null)
                try {
                    User newUser = new()
                    {
                        Username = cred.Username,
                        Password = hasher.HashPassword(cred.Username + "KrystalHash" + cred.Password, cred.Password),
                        Email = cred.Email,
                    };
                    await _users.Users.AddAsync(newUser);
                    await _users.SaveChangesAsync();

                    var refToken = Guid.NewGuid();
                    Response.Cookies.Append("X-Krystal-RefToken", refToken.ToString(),
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddDays(30),
                            Path = "/api/auth/refresh"
                        }
                    );
                    await _users.RefreshTokens.AddAsync(new RefreshToken()
                    {
                        Token = refToken.ToString(),
                        Expiration = DateTime.Now.AddDays(30),
                        IsRevoked = false,
                        User = newUser,
                    }
                    );
                    await _users.SaveChangesAsync();

                    var token = _jwt.GenerateToken(newUser);
                    return CreatedAtAction(nameof(BasicSignup), token);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex);
                    return StatusCode(500);
                }
            else return StatusCode(409, "User with Username already exists!");
        }
    }
}
