using Server.BL;
using System.Data;
using System.Data.SqlClient;

namespace Server.DAL
{
    public class DBServiceUser : DBServiceBase
    {
        public static List<User> ReadUsers()
        {
            Connect();

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_Users2026_ReadAll", null);
            try
            {
                List<User> users = new();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        User user = new User();

                        user.Id = Convert.ToInt32(dr["Id"]);
                        user.Username = dr["Username"].ToString();
                        user.Password = dr["Password"].ToString();
                        user.Email = dr["Email"].ToString();
                        user.IsBlocked = Convert.ToBoolean(dr["IsBlocked"]);
                        user.IsAdmin = Convert.ToBoolean(dr["IsAdmin"]);
                        user.IsAllowedToShare = Convert.ToBoolean(dr["IsAllowedToShare"]);

                        users.Add(user);
                    }
                }
                return users;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static bool Register(User user)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@Username", user.Username },
                { "@Password", user.Password },
                { "@Email", user.Email },
                { "@IsBlocked", user.IsBlocked },
                { "@IsAdmin", user.IsAdmin },
                { "@IsAllowedToShare", user.IsAllowedToShare }
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_Add_User_2026", userParam);


            SqlParameter returnParameter = new SqlParameter();
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery(); // execute the command
                int result = Convert.ToInt32(returnParameter.Value);

                if (result != 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            finally
            {
                if (con != null) con.Close();
            }
        }

        public static bool UpdateUser(User user)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@Id", user.Id },
                { "@Username", user.Username },
                { "@Password", user.Password },
                { "@Email", user.Email },
                { "@IsBlocked", user.IsBlocked },
                { "@IsAdmin", user.IsAdmin },
                { "@IsAllowedToShare", user.IsAllowedToShare }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_Update_User2026", userParam);

            SqlParameter returnParameter = new SqlParameter();
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery(); // execute the command
                int result = Convert.ToInt32(returnParameter.Value);
                if (result == 1)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static bool DeleteUser(int userID)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@Id", userID }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_DeleteUser2026", userParam);

            SqlParameter returnParameter = new SqlParameter();
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery(); // execute the command
                int result = Convert.ToInt32(returnParameter.Value);
                if (result == 1)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static User GetUserByEmail(string email)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@Email", email }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_GetUserByEmail2026", userParam);
            try
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        User user = new User
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Username = dr["Username"].ToString(),
                            Password = dr["Password"].ToString(),
                            Email = dr["Email"].ToString(),
                            IsBlocked = Convert.ToBoolean(dr["IsBlocked"]),
                            IsAdmin = Convert.ToBoolean(dr["IsAdmin"]),
                            IsAllowedToShare = Convert.ToBoolean(dr["IsAllowedToShare"])
                        };
                        return user;
                    }
                }
                return null; // User not found
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }

        }
        
        public static bool BlockUser(int id)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_Block_User", userParam);
            
            SqlParameter returnParameter = new SqlParameter();
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery(); // execute the command
                int result = Convert.ToInt32(returnParameter.Value);
                if (result == 1)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }
        public static bool unblockUser(int id)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_Unblock_User", userParam);

            SqlParameter returnParameter = new SqlParameter();
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery(); // execute the command
                int result = Convert.ToInt32(returnParameter.Value);
                if (result == 1)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static bool preventSharing(int id)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_Prevent_Sharing", userParam);

            SqlParameter returnParameter = new SqlParameter();
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery(); // execute the command
                int result = Convert.ToInt32(returnParameter.Value);
                if (result == 1)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static bool allowSharing(int id)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_Allow_Sharing", userParam);

            SqlParameter returnParameter = new SqlParameter();
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery(); // execute the command
                int result = Convert.ToInt32(returnParameter.Value);
                if (result == 1)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }
        public static Dictionary<string, int> GetAdminStats()
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();

            Connect();
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_Get_Admin_Stats", null);

            SqlDataReader reader = null;

            try
            {
                reader = cmd.ExecuteReader();

                reader.Read();

                stats["login"] = Convert.ToInt32(reader["DailyLogins"]);
                stats["import"] = Convert.ToInt32(reader["ImportedCountries"]);
                stats["save"] = Convert.ToInt32(reader["SavedCountries"]);
                stats["share"] = Convert.ToInt32(reader["Shares"]);

                return stats;
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (con != null) con.Close();
            }
        }

        public static void AddLoginLog(int userId)
        {
            Connect();

            Dictionary<string, object> paramDic = new Dictionary<string, object>();
            paramDic.Add("@userId", userId);

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_Add_Login_Log", paramDic);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
            }
            finally
            {
                if (con != null) con.Close();
            }
        }
    }
}