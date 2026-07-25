using System.Data.SqlClient;

namespace Server.DAL
{
    public abstract class DBServiceBase
    {

        private const string ConString= "myProjDB";

        // The connection string never changes at runtime, so read appsettings.json once
        // and cache it instead of rebuilding the configuration on every DB call.
        private static readonly Lazy<string> _connectionString = new(() =>
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            string cStr = configuration.GetConnectionString(ConString);
            if (string.IsNullOrWhiteSpace(cStr))
            {
                throw new InvalidOperationException($"Connection string '{ConString}' not found in configuration.");
            }
            return cStr;
        });

        protected SqlConnection Connect()
        {
            var con = new SqlConnection(_connectionString.Value);
            con.Open();
            return con;
        }

        protected static SqlCommand CreateCommandWithStoredProcedureGeneral(SqlConnection con, String spName, Dictionary<string, object> paramDic)
        {

            SqlCommand cmd = new SqlCommand
            {
                Connection = con,
                CommandText = spName,
                CommandTimeout = 30,
                CommandType = System.Data.CommandType.StoredProcedure
            };
            if (paramDic != null)
                foreach (KeyValuePair<string, object> param in paramDic)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }
            return cmd;
        }
    }
}

