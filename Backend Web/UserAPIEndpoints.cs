using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text;

namespace Backend_Web
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;

        public UsersController(IMongoCollection<User> users)
        {
            _users = users;
        }

        [HttpGet("/users")]
        public async Task<IActionResult> GetUsers()
        {
            var filter = Builders<User>.Filter.Empty;
            var users = await _users.Find(filter).ToListAsync();
            return Ok(users);
        }

        [HttpPost("/users")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user?.FullName) || string.IsNullOrWhiteSpace(user?.UserName) || string.IsNullOrWhiteSpace(user?.Email) || string.IsNullOrWhiteSpace(user?.PasswordHash))
            {
                return BadRequest("Fill all fields!");
            }
            user.Id = ObjectId.GenerateNewId();
            var hashingPassword = new HashingPassword();
            var hashPass = hashingPassword.HashPassword(user.PasswordHash, out var salt);
            user.PasswordHash = hashPass;
            user.GeneratedSalt = Convert.ToBase64String(salt);
            await _users.InsertOneAsync(user);
            return Ok(user);
        }

        [HttpDelete("/users/delete/{email}")]
        public async Task<IActionResult> DeleteUserByEmail(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var result = await _users.DeleteOneAsync(filter);
            if (result.DeletedCount == 0)
            {
                return NotFound("User not found!");
            }
            return NoContent();
        }

        [HttpGet("/users/{email}/{password}")]
        public async Task<IActionResult> LogAUser(string email, string password)
        {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Email, email)
            );
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("User does not exist");
            }
            var hashingPassword = new HashingPassword();
            bool isPassCorrect = hashingPassword.VerifyPassword(password, user.PasswordHash, Convert.FromBase64String(user.GeneratedSalt));
            if (!isPassCorrect)
            {
                return NotFound("Incorrect password!");
            }
            return Ok(user);
        }
        [HttpGet("/users/{email}")]
        public async Task<IActionResult> CheckIfUserExists(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("User not found!");
            }
            return Ok();
        }
    }
}
