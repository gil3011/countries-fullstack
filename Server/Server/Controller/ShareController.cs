using Microsoft.AspNetCore.Mvc;
using Server.BL;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShareController : ControllerBase
    {
        [HttpGet("ReadAllShares")]
        public IActionResult GetAllShares()
        {
            try
            {
                var shares = BL.Share.GetAllShares();
                return Ok(shares);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving shares.");
            }
        }

        [HttpGet("GetUserShares/{userId}")]
        public IActionResult GetUserShares(int userId)
        {
            try
            {
                var shares = BL.Share.GetUserShares(userId);
                return Ok(shares);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving user shares.");
            }
        }

        [HttpGet("GetCountryShares/{countryName}")]
        public IActionResult GetCountryShares(string countryName)
        {
            try
            {
                var shares = BL.Share.GetCountryShares(countryName);
                return Ok(shares);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving country shares.");
            }
        }

        [HttpPost("CreateShare")]
        public IActionResult CreateShare([FromBody] Share share)
        {
            try
            {
                IActionResult permissionError = CheckCanShare(share.UserId);
                if (permissionError != null)
                {
                    return permissionError;
                }

                bool result = BL.Share.CreateShare(share);
                if (result)
                {
                    return Ok(new { message = "Share created successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Share was not created" });
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the share." });
            }
        }

        [HttpPut("UpdateShare")]
        public IActionResult UpdateShare([FromBody] Share share)
        {
            try
            {
                IActionResult permissionError = CheckCanShare(share.UserId);
                if (permissionError != null)
                {
                    return permissionError;
                }

                bool result = BL.Share.UpdateShare(share);
                if (result)
                {
                    return Ok(new { message = "Share updated successfully" });
                }
                else
                {
                    return NotFound(new { message = "Share was not found" });
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the share.");
            }

        }

        [HttpDelete("DeleteShare")]
        public IActionResult DeleteShare([FromQuery] int shareID, [FromQuery] int userID)
        {
            try
            {
                bool result = BL.Share.DeleteShare(shareID, userID);
                if (result)
                {
                    return Ok(new { message = "Share deleted successfully" });
                }
                else
                {
                    return NotFound("Share was not found");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the share.");
            }
        }

        // Server-side authorization for publishing shares. A user whose sharing
        // privilege has been revoked (IsAllowedToShare == false), or who is
        // blocked, must not be able to create or update shares - the same rule
        // enforced for blocked users at login. Returns null when the user may
        // share, otherwise the error result to return to the caller.
        private IActionResult CheckCanShare(int userId)
        {
            BL.User user = BL.User.GetUserById(userId);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found." });
            }
            if (user.IsBlocked || !user.IsAllowedToShare)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not allowed to share." });
            }
            return null;
        }
    }
}
