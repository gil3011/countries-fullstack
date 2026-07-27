using Server.BL;
using System.Data;
using System.Data.SqlClient;

namespace Server.DAL
{
    public class DBServiceCountry : DBServiceBase
    {
        public List<Country> ReadAllCountries()
        {
            SqlConnection con = null;
            SqlCommand cmd;

            try
            {
                con = Connect();

                cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Countries2026_ReadAll", null);
                List<Country> countries = new();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    int capOrdinal = dr.GetOrdinal("CapitalName");
                    int langOrdinal = dr.GetOrdinal("LanguageNames");
                    int currOrdinal = dr.GetOrdinal("CurrencyNames");
                    while (dr.Read())
                    {
                        Country c = MapCountryFromReader(dr);

                        if (!dr.IsDBNull(capOrdinal))
                            c.Capitals.Add(new Capital { Name = dr.GetString(capOrdinal) });

                        if (!dr.IsDBNull(langOrdinal))
                            foreach (var langName in dr.GetString(langOrdinal).Split('|', StringSplitOptions.RemoveEmptyEntries))
                                c.Languages.Add(new Language { LanguageName = langName });

                        if (!dr.IsDBNull(currOrdinal))
                            foreach (var currName in dr.GetString(currOrdinal).Split('|', StringSplitOptions.RemoveEmptyEntries))
                                c.Currencies.Add(new Currency { CurrencyName = currName });

                        countries.Add(c);
                    }
                }

                return countries;

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

        public Country GetCountriesByCca3(string cca3)
        {
            SqlConnection con = null;
            SqlCommand cmd;

            try
            {
                con = Connect();

                var param = new Dictionary<string, object>()
            {
                { "@Cca3", cca3 ?? string.Empty }
            };
                cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Countries2026_GetByCca3", param);

                Country c = null;
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        c = MapCountryFromReader(dr);
                    }
                }
                if (c == null)
                {
                    return null;
                }
                LoadChildCollections(c.Id, c);
                return c;

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

        public bool InsertCountry(Country country)
        {
            SqlConnection con = null;
            SqlTransaction tx = null;
            try
            {
                con = Connect();

                tx = con.BeginTransaction();

                var countryParams = new Dictionary<string, object>
                {
                    { "@Cca3", country.Cca3 ?? string.Empty },
                    { "@CommonName", country.CommonName ?? string.Empty },
                    { "@OfficialName", country.OfficialName ?? string.Empty },
                    { "@Region", country.Region },
                    { "@Subregion", string.IsNullOrEmpty(country.Subregion) ? (object)DBNull.Value : country.Subregion },
                    { "@Latitude", country.Latitude == 0 ? (object)DBNull.Value : country.Latitude },
                    { "@Longitude", country.Longitude == 0 ? (object)DBNull.Value : country.Longitude },
                    { "@AreaKm2", country.AreaKm2 == 0 ? (object)DBNull.Value : country.AreaKm2 },
                    { "@IsLandlocked", country.IsLandlocked },
                    { "@Population", country.Population },
                    { "@FlagUrl", string.IsNullOrEmpty(country.FlagUrl) ? (object)DBNull.Value : country.FlagUrl },
                    { "@WikipediaUrl", string.IsNullOrEmpty(country.WikipediaUrl) ? (object)DBNull.Value : country.WikipediaUrl }
                };

                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Countries2026_Insert", countryParams))
                {
                    cmd.Transaction = tx;
                    var outParam = new SqlParameter("@NewId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outParam);

                    cmd.ExecuteNonQuery();

                    int newId = (int)outParam.Value;
                    country.Id = newId;
                }

                if (country.Capitals != null && country.Capitals.Count > 0)
                {
                    foreach (var cap in country.Capitals)
                    {
                        var capParams = new Dictionary<string, object>
                        {
                            { "@CountryId", country.Id },
                            { "@Name", cap.Name ?? string.Empty },
                            { "@Latitude", cap.Latitude == 0 ? (object)DBNull.Value : cap.Latitude },
                            { "@Longitude", cap.Longitude == 0 ? (object)DBNull.Value : cap.Longitude }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Capitals2026_Insert", capParams))
                        {
                            cmd.Transaction = tx;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (country.Languages != null && country.Languages.Count > 0)
                {
                    foreach (Language lang in country.Languages)
                    {
                        int langId;
                        var langGetParams = new Dictionary<string, object>
                        {
                            { "@Iso639_1", string.IsNullOrEmpty(lang.Iso639_1) ? (object)DBNull.Value : lang.Iso639_1 },
                            { "@LanguageName", lang.LanguageName ?? string.Empty }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Languages2026_GetOrCreate", langGetParams))
                        {
                            cmd.Transaction = tx;
                            var outLang = new SqlParameter("@LanguageId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            cmd.Parameters.Add(outLang);
                            cmd.ExecuteNonQuery();
                            langId = (int)outLang.Value;
                        }

                        var langRelParams = new Dictionary<string, object>
                        {
                            { "@CountryId", country.Id },
                            { "@LanguageId", langId }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryLanguages2026_Insert", langRelParams))
                        {
                            cmd.Transaction = tx;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (country.Currencies != null && country.Currencies.Count > 0)
                {
                    foreach (Currency cur in country.Currencies)
                    {
                        int curId;
                        var curGetParams = new Dictionary<string, object>
                        {
                            { "@CurrencyCode", cur.CurrencyCode ?? string.Empty },
                            { "@CurrencyName", cur.CurrencyName ?? string.Empty },
                            { "@CurrencySymbol", string.IsNullOrEmpty(cur.CurrencySymbol) ? (object)DBNull.Value : cur.CurrencySymbol }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Currencies2026_GetOrCreate", curGetParams))
                        {
                            cmd.Transaction = tx;
                            var outCur = new SqlParameter("@CurrencyId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            cmd.Parameters.Add(outCur);
                            cmd.ExecuteNonQuery();
                            curId = (int)outCur.Value;
                        }

                        var curRelParams = new Dictionary<string, object>
                        {
                            { "@CountryId", country.Id },
                            { "@CurrencyId", curId }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryCurrencies2026_Insert", curRelParams))
                        {
                            cmd.Transaction = tx;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (country.Borders != null && country.Borders.Count > 0)
                {
                    foreach (Country border in country.Borders)
                    {
                        string borderCca3 = border.Cca3 ?? string.Empty;
                        var borderParams = new Dictionary<string, object>
                        {
                            { "@CountryId", country.Id },
                            { "@BorderCca3", borderCca3 }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryBorders2026_InsertByBorderCca3", borderParams))
                        {
                            cmd.Transaction = tx;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (country.Timezones != null && country.Timezones.Count > 0)
                {
                    foreach (string tz in country.Timezones)
                    {
                        var tzParams = new Dictionary<string, object>
                        {
                            { "@CountryId", country.Id },
                            { "@Timezone", tz ?? string.Empty }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryTimezones2026_Insert", tzParams))
                        {
                            cmd.Transaction = tx;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                tx.Commit();
                return true;

            }
            catch (Exception ex)
            {
                Server.Logging.AppLogger.LogException(ex);
                try { tx.Rollback(); } catch { }
                return false;
            }

            finally
            {
                if (con != null) con.Close();
            }
        }

        public bool UpdateCountry(int id, Country country)
        {
            SqlConnection con = null;
            SqlTransaction tx = null;
            try
            {
                con = Connect();

                tx = con.BeginTransaction();

                var updateParams = new Dictionary<string, object>
                {
                    { "@Id", id },
                    { "@Cca3", country.Cca3 ?? string.Empty },
                    { "@CommonName", country.CommonName ?? string.Empty },
                    { "@OfficialName", country.OfficialName ?? string.Empty },
                    { "@Region", country.Region },
                    { "@Subregion", string.IsNullOrEmpty(country.Subregion) ? (object)DBNull.Value : country.Subregion },
                    { "@Latitude", country.Latitude == 0 ? (object)DBNull.Value : country.Latitude },
                    { "@Longitude", country.Longitude == 0 ? (object)DBNull.Value : country.Longitude },
                    { "@AreaKm2", country.AreaKm2 == 0 ? (object)DBNull.Value : country.AreaKm2 },
                    { "@IsLandlocked", country.IsLandlocked },
                    { "@Population", country.Population },
                    { "@FlagUrl", string.IsNullOrEmpty(country.FlagUrl) ? (object)DBNull.Value : country.FlagUrl },
                    { "@WikipediaUrl", string.IsNullOrEmpty(country.WikipediaUrl) ? (object)DBNull.Value : country.WikipediaUrl }
                };

                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Countries2026_Update", updateParams))
                {
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                }

                var deletes = new[] {
                    "FP_sp_Capitals2026_DeleteByCountryId",
                    "FP_sp_CountryLanguages2026_DeleteByCountryId",
                    "FP_sp_CountryCurrencies2026_DeleteByCountryId",
                    "FP_sp_CountryBorders2026_DeleteByCountryId",
                    "FP_sp_CountryTimezones2026_DeleteByCountryId"
                };
                var deleteParams = new Dictionary<string, object> { { "@CountryId", id } };
                foreach (var sp in deletes)
                {
                    using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, sp, deleteParams))
                    {
                        cmd.Transaction = tx;
                        cmd.ExecuteNonQuery();
                    }
                }

                country.Id = id;

                if (country.Capitals != null)
                {
                    foreach (var cap in country.Capitals)
                    {
                        var capParams = new Dictionary<string, object>
                        {
                            { "@CountryId", country.Id },
                            { "@Name", cap.Name ?? string.Empty },
                            { "@Latitude", cap.Latitude == 0 ? (object)DBNull.Value : cap.Latitude },
                            { "@Longitude", cap.Longitude == 0 ? (object)DBNull.Value : cap.Longitude }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Capitals2026_Insert", capParams))
                        {
                            cmd.Transaction = tx;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (country.Languages != null)
                {
                    foreach (var lang in country.Languages)
                    {
                        int langId;
                        var langGetParams = new Dictionary<string, object>
                        {
                            { "@Iso639_1", string.IsNullOrEmpty(lang.Iso639_1) ? (object)DBNull.Value : lang.Iso639_1 },
                            { "@LanguageName", lang.LanguageName ?? string.Empty }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Languages2026_GetOrCreate", langGetParams))
                        {
                            cmd.Transaction = tx;
                            var outLang = new SqlParameter("@LanguageId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            cmd.Parameters.Add(outLang);
                            cmd.ExecuteNonQuery();
                            langId = (int)outLang.Value;
                        }

                        var langRelParams = new Dictionary<string, object>
                        {
                            { "@CountryId", country.Id },
                            { "@LanguageId", langId }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryLanguages2026_Insert", langRelParams))
                        {
                            cmd.Transaction = tx;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (country.Currencies != null)
                {
                    foreach (var cur in country.Currencies)
                    {
                        int curId;
                        var curGetParams = new Dictionary<string, object>
                        {
                            { "@CurrencyCode", cur.CurrencyCode ?? string.Empty },
                            { "@CurrencyName", cur.CurrencyName ?? string.Empty },
                            { "@CurrencySymbol", string.IsNullOrEmpty(cur.CurrencySymbol) ? (object)DBNull.Value : cur.CurrencySymbol }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Currencies2026_GetOrCreate", curGetParams))
                        {
                            cmd.Transaction = tx;
                            var outCur = new SqlParameter("@CurrencyId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            cmd.Parameters.Add(outCur);
                            cmd.ExecuteNonQuery();
                            curId = (int)outCur.Value;
                        }

                        var curRelParams = new Dictionary<string, object>
                        {
                            { "@CountryId", country.Id },
                            { "@CurrencyId", curId }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryCurrencies2026_Insert", curRelParams))
                        {
                            cmd.Transaction = tx;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (country.Borders != null)
                {
                    foreach (var border in country.Borders)
                    {
                        var borderParams = new Dictionary<string, object>
                        {
                            { "@CountryId", country.Id },
                            { "@BorderCca3", border.Cca3 ?? string.Empty }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryBorders2026_InsertByBorderCca3", borderParams))
                        {
                            cmd.Transaction = tx;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (country.Timezones != null)
                {
                    foreach (var tz in country.Timezones)
                    {
                        var tzParams = new Dictionary<string, object>
                        {
                            { "@CountryId", country.Id },
                            { "@Timezone", tz ?? string.Empty }
                        };

                        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryTimezones2026_Insert", tzParams))
                        {
                            cmd.Transaction = tx;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                tx.Commit();
                return true;

            }
            catch (Exception ex)
            {
                Server.Logging.AppLogger.LogException(ex);
                try { tx.Rollback(); } catch { }
                return false;
            }

            finally
            {
                if (con != null) con.Close();
            }
        }

        public bool DeleteCountry(int id)
        {
            SqlConnection con = null;
            SqlCommand cmd;

            try
            {
                con = Connect();

                cmd = CreateCommandWithStoredProcedureGeneral(
                    con,
                    "FP_sp_Countries2026_Delete",
                    new Dictionary<string, object>() { { "@Id", id } }
                );

                var returnParameter = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;

                cmd.ExecuteNonQuery();

                int deleted = (int)returnParameter.Value;
                return deleted > 0;


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

        public List<string> ReadAllLanguages()
        {
            SqlConnection con = null;
            SqlCommand cmd;

            try
            {
                con = Connect();

                cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Langueges2026_ReadAll", null);
                List<string> langueges = new();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string l = dr.GetString(dr.GetOrdinal("LanguageName")).Trim();
                        langueges.Add(l);
                    }
                }

                return langueges;

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

        internal static Country MapCountryFromReader(SqlDataReader dr)
        {
            Country c = new();
            c.Id = dr.IsDBNull(dr.GetOrdinal("Id")) ? 0 : dr.GetInt32(dr.GetOrdinal("Id"));
            c.Cca3 = dr.IsDBNull(dr.GetOrdinal("Cca3")) ? string.Empty : dr.GetString(dr.GetOrdinal("Cca3")).Trim();
            c.CommonName = dr.IsDBNull(dr.GetOrdinal("CommonName")) ? string.Empty : dr.GetString(dr.GetOrdinal("CommonName"));
            c.OfficialName = dr.IsDBNull(dr.GetOrdinal("OfficialName")) ? string.Empty : dr.GetString(dr.GetOrdinal("OfficialName"));
            c.Region = dr.IsDBNull(dr.GetOrdinal("Region")) ? string.Empty : dr.GetString(dr.GetOrdinal("Region"));
            c.Subregion = dr.IsDBNull(dr.GetOrdinal("Subregion")) ? string.Empty : dr.GetString(dr.GetOrdinal("Subregion"));

            if (!dr.IsDBNull(dr.GetOrdinal("Latitude"))) c.Latitude = Convert.ToDouble(dr["Latitude"]);
            if (!dr.IsDBNull(dr.GetOrdinal("Longitude"))) c.Longitude = Convert.ToDouble(dr["Longitude"]);
            if (!dr.IsDBNull(dr.GetOrdinal("AreaKm2"))) c.AreaKm2 = Convert.ToDouble(dr["AreaKm2"]);
            if (!dr.IsDBNull(dr.GetOrdinal("IsLandlocked"))) c.IsLandlocked = Convert.ToBoolean(dr["IsLandlocked"]);
            if (!dr.IsDBNull(dr.GetOrdinal("Population"))) c.Population = Convert.ToInt32(dr["Population"]);

            c.FlagUrl = dr.IsDBNull(dr.GetOrdinal("FlagUrl")) ? string.Empty : dr.GetString(dr.GetOrdinal("FlagUrl"));
            c.WikipediaUrl = dr.IsDBNull(dr.GetOrdinal("WikipediaUrl")) ? string.Empty : dr.GetString(dr.GetOrdinal("WikipediaUrl"));

            return c;
        }

        internal void LoadChildCollections(int countryId, Country c)
        {
            SqlConnection con = null;
            try
            {
                con = Connect();
                var childParams = new Dictionary<string, object> { { "@CountryId", countryId } };

                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_Capitals2026_GetByCountryId", childParams))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        c.Capitals = new List<Capital>();
                        while (dr.Read())
                        {
                            Capital cap = new()
                            {
                                Id = dr.IsDBNull(dr.GetOrdinal("Id")) ? 0 : dr.GetInt32(dr.GetOrdinal("Id")),
                                Name = dr.IsDBNull(dr.GetOrdinal("Name")) ? string.Empty : dr.GetString(dr.GetOrdinal("Name")),
                                Latitude = dr.IsDBNull(dr.GetOrdinal("Latitude")) ? 0 : Convert.ToDouble(dr["Latitude"]),
                                Longitude = dr.IsDBNull(dr.GetOrdinal("Longitude")) ? 0 : Convert.ToDouble(dr["Longitude"])
                            };
                            c.Capitals.Add(cap);
                        }
                    }
                }

                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryLanguages2026_GetByCountryId", childParams))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        c.Languages = new List<Language>();
                        while (dr.Read())
                        {
                            Language l = new()
                            {
                                Id = dr.IsDBNull(dr.GetOrdinal("LanguageId")) ? 0 : dr.GetInt32(dr.GetOrdinal("LanguageId")),
                                Iso639_1 = dr.IsDBNull(dr.GetOrdinal("Iso639_1")) ? string.Empty : dr.GetString(dr.GetOrdinal("Iso639_1")).Trim(),
                                LanguageName = dr.IsDBNull(dr.GetOrdinal("LanguageName")) ? string.Empty : dr.GetString(dr.GetOrdinal("LanguageName"))
                            };
                            c.Languages.Add(l);
                        }
                    }
                }

                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryCurrencies2026_GetByCountryId", childParams))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        c.Currencies = new List<Currency>();
                        while (dr.Read())
                        {
                            Currency cur = new()
                            {
                                Id = dr.IsDBNull(dr.GetOrdinal("CurrencyId")) ? 0 : dr.GetInt32(dr.GetOrdinal("CurrencyId")),
                                CurrencyCode = dr.IsDBNull(dr.GetOrdinal("CurrencyCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("CurrencyCode")).Trim(),
                                CurrencyName = dr.IsDBNull(dr.GetOrdinal("CurrencyName")) ? string.Empty : dr.GetString(dr.GetOrdinal("CurrencyName")),
                                CurrencySymbol = dr.IsDBNull(dr.GetOrdinal("CurrencySymbol")) ? string.Empty : dr.GetString(dr.GetOrdinal("CurrencySymbol"))
                            };
                            c.Currencies.Add(cur);
                        }
                    }
                }

                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryBorders2026_GetByCountryId", childParams))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        c.Borders = new List<Country>();
                        while (dr.Read())
                        {
                            Country border = new()
                            {
                                Id = dr.IsDBNull(dr.GetOrdinal("BorderCountryId")) ? 0 : dr.GetInt32(dr.GetOrdinal("BorderCountryId")),
                                Cca3 = dr.IsDBNull(dr.GetOrdinal("Cca3")) ? string.Empty : dr.GetString(dr.GetOrdinal("Cca3")).Trim(),
                                CommonName = dr.IsDBNull(dr.GetOrdinal("CommonName")) ? string.Empty : dr.GetString(dr.GetOrdinal("CommonName"))
                            };
                            c.Borders.Add(border);
                        }
                    }
                }

                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(con, "FP_sp_CountryTimezones2026_GetByCountryId", childParams))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        c.Timezones = new List<string>();
                        while (dr.Read())
                        {
                            string tz = dr.IsDBNull(dr.GetOrdinal("Timezone")) ? string.Empty : dr.GetString(dr.GetOrdinal("Timezone"));
                            c.Timezones.Add(tz);
                        }
                    }
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
    }
}

