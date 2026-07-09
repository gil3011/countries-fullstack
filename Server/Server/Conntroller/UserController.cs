using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server.BL;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Conntroller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<User> Get()
        {
            return Server.BL.User.Read();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password);

            bool result = user.Register();

            if (!result)
                return BadRequest("User already exists");

            return Ok(user);
        }

        [HttpPut("UpdateUser")]
        public IActionResult updateUser(User user)
        {
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password);

            bool result = user.UpdateUser();
            if (!result)
                return BadRequest("User update failed");
            return Ok(user);
        }

        [HttpDelete("DeleteUser/{id}")]
        public IActionResult DeleteUser(int id)
        {
            bool result = BL.User.DeleteUser(id);
            if (!result)
                return BadRequest("User deletion failed");
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginUser)
        {
            var user = BL.User.GetUserByEmail(loginUser.Email);


            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }
            if (user.IsBlocked)
            {
                return Unauthorized("User is blocked");
            }
            var hasher = new PasswordHasher<User>();
            var verificationResult = hasher.VerifyHashedPassword(user, user.Password, loginUser.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid username or password");
            }
            DAL.DBServiceUser.AddLoginLog(user.Id);
            return Ok(user);
        }
        [HttpPut("block/{id}")]
        public IActionResult BlockUser(int id)
        {
            bool result = BL.User.BlockUser(id);
            if (!result)
                return BadRequest("User block failed");
            return Ok(new { message = "User blocked successfully" });
        }

        [HttpPut("unblock/{id}")]
        public IActionResult UnblockUser(int id)
        {
            bool result = BL.User.UnblockUser(id);
            if (!result)
                return BadRequest("User unblock failed");
            return Ok(new { message = "User unblocked successfully" });
        }

        [HttpPut("preventSharing/{id}")]
        public IActionResult PreventSharing(int id)
        {
            bool result = BL.User.PreventSharing(id);
            if (!result)
                return BadRequest("User prevent sharing failed");
            return Ok(new { message = "User prevent sharing successfully" });
        }

        [HttpPut("allowSharing/{id}")]
        public IActionResult AllowSharing(int id)
        {
            bool result = BL.User.AllowSharing(id);
            if (!result)
                return BadRequest("User allow sharing failed");
            return Ok(new { message = "User allow sharing successfully" });
        }

        [HttpGet("admin/stats")]
        public IActionResult GetStats()
        {
            var stats = BL.User.GetAdminStats();
            return Ok(stats);
        }
    }
}
