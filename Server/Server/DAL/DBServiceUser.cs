using Microsoft.Extensions.Logging;
using Server.BL;
using System.Data;
using System.Data.SqlClient;

namespace Server.DAL
{
    public class DBServiceUser : DBServiceBase
    {
        // User functions

        public static List<User> ReadUsers()
        {
            List<User> users = new();
            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_Users_ReadAll", null))
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
                }
            }
            return users;
        }

        public static bool Register(User user)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@Username", user.Username },
                { "@Password", user.Password },
                { "@Email", user.Email },
                { "@IsBlocked", user.IsBlocked },
                { "@IsAdmin", user.IsAdmin },
                { "@IsAllowedToShare", user.IsAllowedToShare }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_Users_AddUser", userParam))
                {
                    SqlParameter returnParameter = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParameter);

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(returnParameter.Value);

                    if (result != 0)
                        return true;
                    return false;
                }
            }
        }

        public static bool UpdateUser(User user)
        {
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

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_Users_UpdateUser", userParam))
                {
                    SqlParameter returnParameter = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParameter);

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(returnParameter.Value);

                    if (result == 1)
                        return true;
                    return false;
                }
            }
        }

        public static bool DeleteUser(int userID)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@Id", userID }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_Users_DeleteUser", userParam))
                {
                    SqlParameter returnParameter = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParameter);

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(returnParameter.Value);

                    if (result == 1)
                        return true;
                    return false;
                }
            }
        }

        public static User GetUserByEmail(string email)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@Email", email }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_Users_GetUserByEmail", userParam))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new User
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Username = dr["Username"].ToString(),
                                Password = dr["Password"].ToString(),
                                Email = dr["Email"].ToString(),
                                IsBlocked = Convert.ToBoolean(dr["IsBlocked"]),
                                IsAdmin = Convert.ToBoolean(dr["IsAdmin"]),
                                IsAllowedToShare = Convert.ToBoolean(dr["IsAllowedToShare"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Admin functions

        public static bool BlockUser(int id)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_Block_User", userParam))
                {
                    SqlParameter returnParameter = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParameter);

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(returnParameter.Value);

                    if (result == 1)
                        return true;
                    return false;
                }
            }
        }

        public static bool unblockUser(int id)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_Unblock_User", userParam))
                {
                    SqlParameter returnParameter = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParameter);

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(returnParameter.Value);

                    if (result == 1)
                        return true;
                    return false;
                }
            }
        }

        public static bool preventSharing(int id)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_Prevent_Sharing", userParam))
                {
                    SqlParameter returnParameter = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParameter);

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(returnParameter.Value);

                    if (result == 1)
                        return true;
                    return false;
                }
            }
        }

        public static bool allowSharing(int id)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_Allow_Sharing", userParam))
                {
                    SqlParameter returnParameter = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParameter);

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(returnParameter.Value);

                    if (result == 1)
                        return true;
                    return false;
                }
            }
        }

        public static Dictionary<string, int> GetAdminStats()
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_Get_Admin_Stats", null))
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
                }
            }

            return stats;
        }

        public static Dictionary<DateTime, int> GetDailyLoginCounts()
        {
            Dictionary<DateTime, int> dailyLogins = new Dictionary<DateTime, int>();

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_UserLogins_ReadDailyCounts", null))
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
                }
            }

            return dailyLogins;
        }

        public static void AddLoginLog(int userId)
        {
            var paramDic = new Dictionary<string, object>
            {
                { "@userId", userId }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_Add_Login_Log", paramDic))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                    }
                }
            }
        }

        // User Preferences

        public static bool AddContinentPreference(int userId, string contincent)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@userId", userId},
                { "@continentName" , contincent}
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_Add_User_Continent", userParam))
                {
                    SqlParameter returnParameter = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParameter);

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(returnParameter.Value);

                    if (result == 1)
                        return true;
                    return false;
                }
            }
        }

        public static bool RemoveContinentPreference(int userId, string contincent)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@userId", userId},
                { "@continentName" , contincent}
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_Delete_User_Continent", userParam))
                {
                    SqlParameter returnParameter = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParameter);

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(returnParameter.Value);

                    if (result == 1)
                        return true;
                    return false;
                }
            }
        }

        public static bool AddLanguageToUser(int userId, string language, string LanLevel)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@userId", userId},
                { "@Language" , language},
                { "@lanLevel",  LanLevel}
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_Add_Language_To_User", userParam))
                {
                    SqlParameter returnParameter = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParameter);

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(returnParameter.Value);

                    if (result == 1)
                        return true;
                    return false;
                }
            }
        }

        public static bool RemoveLanguageFromUser(int userId, string language)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@userId", userId},
                { "@Language" , language}
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_Remove_Language_From_User", userParam))
                {
                    SqlParameter returnParameter = new SqlParameter
                    {
                        Direction = ParameterDirection.ReturnValue
                    };
                    cmd.Parameters.Add(returnParameter);

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(returnParameter.Value);

                    if (result == 1)
                        return true;
                    return false;
                }
            }
        }

        public static List<string> GetContinentPrefernces(int userId)
        {
            var ParamDic = new Dictionary<string, object>
            {
                {"@userId", userId}
            };

            List<string> continents = new();

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_read_user_continents_preferences", ParamDic))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            continents.Add(dr["ContinentName"].ToString());
                        }
                    }
                }
            }

            return continents;
        }

        public static Dictionary<string, string> GetUserLanguages(int userId)
        {
            var ParamDic = new Dictionary<string, object>
            {
                {"@userId", userId}
            };

            Dictionary<string, string> languages = new();

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_SP_read_user_languages", ParamDic))
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
                }
            }

            return languages;
        }

        // Country Wishlist 

        public static int addCountryToWishlist(int userId, int countryId)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_UserCountries_AddCountryToWishlist", userParam))
                {
                    object result = cmd.ExecuteScalar();
                    return (result != null) ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public static int removeCountryFromWishlist(int userId, int countryId)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_UserCountries_RemoveCountryFromWishlist", userParam))
                {
                    object result = cmd.ExecuteScalar();
                    return (result != null) ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public static List<Country> getWishlist(int userId)
        {
            var param = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            List<Country> countries = new();

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_UserCountries_GetWishlist", param))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Country c = DBServiceCountry.MapCountryFromReader(dr);
                            countries.Add(c);
                        }
                    }
                }
            }

            foreach (var country in countries)
            {
                DBServiceCountry.LoadChildCollections(country.Id, country);
            }

            return countries;
        }

        // Country Visited

        public static int addCountryToVisited(int userId, int countryId)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_UserCountries_AddCountryToVisited", userParam))
                {
                    object result = cmd.ExecuteScalar();
                    return (result != null) ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public static int removeCountryFromVisited(int userId, int countryId)
        {
            var userParam = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CountryId", countryId }
            };

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_UserCountries_RemoveCountryFromVisited", userParam))
                {
                    object result = cmd.ExecuteScalar();
                    return (result != null) ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public static List<Country> getVisited(int userId)
        {
            var param = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            List<Country> countries = new();

            using (SqlConnection con = GetOpenConnection())
            {
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral2(con, "FP_sp_UserCountries_GetVisited", param))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Country c = DBServiceCountry.MapCountryFromReader(dr);
                            countries.Add(c);
                        }
                    }
                }
            }

            foreach (var country in countries)
            {
                DBServiceCountry.LoadChildCollections(country.Id, country);
            }

            return countries;
        }
    }
}