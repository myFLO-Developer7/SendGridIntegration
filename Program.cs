
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SendGridIntegration;
using System.Data;
using System.Runtime.CompilerServices;

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
            string? sendgridAPIKey = config["SendGrid:SecretKey"];
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

                    foreach(DataRow smf in dtCompanies.Rows) 
                    {
                        if (!String.IsNullOrEmpty(smf["smfEmailEWSConnectionString"].ToString()) && (!alreadyGrab))
                        {
                            string emailDBConn = smf["smfEmailEWSConnectionString"]?.ToString()?.TrimEnd() ?? "";
                            string logFileDir = smf["smfEmailLogFile"]?.ToString()?.Trim() ?? "";

                            // Split connection string into parts
                            string[] emailDBConData = emailDBConn.Split(';', StringSplitOptions.RemoveEmptyEntries);

                            string serverName = "";
                            string db = "";
                            string username = "";
                            string pw = "";

                            foreach (string strConData in emailDBConData)
                            {
                                if (strConData.Contains("Data Source", StringComparison.OrdinalIgnoreCase))
                                    serverName = strConData.Split('=')[1].Trim();

                                if (strConData.Contains("Catalog", StringComparison.OrdinalIgnoreCase))
                                    db = strConData.Split('=')[1].Trim();

                                if (strConData.Contains("Id", StringComparison.OrdinalIgnoreCase))
                                    username = strConData.Split('=')[1].Trim();

                                if (strConData.Contains("Password", StringComparison.OrdinalIgnoreCase))
                                    pw = strConData.Split('=')[1].Trim();
                            }
                            DbSettings companiesDbSettings = new DbSettings();
                            companiesDbSettings.Database = db;
                            companiesDbSettings.UserName = username;
                            companiesDbSettings.Password = pw;
                            companiesDbSettings.SqlServer = serverName;
                            SqlServer companiesDatabase = new SqlServer(companiesDbSettings);
                            Console.WriteLine(companiesDatabase._connectionString);
                            DataTable dEmailSystems = await companiesDatabase.GetData("SELECT * FROM dEmailSystems WHERE emcEWSVersion = 'SENDGRID'", new List<SqlParameter>() { });
                            foreach (DataRow ems in dEmailSystems.Rows)
                            {
                                string? groupCode = ems["emcGroupCode"]?.ToString()?.TrimEnd();
                                string? defaultFromName = ems["emcDefaultFromName"]?.ToString()?.TrimEnd();
                                string? smfAutoBatchEmailStartDate = smf["smfAutoBatchEmailStartDate"]?.ToString()?.TrimEnd();
                                int smfAutobatchEmailMaxAttempt = (int)smf["smfAutobatchEmailMaxAttempt"];
                                string? attachment_foler = ems["emcAttachmentFolder"]?.ToString()?.TrimEnd();

                                SendGridService sendGridService = new SendGridService(sendgridAPIKey);
                                sendGridService.SendBatch(companiesDatabase, groupCode, attachment_foler, smfAutoBatchEmailStartDate, smfAutobatchEmailMaxAttempt, defaultFromName);
                            }
                        }
                        alreadyGrab = true;
                    }
                }
                else if(command == "SendTest")
                {
                    SendGridService sendGridService = new SendGridService(sendgridAPIKey);
                    Console.WriteLine("SENDING");
                    await sendGridService.Send();
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