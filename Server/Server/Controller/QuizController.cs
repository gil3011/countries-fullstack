using Microsoft.AspNetCore.Mvc;
using Server.BL;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        // GET: api/Quiz/Public
        [HttpGet("Public")]
        public IActionResult GetAllPublicQuizzes()
        {
            var quizzes = Quiz.GetAllPublicQuizzes();
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
        // Updates the Quiz title ONLY if IsPublic = 0 AND owned by userId
        [HttpPut("{id}")]
        public IActionResult UpdateQuiz(int id, [FromQuery] int userId, [FromBody] string newTitle)
        {
            Quiz quiz = new Quiz { Id = id };
            int result = quiz.UpdateQuiz(userId, newTitle);
            
            if (result > 0)
            {
                return Ok(true);
            }
            // EDIT RULE: Returns 400 Bad Request if it's public or not owned by user!
            return BadRequest("Cannot update quiz: The quiz is either public, does not exist, or you are not the creator.");
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
            return BadRequest("Cannot publish quiz: It may already be public, or you are not the creator.");
        }

        // POST: api/Quiz/5/Like
        [HttpPost("{id}/Like")]
        public IActionResult AddLike(int id)
        {
            Quiz quiz = new Quiz { Id = id };
            int result = quiz.AddLike();
            if (result > 0) return Ok(true);
            return BadRequest("Failed to add like.");
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
            return BadRequest("Cannot delete quiz: The quiz is public or you are not the creator.");
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
            // EDIT RULE: Prevents adding questions to public quizzes
            return BadRequest("Cannot add question: The quiz is public, does not exist, or you are not the creator.");
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
            // EDIT RULE: Prevents editing questions of public quizzes
            return BadRequest("Cannot update question: The quiz is public, does not exist, or you are not the creator.");
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
            // EDIT RULE: Prevents deleting questions from public quizzes
            return BadRequest("Cannot delete question: The quiz is public, does not exist, or you are not the creator.");
        }
    }
}
