using Server.BL;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;

namespace Server.DAL
{
    public class DBServiceQuiz : DBServiceBase
    {
        // ==========================================
        // CREATE
        // ==========================================

        public int CreateQuiz(Quiz quiz)
        {
            SqlConnection con;
            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@Title", quiz.Title },
                { "@CreatorId", quiz.CreatorId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Quizzes_CreateQuiz", param);
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
                
                if (newQuizId > 0 && quiz.AssociatedCountryIds != null)
                {
                    foreach (var cId in quiz.AssociatedCountryIds)
                    {
                        AddQuizCountry(newQuizId, cId);
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

        private void AddQuizCountry(int quizId, int countryId)
        {
            SqlConnection con;
            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@QuizId", quizId },
                { "@CountryId", countryId }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Quizzes_AddCountry", param);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception) { throw; }
            finally { if (con != null) con.Close(); }
        }

        private void ClearQuizCountries(int quizId)
        {
            SqlConnection con;
            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object> { { "@QuizId", quizId } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Quizzes_ClearCountries", param);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception) { throw; }
            finally { if (con != null) con.Close(); }
        }

        private List<int> GetQuizCountries(int quizId)
        {
            SqlConnection con;
            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object> { { "@QuizId", quizId } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Quizzes_GetQuizCountries", param);
            List<int> list = new List<int>();
            try
            {
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(Convert.ToInt32(dr["CountryId"]));
                }
                return list;
            }
            catch (Exception) { throw; }
            finally { if (con != null) con.Close(); }
        }

        public int AddQuestion(int quizId, int userId, Question q)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@QuizId", quizId },
                { "@UserId", userId },
                { "@Text", q.Text },
                { "@OptionA", q.OptionA },
                { "@OptionB", q.OptionB },
                { "@OptionC", q.OptionC },
                { "@OptionD", q.OptionD }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Quizzes_AddQuestion", param);

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

        public int CreateAttempt(QuizAttempt attempt)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@QuizId", attempt.QuizId },
                { "@UserId", attempt.UserId },
                { "@Score", attempt.Score }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Quizzes_CreateAttempt", param);
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

        private void AddAttemptAnswer(int attemptId, int questionId, string userAnswer)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@AttemptId", attemptId },
                { "@QuestionId", questionId },
                { "@UserAnswer", userAnswer }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_AddAttemptAnswer", param);
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

        public Quiz GetQuizById(int id)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object> { { "@Id", id } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_GetQuizById", param);
            
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
                    quiz.Questions = GetQuestionsByQuizId(quiz.Id);
                    quiz.AssociatedCountryIds = GetQuizCountries(quiz.Id);
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

        public List<Question> GetQuestionsByQuizId(int quizId)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object> { { "@QuizId", quizId } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Quizzes_GetQuestionsByQuizId", param);
            
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
                            OptionD = dr["OptionD"].ToString()
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

        public List<Quiz> GetAllPublicQuizzes()
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_GetAllPublicQuizzes", null);
            
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
                            Likes = Convert.ToInt32(dr["Likes"]),
                            QuestionCount = Convert.ToInt32(dr["QuestionCount"])
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

        public List<Quiz> GetQuizzesByUserId(int userId)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var param = new Dictionary<string, object> { { "@UserId", userId } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_GetQuizzesByUserId", param);
            
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
                            Likes = Convert.ToInt32(dr["Likes"]),
                            QuestionCount = Convert.ToInt32(dr["QuestionCount"])
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

        public QuizAttempt GetAttempt(int attemptId)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var param = new Dictionary<string, object> { { "@AttemptId", attemptId } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_GetAttempt", param);
            
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
                            QuizTitle = dr["QuizTitle"].ToString(),
                            Answers = new Dictionary<int, string>()
                        };
                    }
                }
                
                if (attempt != null)
                {
                    // Fetch answers
                    // Ensure connection is open for second query

                    try
                    {
                        con = Connect(); // create the connection
                    }
                    catch (Exception ex)
                    {
                        // write to log
                        throw (ex);
                    }
                    var paramAnswers = new Dictionary<string, object> { { "@AttemptId", attemptId } };
                    SqlCommand cmdAnswers = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Quizzes_GetAttemptAnswers", paramAnswers);
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

        public List<QuizAttempt> GetAttemptsByUserId(int userId)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object> { { "@UserId", userId } };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_GetAttemptsByUserId", param);
            
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
                            QuizTitle = dr["QuizTitle"].ToString(),
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


        public int UpdateQuiz(Quiz quiz, int userId)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@Id", quiz.Id },
                { "@UserId", userId },
                { "@Title", quiz.Title }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_UpdateQuiz", param);
            try
            {
                object result = cmd.ExecuteScalar();
                int rows = (result != null) ? Convert.ToInt32(result) : 0;
                
                if (rows > 0)
                {
                    ClearQuizCountries(quiz.Id);
                    if (quiz.AssociatedCountryIds != null)
                    {
                        foreach (var cId in quiz.AssociatedCountryIds)
                        {
                            AddQuizCountry(quiz.Id, cId);
                        }
                    }
                }
                return rows;
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

        public int UpdateQuestion(int questionId, int userId, Question q)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var param = new Dictionary<string, object>
            {
                { "@Id", questionId },
                { "@UserId", userId },
                { "@Text", q.Text },
                { "@OptionA", q.OptionA },
                { "@OptionB", q.OptionB },
                { "@OptionC", q.OptionC },
                { "@OptionD", q.OptionD }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_UpdateQuestion", param);
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

        public int PublishQuiz(int quizId, int userId)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@Id", quizId },
                { "@UserId", userId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_PublishQuiz", param);
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

        public int UnpublishQuiz(int quizId, int userId)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@Id", quizId },
                { "@UserId", userId }
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Quizzes_UnpublishQuiz", param);
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

        public int DeleteQuiz(int quizId, int userId)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@Id", quizId },
                { "@UserId", userId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_DeleteQuiz", param);
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

        public int DeleteQuestion(int questionId, int userId)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@Id", questionId },
                { "@UserId", userId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_DeleteQuestion", param);
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

        public int DeleteAttempt(int attemptId, int userId)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@AttemptId", attemptId },
                { "@UserId", userId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_DeleteAttempt", param);
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
        public int ToggleLike(int quizId, int userId)
        {
            SqlConnection con;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var param = new Dictionary<string, object>
            {
                { "@QuizId", quizId },
                { "@UserId", userId }
            };
            
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Quizzes_ToggleLike", param);
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


