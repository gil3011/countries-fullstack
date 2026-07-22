using Server.DAL;

namespace Server.BL
{
    public class QuizAttempt
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public int Score { get; set; }
        public DateTime DateTaken { get; set; }
        
        // Dictionary maps QuestionId -> User's Answer String
        public Dictionary<int, string> Answers { get; set; }

        public QuizAttempt()
        {
            Answers = new Dictionary<int, string>();
        }

        public QuizAttempt(int id, int quizId, int userId, int score, DateTime dateTaken, Dictionary<int, string> answers)
        {
            Id = id;
            QuizId = quizId;
            UserId = userId;
            Score = score;
            DateTaken = dateTaken;
            Answers = answers ?? new Dictionary<int, string>();
        }

        // ==========================================
        // CREATE
        // ==========================================
        public int Insert()
        {
            DBServiceQuiz dbs = new();
            return dbs.CreateAttempt(this);
        }

        // ==========================================
        // GET
        // ==========================================
        public static QuizAttempt GetAttempt(int attemptId)
        {
            DBServiceQuiz dbs = new();
            return dbs.GetAttempt(attemptId);
        }

        public static List<QuizAttempt> GetAttemptsByUserId(int userId)
        {
            DBServiceQuiz dbs = new();
            return dbs.GetAttemptsByUserId(userId);
        }

        // ==========================================
        // DELETE
        // ==========================================
        public int DeleteAttempt(int userId)
        {
            DBServiceQuiz dbs = new();
            return dbs.DeleteAttempt(this.Id, userId);
        }
    }
}
