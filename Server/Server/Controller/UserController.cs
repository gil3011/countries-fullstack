using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Server.BL;
using Server.DTO;

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

                int result = user.Register();

                if (result == -1)
                    return Conflict("email already exists");
                if (result == -2)
                    return Conflict("username already exists");
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
        public IActionResult Login([FromBody] UserLoginDto loginUser)
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

        // --- Admin Endpoints ---

        [HttpPut("admin/blockUser/{id}")]
        public IActionResult blockUser(int id)
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
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving wishlist.");
            }
        }

        [HttpPut("admin/unblockUser/{id}")]
        public IActionResult unblockUser(int id)
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
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving wishlist.");
            }
        }

        [HttpPut("admin/preventSharing/{id}")]
        public IActionResult preventSharing(int id)
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
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving wishlist.");
            }
        }

        [HttpPut("admin/allowSharing/{id}")]
        public IActionResult allowSharing(int id)
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
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving wishlist.");
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
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving wishlist.");
            }
        }

        [HttpGet("admin/GetDailyLoginCounts")]
        public IActionResult GetDailyLoginCounts()
        {
            try
            {
                var loginCounts = BL.User.GetDailyLoginCounts();
                return Ok(loginCounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("admin/promote/{userId}")]
        public IActionResult PromoteToAdmin(int userId)
        {
            try
            {
                bool success = BL.User.PromoteToAdmin(userId);

                if (!success)
                {
                    return NotFound(new
                    {
                        message = "User was not found or could not be promoted."
                    });
                }

                return Ok(new
                {
                    message = "User was promoted to admin successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while promoting the user.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("admin/demote/{userId}")]
        public IActionResult DemoteFromAdmin(int userId)
        {
            try
            {
                bool success = BL.User.DemoteFromAdmin(userId);

                if (!success)
                {
                    return NotFound(new
                    {
                        message = "User was not found or could not be demoted."
                    });
                }

                return Ok(new
                {
                    message = "User was demoted from admin successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while demoting the user.",
                    error = ex.Message
                });
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

        [HttpPost("{userId}/moveToVisited/{countryId}")]
        public IActionResult MoveToVisited(int userId, int countryId)
        {
            bool result = BL.User.moveToVisited(userId, countryId);
            if (!result)
                return BadRequest("Failed to move country to visited list.");
            return Ok(new { message = "Country moved to visited list successfully." });
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

        // --- Continent Preference Endpoints ---

        [HttpPost("{userId}/continent/{continentName}")]
        public IActionResult AddContinentPreference(int userId, string continentName)
        {
            try
            {
                bool result = BL.User.AddContinentPreference(userId, continentName);
                if (!result)
                    return BadRequest("Failed to add continent preference.");
                return Ok(new { message = "Continent preference added successfully." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding continent preference.");
            }
        }

        [HttpDelete("{userId}/continent/{continentName}")]
        public IActionResult RemoveContinentPreference(int userId, string continentName)
        {
            try
            {
                bool result = BL.User.RemoveContinentPreference(userId, continentName);
                if (!result)
                    return BadRequest("Failed to remove continent preference.");
                return Ok(new { message = "Continent preference removed successfully." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing continent preference.");
            }
        }

        // --- Language Endpoints ---

        [HttpPost("{userId}/language")]
        public IActionResult AddLanguage(int userId, [FromBody] LanguageRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Language) || string.IsNullOrWhiteSpace(request.Level))
                    return BadRequest("Language and level are required.");

                bool result = BL.User.AddLanguageToUser(userId, request.Language, request.Level);
                if (!result)
                    return BadRequest("Failed to add language. Level must be Beginner, Intermediate, or Advanced.");
                return Ok(new { message = "Language added successfully." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the language.");
            }
        }

        [HttpDelete("{userId}/language/{language}")]
        public IActionResult RemoveLanguage(int userId, string language)
        {
            try
            {
                bool result = BL.User.RemoveLanguageFromUser(userId, language);
                if (!result)
                    return BadRequest("Failed to remove language.");
                return Ok(new { message = "Language removed successfully." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the language.");
            }
        }

        // --- Change Password ---

        [HttpPost("changePassword")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                    return BadRequest("Current and new passwords are required.");

                var user = BL.User.GetUserById(request.UserId);
                if (user == null)
                    return NotFound("User not found.");

                var hasher = new PasswordHasher<User>();
                var verification = hasher.VerifyHashedPassword(user, user.Password, request.CurrentPassword);
                if (verification == PasswordVerificationResult.Failed)
                    return Unauthorized("Current password is incorrect.");

                string newHash = hasher.HashPassword(user, request.NewPassword);
                bool updated = BL.User.UpdatePassword(request.UserId, newHash);
                if (!updated)
                    return BadRequest("Failed to update password.");

                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while changing the password.");
            }
        }
    }

    public class LanguageRequest
    {
        public string Language { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
