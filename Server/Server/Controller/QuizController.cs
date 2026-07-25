using Microsoft.AspNetCore.Mvc;
using Server.BL;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        // GET: api/Quiz
        [HttpGet]
        public IEnumerable<Quiz> Get([FromQuery] int? countryId = null, [FromQuery] int? userId = null, [FromQuery] string region = null)
        {
            return Quiz.GetAllPublicQuizzes(countryId, userId, region);
        }

        // GET: api/Quiz/Public
        [HttpGet("Public")]
        public IActionResult GetAllPublicQuizzes([FromQuery] int? countryId = null, [FromQuery] int? userId = null, [FromQuery] string region = null)
        {
            var quizzes = Quiz.GetAllPublicQuizzes(countryId, userId, region);
            return Ok(quizzes);
        }

        // GET: api/Quiz/User/5
        [HttpGet("User/{userId}")]
        public IActionResult GetQuizzesByUserId(int userId)
        {
            var quizzes = Quiz.GetQuizzesByUserId(userId);
            return Ok(quizzes);
        }

        // GET: api/Quiz/5
        [HttpGet("{id}")]
        public IActionResult GetQuizById(int id)
        {
            var quiz = Quiz.GetQuizById(id);
            if (quiz == null)
            {
                return NotFound($"Quiz with id {id} was not found.");
            }
            return Ok(quiz);
        }

        // POST: api/Quiz
        [HttpPost]
        public IActionResult CreateQuiz([FromBody] Quiz quiz)
        {
            int newId = quiz.Insert();
            if (newId > 0)
            {
                return Created($"/api/Quiz/{newId}", newId);
            }
            return BadRequest("Failed to create the quiz.");
        }

        // PUT: api/Quiz/5
        // Updates the Quiz title and associated countries ONLY if owned by userId
        [HttpPut("{id}")]
        public IActionResult UpdateQuiz(int id, [FromQuery] int userId, [FromBody] Quiz updatedQuiz)
        {
            updatedQuiz.Id = id; // Ensure the ID matches the route
            int result = updatedQuiz.UpdateQuiz(userId);
            
            if (result > 0)
            {
                return Ok(true);
            }
            // EDIT RULE: Returns 403 Forbidden if not owned by user!
            return StatusCode(403, "Cannot update quiz: The quiz does not exist, or you are not the creator.");
        }

        // POST: api/Quiz/5/Publish
        // Sets IsPublic = 1. Cannot be undone!
        [HttpPost("{id}/Publish")]
        public IActionResult PublishQuiz(int id, [FromQuery] int userId)
        {
            Quiz quiz = new Quiz { Id = id };
            int result = quiz.PublishQuiz(userId);
            
            if (result > 0)
            {
                return Ok(true);
            }
            // PUBLISHING RULE: Cannot publish an already public quiz or unowned quiz
            return StatusCode(403, "Cannot publish quiz: It may already be public, or you are not the creator.");
        }

        // POST: api/Quiz/5/Unpublish
        // Sets IsPublic = 0.
        [HttpPost("{id}/Unpublish")]
        public IActionResult UnpublishQuiz(int id, [FromQuery] int userId)
        {
            Quiz quiz = new Quiz { Id = id };
            int result = quiz.UnpublishQuiz(userId);
            
            if (result > 0)
            {
                return Ok(true);
            }
            return StatusCode(403, "Cannot unpublish quiz: It may already be private, or you are not the creator.");
        }

        // POST: api/Quiz/5/Like
        [HttpPost("{id}/Like")]
        public IActionResult ToggleLike(int id, [FromQuery] int userId)
        {
            Quiz quiz = new Quiz { Id = id };
            int result = quiz.ToggleLike(userId);
            // result == 1 means Liked, result == 2 means Unliked.
            if (result > 0) return Ok(result);
            return StatusCode(403, "Failed to toggle like. Quiz may not exist or is not public.");
        }

        // DELETE: api/Quiz/5
        [HttpDelete("{id}")]
        public IActionResult DeleteQuiz(int id, [FromQuery] int userId)
        {
            Quiz quiz = new Quiz { Id = id };
            int result = quiz.DeleteQuiz(userId);
            
            if (result > 0)
            {
                return Ok(true);
            }
            return StatusCode(403, "Cannot delete quiz: The quiz does not exist or you are not the creator.");
        }

        // ==========================================
        // QUESTIONS ENDPOINTS
        // ==========================================

        // POST: api/Quiz/5/Question
        [HttpPost("{id}/Question")]
        public IActionResult AddQuestion(int id, [FromQuery] int userId, [FromBody] Question q)
        {
            Quiz quiz = new Quiz { Id = id };
            int newQId = quiz.AddQuestion(userId, q);
            
            if (newQId > 0)
            {
                return Created($"/api/Quiz/Question/{newQId}", newQId);
            }
            // EDIT RULE: Prevents adding questions to unowned quizzes
            return StatusCode(403, "Cannot add question: The quiz does not exist, or you are not the creator.");
        }

        // PUT: api/Quiz/Question/5
        [HttpPut("Question/{questionId}")]
        public IActionResult UpdateQuestion(int questionId, [FromQuery] int userId, [FromBody] Question q)
        {
            int result = Quiz.UpdateQuestion(questionId, userId, q);
            if (result > 0)
            {
                return Ok(true);
            }
            // EDIT RULE: Prevents editing questions of unowned quizzes
            return StatusCode(403, "Cannot update question: The quiz does not exist, or you are not the creator.");
        }

        // DELETE: api/Quiz/Question/5
        [HttpDelete("Question/{questionId}")]
        public IActionResult DeleteQuestion(int questionId, [FromQuery] int userId)
        {
            int result = Quiz.DeleteQuestion(questionId, userId);
            if (result > 0)
            {
                return Ok(true);
            }
            // EDIT RULE: Prevents deleting questions from unowned quizzes
            return StatusCode(403, "Cannot delete question: The quiz does not exist, or you are not the creator.");
        }
    }
}
