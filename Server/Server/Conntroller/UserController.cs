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
    }
}
