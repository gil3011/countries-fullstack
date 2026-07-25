using Server.BL;
using System.Data;
using System.Data.SqlClient;

namespace Server.DAL
{
    public class DBServiceShare : DBServiceBase
    {
        public List<Share> GetAllShares()
        {
            SqlConnection con = null;

            try
            {
                con = Connect();
                SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Shares_ReadAll", null);

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
                            Cca3 = Convert.ToString(dr["cca3"]) ?? ""
                        };
                        shares.Add(share);
                    }
                    return shares;
                }

            }
            catch (Exception ex)
            {
                Server.Logging.AppLogger.LogException(ex);
                throw;
            }

            finally
            {
                if (con != null) con.Close();
            }
        }

        public List<Share> GetUserShares(int userId)
        {
            SqlConnection con = null;

            try
            {
                con = Connect();
                var param = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };
                SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Shares_ReadUserShares", param);

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
                            CountryName = Convert.ToString(dr["CountryName"]) ?? "",
                            Cca3 = Convert.ToString(dr["cca3"]) ?? ""
                        };
                        shares.Add(share);
                    }
                    return shares;
                }

            }
            catch (Exception ex)
            {
                Server.Logging.AppLogger.LogException(ex);
                throw;
            }

            finally
            {
                if (con != null) con.Close();
            }
        }

        public List<Share> GetCountryShares(string countryName)
        {
            SqlConnection con = null;

            try
            {
                con = Connect();
                var param = new Dictionary<string, object>
            {
                { "@CountryName", countryName }
            };
                SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_SP_Shares_ReadCountryShares", param);

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
                            Cca3 = Convert.ToString(dr["Cca3"]) ?? "",
                        };
                        shares.Add(share);
                    }
                    return shares;
                }

            }
            catch (Exception ex)
            {
                Server.Logging.AppLogger.LogException(ex);
                throw;
            }

            finally
            {
                if (con != null) con.Close();
            }
        }


        public bool CreateShare(Share share)
        {
            SqlConnection con = null;

            try
            {
                con = Connect();

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

                cmd.ExecuteNonQuery();

                int result = Convert.ToInt32(returnParameter.Value);

                return result == 1;

            }
            catch (Exception ex)
            {
                Server.Logging.AppLogger.LogException(ex);
                throw;
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
            SqlConnection con = null;

            try
            {
                con = Connect();

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

                int affectedRows = Convert.ToInt32(cmd.ExecuteScalar());
                return affectedRows > 0;

            }
            catch (Exception ex)
            {
                Server.Logging.AppLogger.LogException(ex);
                throw;
            }

            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        public bool DeleteShare(int ShareID, int UserID)
        {
            SqlConnection con = null;

            try
            {
                con = Connect();

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

                cmd.ExecuteNonQuery();

                int result = Convert.ToInt32(returnParameter.Value);

                return result == 1;

            }
            catch (Exception ex)
            {
                Server.Logging.AppLogger.LogException(ex);
                throw;
            }

            finally
            {
                if (con != null)
                    con.Close();
            }
        }
    }
}

