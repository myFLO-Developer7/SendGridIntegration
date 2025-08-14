using System.Data;
using Microsoft.Data.SqlClient;

namespace SendGridIntegration
{
    internal class SqlServer
    {
        private static DbSettings _dbSettings = new DbSettings();
        public string _connectionString = "";
        public SqlServer() { }
        public SqlServer(DbSettings dbSettings)
        {
            _dbSettings = dbSettings;
            string dbServer = _dbSettings.SqlServer;
            string dbName = _dbSettings.Database;
            string userName = _dbSettings.UserName;
            string password = _dbSettings.Password;
            _connectionString = $"data source={dbServer}; Initial Catalog={dbName}; uid={userName}; pwd={password};Encrypt=True;TrustServerCertificate=True;";
        }

        public DbSettings GetDbSettingsFromWrapper(string systemId, string wrapperDatabase)
        {
            string connectionString = wrapperDatabase;
            string sqlString = "select * from dMyFloSystems where sysSystemId = @systemId";
            DataTable dt = new DataTable();

            // CONNECTION TO WRAPPER
            SqlConnection con = new SqlConnection(connectionString);

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@systemId", systemId);
            cmd.Connection = con;
            cmd.CommandText = sqlString;

            con.Open();
            dt.Load(cmd.ExecuteReader());
            con.Close();

            DbSettings dbSettings = new DbSettings();
            if (dt.Rows.Count > 0)
            {
                dbSettings.SqlServer = dt.Rows[0]["syssqlserver"]?.ToString()?.Trim() ?? "";
                dbSettings.Database = dt.Rows[0]["sysdatabasename"]?.ToString()?.Trim() ?? "";
                dbSettings.UserName = dt.Rows[0]["syssqlusername"]?.ToString()?.Trim() ?? "";
                dbSettings.Password = dt.Rows[0]["syssqlpassword"]?.ToString()?.Trim() ?? "";
                dbSettings.PaymentGatewaySqlServer = dt.Rows[0]["syspaymentgatewaysqlserver"]?.ToString()?.Trim() ?? "";
                dbSettings.PaymentGatewayDbName = dt.Rows[0]["syspaymentgatewaydatabasename"]?.ToString()?.Trim() ?? "";
            }
            if (dbSettings.SqlServer == null || dbSettings.Database == null)
            {
                Environment.Exit(1); // Exit with error code
            }
            return dbSettings;
        }

        public async Task<DataTable> GetData(string sqlQuery, List<SqlParameter> sqlParameters)
        {
            string dbServer = _dbSettings.SqlServer;
            string dbName = _dbSettings.Database;
            string userName = _dbSettings.UserName;
            string password = _dbSettings.Password;
            string connectionString = $"data source={dbServer}; Initial Catalog={dbName}; uid={userName}; pwd={password};Encrypt=True;TrustServerCertificate=True;";

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                    {

                        if (sqlParameters != null && sqlParameters.Count > 0)
                            cmd.Parameters.AddRange(sqlParameters.ToArray());
                        await con.OpenAsync();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return dt;
        }

        public string UpdateOrInsertData(string sqlQuery, List<SqlParameter> parameters)
        {
            string connectionString = "data source=" + _dbSettings.SqlServer +
                          "; Initial Catalog=" + _dbSettings.Database +
                          "; uid=" + _dbSettings.UserName +
                          "; pwd=" + _dbSettings.Password +
                          "; Max Pool Size=200; Encrypt=True; TrustServerCertificate=True;";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sqlQuery;

                    command.CommandType = CommandType.Text;

                    foreach (SqlParameter param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> ExecuteNonQueryAsync(string systemid, string sqltext, List<SqlParameter> parameters)
        {
            string retval;

            string connectionString = "data source=" + _dbSettings.SqlServer +
                                          "; Initial Catalog=" + _dbSettings.Database +
                                          "; uid=" + _dbSettings.UserName +
                                          "; pwd=" + _dbSettings.Password +
                                          "; Max Pool Size=200; Encrypt=True; TrustServerCertificate=True;";
            try
            {
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand(sqltext, connection);

                command.Parameters.AddRange(parameters.ToArray());
                connection.Open();
                int result = await command.ExecuteNonQueryAsync();
                retval = $"{result} rows affected.";
            }
            catch (Exception ex)
            {
                throw;
            }

            return retval;
        }
    }
}
