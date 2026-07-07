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

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_Users2026_FinalProj_ReadAll", null);
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

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_Add_User_2026_FinalProj", userParam);


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
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SP_Update_User2026_FinalProj", userParam);

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
    }
}