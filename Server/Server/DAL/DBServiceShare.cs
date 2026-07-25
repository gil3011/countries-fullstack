using Server.BL;
using System.Data;
using System.Data.SqlClient;

namespace Server.DAL
{
    public class DBServiceShare : DBServiceBase
    {
        public List<Share> GetAllShares()
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
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_SP_Shares_ReadAll", null);
            try
            {
                List<Share> shares = new();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Share share = new Share
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            UserId = Convert.ToInt32(dr["UserId"]),
                            Description = Convert.ToString(dr["Description"]) ?? "",
                            Type = Enum.Parse<ShareType>(dr["Type"].ToString()!, ignoreCase: true),
                            CountryId = Convert.ToInt32(dr["CountryId"]),
                            CreatedAt = Convert.ToDateTime(dr["CreatedAt"]),
                            Title = Convert.ToString(dr["Title"]) ?? "",
                            UserName = Convert.ToString(dr["UserName"]) ?? "",
                            CountryName = Convert.ToString(dr["CountryName"]) ?? "",
                        };
                        shares.Add(share);
                    }
                    return shares;
                }
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

        public List<Share> GetUserShares(int userId)
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
                { "@UserId", userId }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_SP_Shares_ReadUserShares", param);
            try
            {
                List<Share> shares = new();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Share share = new Share
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            UserId = Convert.ToInt32(dr["UserId"]),
                            Description = Convert.ToString(dr["Description"]) ?? "",
                            Type = Enum.Parse<ShareType>(dr["Type"].ToString()!, ignoreCase: true),
                            CountryId = Convert.ToInt32(dr["CountryId"]),
                            CreatedAt = Convert.ToDateTime(dr["CreatedAt"]),
                            Title = Convert.ToString(dr["Title"]) ?? "",
                            CountryName = Convert.ToString(dr["CountryName"]) ?? ""
                        };
                        shares.Add(share);
                    }
                    return shares;
                }
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

        public List<Share> GetCountryShares(string countryName)
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
                { "@CountryName", countryName }
            };
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con,"FP_SP_Shares_ReadCountryShares", param);
            try
            {
                List<Share> shares = new();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Share share = new Share
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            UserId = Convert.ToInt32(dr["UserId"]),
                            Description = Convert.ToString(dr["Description"]) ?? "",
                            Type = Enum.Parse<ShareType>(dr["Type"].ToString()!, ignoreCase: true),
                            CountryId = Convert.ToInt32(dr["CountryId"]),
                            CreatedAt = Convert.ToDateTime(dr["CreatedAt"]),
                            Title = Convert.ToString(dr["Title"]) ?? "",
                            UserName = Convert.ToString(dr["UserName"]) ?? "",
                            CountryName = Convert.ToString(dr["CountryName"]) ?? ""
                        };
                        shares.Add(share);
                    }
                    return shares;
                }
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


        public bool CreateShare(Share share)
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
                { "@UserId", share.UserId },
                { "@Description", share.Description },
                { "@Title", share.Title },
                { "@Type", share.Type.ToString() },
                { "@CountryId", share.CountryId }
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(
                con,
                "FP_SP_Shares_Create",
                param
            );

            SqlParameter returnParameter = new SqlParameter
            {
                ParameterName = "@ReturnValue",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.ReturnValue
            };

            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery();

                int result = Convert.ToInt32(returnParameter.Value);

                return result == 1;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }
        }

        public bool UpdateShare(Share share)
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
                { "@Id", share.Id },
                { "@UserId", share.UserId },
                { "@Description", share.Description },
                { "@Title", share.Title },
                { "@Type", share.Type.ToString() },
                { "@CountryId", share.CountryId }
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(
                con,
                "FP_SP_Shares_Update",
                param
            );

            try
            {
                int affectedRows = Convert.ToInt32(cmd.ExecuteScalar());
                return affectedRows > 0;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        public bool DeleteShare(int ShareID, int UserID)
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
                { "@Id", ShareID },
                { "@UserId", UserID }
            };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(
                con,
                "FP_SP_Shares_Delete",
                param
            );

            SqlParameter returnParameter = new SqlParameter
            {
                ParameterName = "@ReturnValue",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.ReturnValue
            };

            cmd.Parameters.Add(returnParameter);

            try
            {
                cmd.ExecuteNonQuery();

                int result = Convert.ToInt32(returnParameter.Value);

                return result == 1;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }
    }
}

