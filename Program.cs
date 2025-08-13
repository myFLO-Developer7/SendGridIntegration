
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SendGridIntegration;
using System.Data;

class Program
{
    static async Task Main(string[] args)
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
                // CONNECTED TO WRAPPER!!
                var command = args[0];
                if (command == "SendEmail")
                {
                    DataTable dtCompanies = await database.GetData("SELECT * FROM dCompanies WHERE smfAutobatchEmails = 1", new List<SqlParameter>() { });
                    bool alreadyGrab = false;

                    //Create AutoBatch History
                    //dateTimeBatch = dt.datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')[:-3];
                    //dAutoBatchHistory(connectionString1, dateTimeBatch, 'START')

                    foreach(DataRow row in dtCompanies.Rows) 
                    {
                        if (!String.IsNullOrEmpty(row["smfEmailEWSConnectionString"].ToString()) && (!alreadyGrab))
                        {
                            Console.WriteLine($"Department: {row["smfCode"]} - {row["smfCompanyName"]} - {row["smfEmailEWSConnectionString"].ToString()}");
                        }
                        Console.WriteLine($"Department: {row["smfCode"]} - {row["smfCompanyName"]}");
                    }
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