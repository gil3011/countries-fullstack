using Microsoft.Extensions.Logging;
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

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Users_ReadAll", null);
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

        public static int Register(User user)
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

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Users_AddUser", userParam);
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
                            using (SqlCommand cmdCont = CreateCommandWithStoredProcedureGeneral("FP_sp_Add_User_Continent", contParams))
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
                            using (SqlCommand cmdLang = CreateCommandWithStoredProcedureGeneral("FP_SP_Add_Language_To_User", langParams))
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
            catch (Exception ex)
            {
                // log as needed
                throw;
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
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Users_UpdateUser", userParam);

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
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Users_DeleteUser", userParam);

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
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_Users_GetUserByEmail", userParam);
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

        public static List<string> GetContinentPrefernces(int userId)
        {
            Connect();

            var ParamDic = new Dictionary<string, object>
            {
                {"@userId", userId}
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_read_user_continents_preferences", ParamDic);

            List<string> continents = new();

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
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static Dictionary<string,string> GetUserLanguages(int userId)
        {
            Connect();

            var ParamDic = new Dictionary<string, object>
            {
                {"@userId", userId}
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_SP_read_user_languages", ParamDic);

            Dictionary<string, string> languages = new();

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
            finally
            {
                if (con != null) con.Close();
            }
        }

        // Country Wishlist 
        public static int addCountryToWishlist(int userId, int countryId)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_UserCountries_AddCountryToWishlist", userParam);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static int removeCountryFromWishlist(int userId, int countryId)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_UserCountries_RemoveCountryFromWishlist", userParam);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static List<Country> getWishlist(int userId)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_UserCountries_GetWishlist", param);
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
                    DBServiceCountry.LoadChildCollections(country.Id, country);
                }
                return countries;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        // Country Visited
        public static int addCountryToVisited(int userId, int countryId)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_UserCountries_AddCountryToVisited", userParam);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static int removeCountryFromVisited(int userId, int countryId)
        {
            Connect();
            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_UserCountries_RemoveCountryFromVisited", userParam);
            try
            {
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }

        public static List<Country> getVisited(int userId)
        {
            Connect();
            var param = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FP_sp_UserCountries_GetVisited", param);
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
                    DBServiceCountry.LoadChildCollections(country.Id, country);
                }
                return countries;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (con != null) con.Close();
            }
        }
    }
}