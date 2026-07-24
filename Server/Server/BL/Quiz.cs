using Server.DAL;

namespace Server.BL
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int CreatorId { get; set; }
        public bool IsPublic { get; set; }
        public int Likes { get; set; }
        public List<Question> Questions { get; set; }
        public int QuestionCount { get; set; }
        public List<int> AssociatedCountryIds { get; set; }

        public Quiz()
        {
            Questions = new List<Question>();
            AssociatedCountryIds = new List<int>();
            IsPublic = false; // default to private
            Likes = 0;
        }

        public Quiz(int id, string title, int creatorId, bool isPublic, int likes, List<Question> questions)
        {
            Id = id;
            Title = title;
            CreatorId = creatorId;
            IsPublic = isPublic;
            Likes = likes;
            Questions = questions ?? new List<Question>();
        }

        // ==========================================
        // CREATE
        // ==========================================
        public int Insert()
        {
            return DBServiceQuiz.CreateQuiz(this);
        }

        public int AddQuestion(int userId, Question q)
        {
            return DBServiceQuiz.AddQuestion(this.Id, userId, q);
        }

        // ==========================================
        // GET
        // ==========================================
        public static Quiz GetQuizById(int id)
        {
            return DBServiceQuiz.GetQuizById(id);
        }

        public static List<Quiz> GetAllPublicQuizzes()
        {
            return DBServiceQuiz.GetAllPublicQuizzes();
        }

        public static List<Quiz> GetQuizzesByUserId(int userId)
        {
            return DBServiceQuiz.GetQuizzesByUserId(userId);
        }

        // ==========================================
        // UPDATE
        // ==========================================
        public int UpdateQuiz(int userId)
        {
            return DBServiceQuiz.UpdateQuiz(this, userId);
        }

        public static int UpdateQuestion(int questionId, int userId, Question q)
        {
            return DBServiceQuiz.UpdateQuestion(questionId, userId, q);
        }

        public int PublishQuiz(int userId)
        {
            return DBServiceQuiz.PublishQuiz(this.Id, userId);
        }

        public int UnpublishQuiz(int userId)
        {
            return DBServiceQuiz.UnpublishQuiz(this.Id, userId);
        }

        public int ToggleLike(int userId)
        {
            return DBServiceQuiz.ToggleLike(this.Id, userId);
        }

        // ==========================================
        // DELETE
        // ==========================================
        public int DeleteQuiz(int userId)
        {
            return DBServiceQuiz.DeleteQuiz(this.Id, userId);
        }

        public static int DeleteQuestion(int questionId, int userId)
        {
            return DBServiceQuiz.DeleteQuestion(questionId, userId);
        }
    }
}
