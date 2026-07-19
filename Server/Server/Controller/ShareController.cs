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
        public IEnumerable<Share> GetAllShares()
        {
            return BL.Share.GetAllShares();
        }

        [HttpGet("GetUserShares/{userId}")]
        public IEnumerable<Share> GetUserShares(int userId)
        {
            return BL.Share.GetUserShares(userId);
        }

        [HttpGet("GetCountryShares/{countryName}")]
        public IEnumerable<Share> GetCountryShares(string countryName)
        {
            return BL.Share.GetCountryShares(countryName);
        }

        [HttpPost("CreateShare")]
        public IActionResult CreateShare([FromBody] Share share)
        {
            bool result = BL.Share.CreateShare(share);
            if (result)
            {
                return Ok("Share created successfully");
            }
            else
            {
                return BadRequest("Share was not created");
            }
        }

        [HttpPut("UpdateShare")]
        public IActionResult UpdateShare([FromBody] Share share)
        {
            bool result = BL.Share.UpdateShare(share);
            if (result)
            {
                return Ok("Share updated successfully");
            }
            else
            {
                return NotFound("Share was not found");
            }
        }

        [HttpDelete("DeleteShare")]
        public IActionResult DeleteShare([FromQuery] int shareID, [FromQuery] int userID)
        {
            bool result = BL.Share.DeleteShare(shareID, userID);
            if (result)
            {
                return Ok("Share deleted successfully");
            }
            else
            {
                return NotFound("Share was not found");
            }
        }
    }
}
