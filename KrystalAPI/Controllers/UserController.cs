using KrystalAPI.Data;
using KrystalAPI.Models;
using KrystalAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KrystalAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _users;
        private readonly JwtService _jwt;
        public UserController(JwtService jwt, UserContext users)
        {
            _jwt = jwt;
            _users = users;
        }

        [HttpGet] [Authorize(Roles = "Admin, Moderator")]
        public async Task<ActionResult<List<User>>> GetUsers() {
            var list = await _users.Users.ToListAsync();
            return Ok(list);
        }
        [HttpGet("{id}")] [Authorize(Roles = "Admin, Moderator, User")]
        public async Task<ActionResult<User>> GetUserById(int id) {
            var tokenObj = _jwt.ToTokenObj(User);
            Console.WriteLine(tokenObj);
            if (int.Parse(tokenObj.id) != id) return Forbid();

            var targetUser = await _users.Users.Where(User => User.ID == id).FirstOrDefaultAsync();
            if (targetUser == null) return NotFound();
            return Ok(targetUser);
        }

        [HttpPost]
        public ActionResult<User> CreateUser(User newUser) {
            return Ok(0);
        }

        [HttpPut("{id}")]
        public ActionResult<User> EditUser(int id, User editedUser) {
            return Ok(0);
        }
    }
}
