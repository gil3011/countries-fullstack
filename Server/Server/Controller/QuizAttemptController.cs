using Microsoft.AspNetCore.Mvc;
using Server.BL;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizAttemptController : ControllerBase
    {
        // GET: api/QuizAttempt/User/5
        [HttpGet("User/{userId}")]
        public IActionResult GetAttemptsByUserId(int userId)
        {
            var attempts = QuizAttempt.GetAttemptsByUserId(userId);
            return Ok(attempts);
        }

        // GET: api/QuizAttempt/5
        [HttpGet("{id}")]
        public IActionResult GetAttemptById(int id)
        {
            var attempt = QuizAttempt.GetAttempt(id);
            if (attempt == null)
            {
                return NotFound($"Quiz attempt with id {id} was not found.");
            }
            return Ok(attempt);
        }

        // POST: api/QuizAttempt
        [HttpPost]
        public IActionResult CreateAttempt([FromBody] QuizAttempt attempt)
        {
            int newId = attempt.Insert();
            if (newId > 0)
            {
                return Created($"/api/QuizAttempt/{newId}", newId);
            }
            return BadRequest("Failed to save the quiz attempt.");
        }

        // DELETE: api/QuizAttempt/5
        [HttpDelete("{id}")]
        public IActionResult DeleteAttempt(int id, [FromQuery] int userId)
        {
            QuizAttempt attempt = new QuizAttempt { Id = id };
            int result = attempt.DeleteAttempt(userId);
            
            if (result > 0)
            {
                return Ok(true);
            }
            return BadRequest("Cannot delete attempt: It does not exist or does not belong to you.");
        }
    }
}
