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
    }
}
