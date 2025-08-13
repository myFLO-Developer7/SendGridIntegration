
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SendGridIntegration;
using System.Data;

class Program
{
    static async void Main(string[] args)
    {
        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // so it looks in the executable's folder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            string? dbServer = config["ConnectionStrings:WrapperDatabase"];
            string? systemId = config["ConnectionStrings:System_ID"];
            if (string.IsNullOrEmpty(dbServer) || string.IsNullOrEmpty(systemId))
            {
                throw new Exception("Missing WrapperDatabase connection string or System ID in appsettings.json");
            }
            if (args.Length > 0)
            {
                SqlServer wrapperDatabase = new SqlServer();
                DbSettings dbSettings = wrapperDatabase.GetDbSettingsFromWrapper(systemId, dbServer);
                SqlServer database = new SqlServer(dbSettings);
                var command = args[0];
                if (command == "Test")
                {
                    DataTable dt = await database.GetData("SELECT * FROM dRegisteredUsers WHERE regUserID = 194", new List<SqlParameter>() { });
                    Console.WriteLine(dt.Rows[0].ToString());
                }
                else
                {
                    throw new Exception($"No command such as {command}");
                }
            }
            else
            {
                throw new Exception($"No command input");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}