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
            var hasher = new PasswordHasher<User>();
            var verificationResult = hasher.VerifyHashedPassword(user, user.Password, loginUser.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid username or password");
            }
            return Ok(user);
        }

        // --- Wishlist Endpoints ---

        [HttpPost("{userId}/wishlist/{countryId}")]
        public IActionResult AddToWishlist(int userId, int countryId)
        {
            int result = BL.User.addCountryToWishlist(userId, countryId);
            if (result == 0)
                return BadRequest("Failed to add country to wishlist.");
            return Ok(new { message = "Country added to wishlist successfully." });
        }

        [HttpDelete("{userId}/wishlist/{countryId}")]
        public IActionResult RemoveFromWishlist(int userId, int countryId)
        {
            int result = BL.User.removeCountryFromWishlist(userId, countryId);
            if (result == 0)
                return BadRequest("Failed to remove country from wishlist.");
            return Ok(new { message = "Country removed from wishlist successfully." });
        }

        [HttpGet("{userId}/wishlist")]
        public IActionResult GetWishlist(int userId)
        {
            var wishlist = BL.User.getWishlist(userId);
            return Ok(wishlist);
        }

        // --- Visited Endpoints ---

        [HttpPost("{userId}/visited/{countryId}")]
        public IActionResult AddToVisited(int userId, int countryId)
        {
            int result = BL.User.addCountryToVisited(userId, countryId);
            if (result == 0)
                return BadRequest("Failed to add country to visited list.");
            return Ok(new { message = "Country added to visited list successfully." });
        }

        [HttpDelete("{userId}/visited/{countryId}")]
        public IActionResult RemoveFromVisited(int userId, int countryId)
        {
            int result = BL.User.removeCountryFromVisited(userId, countryId);
            if (result == 0)
                return BadRequest("Failed to remove country from visited list.");
            return Ok(new { message = "Country removed from visited list successfully." });
        }

        [HttpGet("{userId}/visited")]
        public IActionResult GetVisited(int userId)
        {
            var visited = BL.User.getVisited(userId);
            return Ok(visited);
        }
    }
}
