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
                var users = Server.BL.User.Read().Select(UserDto.FromUser);
                return Ok(users);

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

                user.Id = result;
                return Ok(UserDto.FromUser(user));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while registering the user.");
            }
        }

        [HttpDelete("{id}")]
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
                if (user.IsBlocked)
                {
                    // Credentials are valid but the account is blocked: deny access and
                    // do not record a login. 403 distinguishes this from bad credentials.
                    return StatusCode(StatusCodes.Status403Forbidden, "Your account has been blocked.");
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

        // Aggregate everything the profile page needs in a single round-trip.
        [HttpGet("{userId}/profile")]
        public IActionResult GetProfile(int userId)
        {
            try
            {
                var profile = new
                {
                    visited = BL.User.getVisited(userId),
                    wishlist = BL.User.getWishlist(userId),
                    continents = BL.User.GetContinentPrefernces(userId),
                    languages = BL.User.GetUserLanguages(userId)
                };
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the profile.");
            }
        }

        [HttpGet("GetContinentPreferences/{userId}")]
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

        [HttpGet("GetUserLanguages/{userId}")]
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
}
