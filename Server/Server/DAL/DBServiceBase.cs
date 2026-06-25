using System.Data.SqlClient;

namespace GamesServerSide.DAL
{
    public abstract class DBServiceBase
    {
        protected static SqlConnection? con;
        private const string ConString= "myProjDB";
        protected static void Connect()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            string cStr = configuration.GetConnectionString(ConString);
            if (string.IsNullOrWhiteSpace(cStr))
            {
                throw new InvalidOperationException($"Connection string '{ConString}' not found in configuration.");
            }

            con = new SqlConnection(cStr);
            con.Open();
        }

        protected static SqlCommand CreateCommandWithStoredProcedureGeneral(String spName, Dictionary<string, object> paramDic)
        {

            SqlCommand cmd = new SqlCommand
            {
                Connection = con,
                CommandText = spName,
                CommandTimeout = 10,
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