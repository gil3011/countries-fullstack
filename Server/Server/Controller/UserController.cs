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


        // --- Admin Endpoints ---
        
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

        [HttpGet("GetDailyLoginCounts")]
        public IActionResult GetDailyLoginCounts()
        {
            var dailyLoginCounts = BL.User.GetDailyLoginCounts();
            return Ok(dailyLoginCounts);
        }

        [HttpPost("addContinentPreference/{userId}")]
        public IActionResult AddContinentPreference(int userId,[FromBody] string preference)
        {
            bool result = BL.User.AddContinentPreference(userId, preference);
            if (!result)
                return BadRequest("Adding preference failed");
            return Ok(new { message = "Preference added successfully" });
        }

        [HttpPost("removeContinentPreference/{userId}")]
        public IActionResult RemovePreference(int userId, [FromBody] string preference)
        {
            bool result = BL.User.RemoveContinentPreference(userId, preference);
            if (!result)
                return BadRequest("Removing preference failed");
            return Ok(new { message = "Preference removed successfully" });
        }

        [HttpPost("addLanguageToUser/{userId}")]
        public IActionResult AddLanguageToUser(int userId, [FromBody] string language, string lanLevel)
        {
            bool result = BL.User.AddLanguageToUser(userId, language, lanLevel);
            if (!result)
                return BadRequest("Adding preference failed");
            return Ok(new { message = "Language added successfully" });
        }

        [HttpPost("removeLanguageFromUser/{userId}")]
        public IActionResult RemoveLanguageFromUser(int userId, [FromBody] string language)
        {
            bool result = BL.User.RemoveLanguageFromUser(userId, language);
            if (!result)
                return BadRequest("Removing language failed");
            return Ok(new { message = "Language removed successfully" });
        }

        [HttpGet("getContinentPreferences/{userId}")]
        public IEnumerable<string> GetContinentPrefernces(int userId)
        {
            return BL.User.GetContinentPrefernces(userId);
        }

        [HttpGet("getUserLanguages/{userId}")]
        public IActionResult GetUserLanguages(int userId)
        {
            var result = BL.User.GetUserLanguages(userId);
            return Ok(result);
        }
    }
}
