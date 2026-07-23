using Microsoft.Extensions.Logging;
using Server.BL;
using System.Data;
using System.Data.SqlClient;

namespace Server.DAL
{
    public class DBServiceUser : DBServiceBase
    {
        // User functions

        public List<User> ReadUsers()
        {
            List<User> users = new();
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Users_ReadAll", null);

            try
            {
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

        public int Register(User user)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }
            var userParam = new Dictionary<string, object>
            {
                { "@Username", user.Username },
                { "@Password", user.Password },
                { "@Email", user.Email },
                { "@IsBlocked", user.IsBlocked },
                { "@IsAdmin", user.IsAdmin },
                { "@IsAllowedToShare", user.IsAllowedToShare }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Users_AddUser", userParam);

            SqlParameter returnParameter = new SqlParameter();
            returnParameter.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnParameter);

            try
            {

                cmd.ExecuteNonQuery(); // execute the command
                int result = Convert.ToInt32(returnParameter.Value);

                int newUserId = result;

                if (user.PreferdContinents != null && user.PreferdContinents.Count > 0)
                {
                    foreach (var continent in user.PreferdContinents)
                    {
                        try
                        {
                            var contParams = new Dictionary<string, object>
                            {
                                { "@userId", newUserId },
                                { "@continentName", continent }
                            };
                            using (SqlCommand cmdCont = CreateCommandWithStoredProcedureGeneral(con,"FP_sp_Add_User_Continent", contParams))
                            {
                                SqlParameter contRet = new SqlParameter();
                                contRet.Direction = ParameterDirection.ReturnValue;
                                cmdCont.Parameters.Add(contRet);
                                cmdCont.ExecuteNonQuery();
                            }
                        }
                        catch
                        {
                            // do not fail but log
                        }
                    }
                }

                if (user.LanguegeLevels != null && user.LanguegeLevels.Count > 0)
                {
                    foreach (var kv in user.LanguegeLevels)
                    {
                        try
                        {
                            var langParams = new Dictionary<string, object>
                            {
                                { "@userId", newUserId },
                                { "@Language", kv.Key },
                                { "@lanLevel", kv.Value.ToString() }
                            };
                            using (SqlCommand cmdLang = CreateCommandWithStoredProcedureGeneral(con,"FP_SP_Add_Language_To_User", langParams))
                            {
                                SqlParameter langRet = new SqlParameter();
                                langRet.Direction = ParameterDirection.ReturnValue;
                                cmdLang.Parameters.Add(langRet);
                                cmdLang.ExecuteNonQuery();
                            }
                        }
                        catch
                        {
                            // do not fail but log
                        }
                    }
            }

                return result;
            }
            catch (Exception)
            {
                // log as needed
                throw;
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public bool UpdateUser(User user)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

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

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Users_UpdateUser", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public bool DeleteUser(int userID)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@Id", userID }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Users_DeleteUser", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public User GetUserByEmail(string email)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@Email", email }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Users_GetUserByEmail", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public User GetUserById(int id)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Users_GetUserById", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public bool UpdatePassword(int userId, string hashedPassword)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@Id", userId },
                { "@Password", hashedPassword }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Users_UpdatePassword", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        // Admin functions
        public bool BlockUser(int id)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Block_User", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public bool unblockUser(int id)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Unblock_User", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public bool preventSharing(int id)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Prevent_User_Sharing", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public bool allowSharing(int id)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Allow_User_Sharing", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public Dictionary<string, int> GetAdminStats()
        {
            SqlConnection con;
            SqlCommand cmd;
            Dictionary<string, int> stats = new Dictionary<string, int>();

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Get_Admin_Stats", null);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        stats["DailyLogins"] = Convert.ToInt32(reader["DailyLogins"]);
                        stats["WishlistCountries"] = Convert.ToInt32(reader["WishlistCountries"]);
                        stats["VisitedCountries"] = Convert.ToInt32(reader["VisitedCountries"]);
                        stats["ImportedCountries"] = Convert.ToInt32(reader["ImportedCountries"]);
                        stats["Shares"] = Convert.ToInt32(reader["Shares"]);
                    }
                }
                return stats;
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

        public Dictionary<DateTime, int> GetDailyLoginCounts()
        {
            SqlConnection con;
            SqlCommand cmd;
            Dictionary<DateTime, int> dailyLogins = new Dictionary<DateTime, int>();

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_UserLogins_ReadDailyCounts", new Dictionary<string, object>());

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime loginDate = Convert.ToDateTime(reader["LoginDate"]);
                        int loginCount = Convert.ToInt32(reader["LoginCount"]);
                        dailyLogins.Add(loginDate, loginCount);
                    }
                }
                return dailyLogins;
            }
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public void AddLoginLog(int userId)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            Dictionary<string, object> paramDic = new Dictionary<string, object>();
            paramDic.Add("@userId", userId);

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Add_Login_Log", paramDic);

            try
            {
                cmd.ExecuteNonQuery();
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

        public bool promoteToAdmin(int userID)
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@userId", userID}
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Promote_To_Admin", userParam);

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

        public bool demoteFromAdmin(int userID)
        {
            SqlConnection con;
            SqlCommand cmd;
            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@userId", userID}
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Demote_From_Admin", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }


        // User Preferences
        public bool AddContinentPreference(int userId, string contincent)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@userId", userId},
                { "@continentName" , contincent}
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Add_User_Continent", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public bool RemoveContinentPreference(int userId, string contincent)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@userId", userId},
                { "@continentName" , contincent}
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Delete_User_Continent", userParam);

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

        public bool AddLanguageToUser(int userId, string language, string LanLevel)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@userId", userId},
                { "@Language" , language},
                { "@lanLevel",  LanLevel}
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Add_Language_To_User", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public bool RemoveLanguageFromUser(int userId, string language)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@userId", userId},
                { "@Language" , language}
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Remove_Language_From_User", userParam);

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
            catch (Exception)
            {
                // write to log
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public List<string> GetContinentPrefernces(int userId)
        {
            SqlConnection con;
            SqlCommand cmd;
            List<string> continents = new();

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var ParamDic = new Dictionary<string, object>
            {
                {"@userId", userId}
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_read_user_continents_preferences", ParamDic);

            try
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        continents.Add(dr["ContinentName"].ToString());
                    }
                }
                return continents;
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

        public Dictionary<string, string> GetUserLanguages(int userId)
        {
            SqlConnection con;
            SqlCommand cmd;
            Dictionary<string, string> languages = new();

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var ParamDic = new Dictionary<string, object>
            {
                {"@userId", userId}
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_read_user_languages", ParamDic);

            try
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string language = dr["LanguageName"].ToString();
                        string level = dr["Level"].ToString();
                        languages[language] = level;
                    }
                }
                return languages;
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

        // Country Wishlist 
        public int addCountryToWishlist(int userId, int countryId)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_UserCountries_AddCountryToWishlist", userParam);

            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
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

        public int removeCountryFromWishlist(int userId, int countryId)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_UserCountries_RemoveCountryFromWishlist", userParam);

            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
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

        public List<Country> getWishlist(int userId)
        {
            SqlConnection con;
            SqlCommand cmd;

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
                { "@UserId", userId }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_UserCountries_GetWishlist", param);

            DBServiceCountry dbs = new();
            try
            {
                List<Country> countries = new();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Country c = DBServiceCountry.MapCountryFromReader(dr);
                        countries.Add(c);
                    }
                }
                foreach (var country in countries)
                {
                    dbs.LoadChildCollections(country.Id, country);
                }
                return countries;
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

        // Country Visited
        public int addCountryToVisited(int userId, int countryId)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_UserCountries_AddCountryToVisited", userParam);

            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
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

        public int removeCountryFromVisited(int userId, int countryId)
        {
            SqlConnection con;
            SqlCommand cmd;

            try
            {
                con = Connect(); // create the connection
            }
            catch (Exception ex)
            {
                // write to log
                throw (ex);
            }

            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_UserCountries_RemoveCountryFromVisited", userParam);

            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
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

        public List<Country> getVisited(int userId)
        {
            SqlConnection con;
            SqlCommand cmd;

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
                { "@UserId", userId }
            };

            cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_UserCountries_GetVisited", param);
            DBServiceCountry dbs = new();
            try
            {
                List<Country> countries = new();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Country c = DBServiceCountry.MapCountryFromReader(dr);
                        countries.Add(c);
                    }
                }
                foreach (var country in countries)
                {
                    dbs.LoadChildCollections(country.Id, country);
                }
                return countries;
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
    }
}

