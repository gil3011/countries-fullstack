using Server.BL;
using System.Data;
using System.Data.SqlClient;

namespace Server.DAL
{
    public class DBServiceQuiz : DBServiceBase
    {
        // ==========================================
        // CREATE
        // ==========================================

        public static int CreateQuiz(Quiz quiz)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@Title", quiz.Title },
                { "@CreatorId", quiz.CreatorId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_CreateQuiz", param);
            try
            {
                object result = cmd.ExecuteScalar();
                int newQuizId = (result != null) ? Convert.ToInt32(result) : 0;
                
                if (newQuizId > 0 && quiz.Questions != null)
                {
                    foreach (var q in quiz.Questions)
                    {
                        AddQuestion(newQuizId, quiz.CreatorId, q);
                    }
                }
                
                return newQuizId;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static int AddQuestion(int quizId, int userId, Question q)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@QuizId", quizId },
                { "@UserId", userId },
                { "@Text", q.Text },
                { "@OptionA", q.OptionA },
                { "@OptionB", q.OptionB },
                { "@OptionC", q.OptionC },
                { "@OptionD", q.OptionD },
                { "@CorrectAnswer", q.CorrectAnswer }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_AddQuestion", param);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // We don't close the connection here if we are looping in CreateQuiz, 
                // but since DBServiceBase creates a new connection per call, we close it.
                if (con != null) con.Close();
            }
        }

        public static int CreateAttempt(QuizAttempt attempt)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@QuizId", attempt.QuizId },
                { "@UserId", attempt.UserId },
                { "@Score", attempt.Score }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_CreateAttempt", param);
            try
            {
                object result = cmd.ExecuteScalar();
                int newAttemptId = (result != null) ? Convert.ToInt32(result) : 0;

                if (newAttemptId > 0 && attempt.Answers != null)
                {
                    foreach (var kvp in attempt.Answers)
                    {
                        AddAttemptAnswer(newAttemptId, kvp.Key, kvp.Value);
                    }
                }
                
                return newAttemptId;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        private static void AddAttemptAnswer(int attemptId, int questionId, string userAnswer)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@AttemptId", attemptId },
                { "@QuestionId", questionId },
                { "@UserAnswer", userAnswer }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_AddAttemptAnswer", param);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        // ==========================================
        // GET
        // ==========================================

        public static Quiz GetQuizById(int id)
        {
            Connect();
            var param = new Dictionary<string, object> { { "@Id", id } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_GetQuizById", param);
            
            Quiz quiz = null;
            try
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        quiz = new Quiz
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Title = dr["Title"].ToString(),
                            CreatorId = Convert.ToInt32(dr["CreatorId"]),
                            IsPublic = Convert.ToBoolean(dr["IsPublic"]),
                            Likes = Convert.ToInt32(dr["Likes"])
                        };
                    }
                }
                
                if (quiz != null)
                {
                    quiz.Questions = GetQuestionsByQuizId(id);
                }
                return quiz;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static List<Question> GetQuestionsByQuizId(int quizId)
        {
            Connect();
            var param = new Dictionary<string, object> { { "@QuizId", quizId } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_GetQuestionsByQuizId", param);
            
            List<Question> questions = new List<Question>();
            try
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        questions.Add(new Question
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Text = dr["Text"].ToString(),
                            OptionA = dr["OptionA"].ToString(),
                            OptionB = dr["OptionB"].ToString(),
                            OptionC = dr["OptionC"].ToString(),
                            OptionD = dr["OptionD"].ToString(),
                            CorrectAnswer = dr["CorrectAnswer"].ToString()
                        });
                    }
                }
                return questions;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static List<Quiz> GetAllPublicQuizzes()
        {
            Connect();
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_GetAllPublicQuizzes", null);
            
            List<Quiz> quizzes = new List<Quiz>();
            try
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        quizzes.Add(new Quiz
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Title = dr["Title"].ToString(),
                            CreatorId = Convert.ToInt32(dr["CreatorId"]),
                            IsPublic = Convert.ToBoolean(dr["IsPublic"]),
                            Likes = Convert.ToInt32(dr["Likes"])
                        });
                    }
                }
                return quizzes;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static List<Quiz> GetQuizzesByUserId(int userId)
        {
            Connect();
            var param = new Dictionary<string, object> { { "@UserId", userId } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_GetQuizzesByUserId", param);
            
            List<Quiz> quizzes = new List<Quiz>();
            try
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        quizzes.Add(new Quiz
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Title = dr["Title"].ToString(),
                            CreatorId = Convert.ToInt32(dr["CreatorId"]),
                            IsPublic = Convert.ToBoolean(dr["IsPublic"]),
                            Likes = Convert.ToInt32(dr["Likes"])
                        });
                    }
                }
                return quizzes;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static QuizAttempt GetAttempt(int attemptId)
        {
            Connect();
            var param = new Dictionary<string, object> { { "@AttemptId", attemptId } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_GetAttempt", param);
            
            QuizAttempt attempt = null;
            try
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        attempt = new QuizAttempt
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            QuizId = Convert.ToInt32(dr["QuizId"]),
                            UserId = Convert.ToInt32(dr["UserId"]),
                            Score = Convert.ToInt32(dr["Score"]),
                            DateTaken = Convert.ToDateTime(dr["DateTaken"]),
                            Answers = new Dictionary<int, string>()
                        };
                    }
                }
                
                if (attempt != null)
                {
                    // Fetch answers
                    Connect(); // Ensure connection is open for second query
                    var paramAnswers = new Dictionary<string, object> { { "@AttemptId", attemptId } };
                    SqlCommand cmdAnswers = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_GetAttemptAnswers", paramAnswers);
                    using (SqlDataReader dr = cmdAnswers.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            attempt.Answers[Convert.ToInt32(dr["QuestionId"])] = dr["UserAnswer"].ToString();
                        }
                    }
                }
                
                return attempt;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static List<QuizAttempt> GetAttemptsByUserId(int userId)
        {
            Connect();
            var param = new Dictionary<string, object> { { "@UserId", userId } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_GetAttemptsByUserId", param);
            
            List<QuizAttempt> attempts = new List<QuizAttempt>();
            try
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        attempts.Add(new QuizAttempt
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            QuizId = Convert.ToInt32(dr["QuizId"]),
                            UserId = Convert.ToInt32(dr["UserId"]),
                            Score = Convert.ToInt32(dr["Score"]),
                            DateTaken = Convert.ToDateTime(dr["DateTaken"]),
                            Answers = new Dictionary<int, string>() // empty as requested
                        });
                    }
                }
                return attempts;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        // ==========================================
        // UPDATE
        // ==========================================


        public static int UpdateQuiz(int quizId, int userId, string title)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@Id", quizId },
                { "@UserId", userId },
                { "@Title", title }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_UpdateQuiz", param);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static int UpdateQuestion(int questionId, int userId, Question q)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@Id", questionId },
                { "@UserId", userId },
                { "@Text", q.Text },
                { "@OptionA", q.OptionA },
                { "@OptionB", q.OptionB },
                { "@OptionC", q.OptionC },
                { "@OptionD", q.OptionD },
                { "@CorrectAnswer", q.CorrectAnswer }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_UpdateQuestion", param);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static int PublishQuiz(int quizId, int userId)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@Id", quizId },
                { "@UserId", userId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_PublishQuiz", param);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        // ==========================================
        // DELETE
        // ==========================================

        public static int DeleteQuiz(int quizId, int userId)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@Id", quizId },
                { "@UserId", userId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_DeleteQuiz", param);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static int DeleteQuestion(int questionId, int userId)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@Id", questionId },
                { "@UserId", userId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_DeleteQuestion", param);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static int DeleteAttempt(int attemptId, int userId)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@AttemptId", attemptId },
                { "@UserId", userId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_DeleteAttempt", param);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }
        public static int ToggleLike(int quizId, int userId)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@QuizId", quizId },
                { "@UserId", userId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Quizzes_ToggleLike", param);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }
    }
}


