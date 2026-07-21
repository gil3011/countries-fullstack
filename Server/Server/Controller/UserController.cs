using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Server.BL;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Conntroller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetUsers()
        {
            try
            {
                return Ok(Server.BL.User.Read());

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving data.");
            }
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            try
            {
                var hasher = new PasswordHasher<User>();
                user.Password = hasher.HashPassword(user, user.Password);

                bool result = user.Register();

                if (!result)
                    return BadRequest("User already exists");
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while registering the user.");
            }
        }

        [HttpPut("UpdateUser")]
        public IActionResult updateUser(User user)
        {
            try
            {
                var hasher = new PasswordHasher<User>();
                user.Password = hasher.HashPassword(user, user.Password);

                bool result = user.UpdateUser();
                if (!result)
                    return BadRequest("User update failed");
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the user.");
            }
        }

        [HttpDelete("DeleteUser/{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                bool result = BL.User.DeleteUser(id);
                if (!result)
                    return BadRequest("User deletion failed");
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the user.");
            }

        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginUser)
        {
            try
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
                BL.User.AddLoginLog(user.Id);
                return Ok(new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.IsAdmin,
                    user.IsAllowedToShare
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while logging in.");
            }
        }

        // --- Wishlist Endpoints ---

        [HttpPost("{userId}/wishlist/{countryId}")]
        public IActionResult AddToWishlist(int userId, int countryId)
        {
            try
            {
                int result = BL.User.addCountryToWishlist(userId, countryId);
                if (result == 0)
                    return BadRequest("Failed to add country to wishlist.");
                return Ok(new { message = "Country added to wishlist successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding country to wishlist.");
            }

        }

        [HttpDelete("{userId}/wishlist/{countryId}")]
        public IActionResult RemoveFromWishlist(int userId, int countryId)
        {
            try
            {
                int result = BL.User.removeCountryFromWishlist(userId, countryId);
                if (result == 0)
                    return BadRequest("Failed to remove country from wishlist.");
                return Ok(new { message = "Country removed from wishlist successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing country from wishlist.");
            }

        }

        [HttpGet("{userId}/wishlist")]
        public IActionResult GetWishlist(int userId)
        {
            try
            {
                var wishlist = BL.User.getWishlist(userId);
                return Ok(wishlist);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving wishlist.");
            }
        }

        // --- Visited Endpoints ---

        [HttpPost("{userId}/visited/{countryId}")]
        public IActionResult AddToVisited(int userId, int countryId)
        {
            try
            {
                int result = BL.User.addCountryToVisited(userId, countryId);
                if (result == 0)
                    return BadRequest("Failed to add country to visited list.");
                return Ok(new { message = "Country added to visited list successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding country to visited list.");
            }
        }

        [HttpDelete("{userId}/visited/{countryId}")]
        public IActionResult RemoveFromVisited(int userId, int countryId)
        {
            try
            {
                int result = BL.User.removeCountryFromVisited(userId, countryId);
                if (result == 0)
                    return BadRequest("Failed to remove country from visited list.");
                return Ok(new { message = "Country removed from visited list successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing country from visited list.");
            }
        }

        [HttpGet("{userId}/visited")]
        public IActionResult GetVisited(int userId)
        {
            try
            {
                var visited = BL.User.getVisited(userId);
                return Ok(visited);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving visited list.");
            }   
        }


        // --- Admin Endpoints ---
        
        [HttpPut("block/{id}")]
        public IActionResult BlockUser(int id)
        {
            try
            {
                bool result = BL.User.BlockUser(id);
                if (!result)
                    return BadRequest("User block failed");
                return Ok(new { message = "User blocked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while blocking the user.");
            }
        }

        [HttpPut("unblock/{id}")]
        public IActionResult UnblockUser(int id)
        {
            try
            {
                bool result = BL.User.UnblockUser(id);
                if (!result)
                    return BadRequest("User unblock failed");
                return Ok(new { message = "User unblocked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while unblocking the user.");
            }
        }

        [HttpPut("preventSharing/{id}")]
        public IActionResult PreventSharing(int id)
        {
            try
            {
                bool result = BL.User.PreventSharing(id);
                if (!result)
                    return BadRequest("User prevent sharing failed");
                return Ok(new { message = "User prevent sharing successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while preventing sharing for the user.");
            }
        }

        [HttpPut("allowSharing/{id}")]
        public IActionResult AllowSharing(int id)
        {
            try
            {
                bool result = BL.User.AllowSharing(id);
                if (!result)
                    return BadRequest("User allow sharing failed");
                return Ok(new { message = "User allow sharing successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while allowing sharing for the user.");
            }
        }

        [HttpGet("admin/stats")]
        public IActionResult GetStats()
        {
            try
            {
                var stats = BL.User.GetAdminStats();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving admin stats.");
            }
        }

        [HttpGet("GetDailyLoginCounts")]
        public IActionResult GetDailyLoginCounts()
        {
            try
            {
                var dailyLoginCounts = BL.User.GetDailyLoginCounts();
                return Ok(dailyLoginCounts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving daily login counts.");
            }
        }

        [HttpPost("addContinentPreference/{userId}")]
        public IActionResult AddContinentPreference(int userId,[FromBody] string preference)
        {
            try
            {
                bool result = BL.User.AddContinentPreference(userId, preference);
                if (!result)
                    return BadRequest("Adding preference failed");
                return Ok(new { message = "Preference added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding continent preference.");
            }
        }

        [HttpPost("removeContinentPreference/{userId}")]
        public IActionResult RemovePreference(int userId, [FromBody] string preference)
        {
            try
            {
                bool result = BL.User.RemoveContinentPreference(userId, preference);
                if (!result)
                    return BadRequest("Removing preference failed");
                return Ok(new { message = "Preference removed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing continent preference.");
            }
        }

        [HttpPost("addLanguageToUser/{userId}")]
        public IActionResult AddLanguageToUser(int userId, [FromBody] string language, string lanLevel)
        {
            try
            {
                bool result = BL.User.AddLanguageToUser(userId, language, lanLevel);
                if (!result)
                    return BadRequest("Adding preference failed");
                return Ok(new { message = "Language added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding language to user.");
            }
        }

        [HttpPost("removeLanguageFromUser/{userId}")]
        public IActionResult RemoveLanguageFromUser(int userId, [FromBody] string language)
        {
            try
            {
                bool result = BL.User.RemoveLanguageFromUser(userId, language);
                if (!result)
                    return BadRequest("Removing language failed");
                return Ok(new { message = "Language removed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing language from user.");
            }
        }

        [HttpGet("getContinentPreferences/{userId}")]
        public IActionResult GetContinentPrefernces(int userId)
        {
            try
            {
                var result = BL.User.GetContinentPrefernces(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving continent preferences.");
            }
        }

        [HttpGet("getUserLanguages/{userId}")]
        public IActionResult GetUserLanguages(int userId)
        {
            try
            {
                var result = BL.User.GetUserLanguages(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving user languages.");
            }
        }
    }
}
