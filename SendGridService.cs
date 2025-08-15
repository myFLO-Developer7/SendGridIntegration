using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Data.SqlClient;

namespace SendGridIntegration
{
    internal class SendGridService
    {
        private readonly string sendgridAPIKey;
        public SendGridService(string _apiKey)
        {
            sendgridAPIKey = _apiKey;
        }

        public async Task Send()
        {
            try
            {
                var client = new SendGridClient(sendgridAPIKey);
                var from = new EmailAddress("Bradfordenergy@appmail.csr.com.au", "Bradfordenergy");
                var subject = "[TEST] Email from SendGrid";
                var to = new EmailAddress("kennethbalane.main@gmail.com", "Kenneth Balane");
                var plainTextContent = "HELLO, TEST EMAIL FROM MYFLO";
                var htmlContent = "<strong>HELLO, TEST EMAIL FROM MYFLO</strong>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                Console.WriteLine($"Email sent with status code: {response.StatusCode}");
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.Message );
            }
            
        }

        public async Task SendBatch(SqlServer emailServer, string groupCode, string attachmentFolder, string smfAutoBatchEmailStartDate, int smfAutobatchEmailMaxAttempt, string fromEmail)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>()
                {
                    new SqlParameter("@smfAutoBatchEmailStartDate", System.Data.SqlDbType.DateTime) { Value = smfAutoBatchEmailStartDate},
                    new SqlParameter("@smfAutobatchEmailMaxAttempt", System.Data.SqlDbType.Int) { Value = smfAutobatchEmailMaxAttempt},
                    new SqlParameter("@groupCode", System.Data.SqlDbType.Char) { Value = groupCode}
                };
                DataTable emails = await emailServer.GetData($"SELECT * FROM dEmails LEFT JOIN dEmailsLinked on emkEmailID = emgMessageID and emkType = 'JOB' WHERE emgFunction = 'S' and emgSentOK = 0 and emgAddedDate >= '2023-08-13T00:00:00' and emgSendAttemptCount < @smfAutobatchEmailMaxAttempt and emgSystem = @groupCode", parameters);
                //Console.WriteLine($"SELECT * FROM dEmails LEFT JOIN dEmailsLinked on emkEmailID = emgMessageID and emkType = 'JOB' WHERE emgFunction = 'S' and emgSentOK = 0 and emgAddedDate >= '{smfAutoBatchEmailStartDate}' and emgSendAttemptCount < {smfAutobatchEmailMaxAttempt} and emgSystem = '{groupCode}')");
                if (emails.Rows.Count > 0)
                {
                    foreach(DataRow email in emails.Rows)
                    {
                        List<object> attachments = new List<object>();
                        List<SqlParameter> attachmentParameters = new List<SqlParameter>()
                        {
                            new SqlParameter("@emgMessageID", System.Data.SqlDbType.Char) { Value = email["emgMessageID"].ToString() }
                        };
                        DataTable emailAttachments = await emailServer.GetData("SELECT RTRIM(emaAttachmentFilePath) as emaAttachmentFilePath FROM dEmailsAttachments WHERE emaEmailID = @emgMessageID", attachmentParameters);
                        foreach(DataRow attachment in emailAttachments.Rows)
                        {
                            attachments.Add($"{attachmentFolder}{attachment["emaAttachmentFilePath"]}");
                        }
                        Console.WriteLine($"{email["emgMessageID"].ToString().TrimEnd()} - {email["emgTo"].ToString().TrimEnd()} - {email["emgSubject"].ToString().TrimEnd()}");
                        Console.WriteLine("Attachments: " + string.Join(", ", attachments));
                    }
                }
                else
                {
                    throw new Exception("No Emails to Send.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public async Task Send(string _subject, string _to, string _htmlContent, string _plainTextContent)
        {
            try
            {
                var client = new SendGridClient(sendgridAPIKey);
                var from = new EmailAddress("Bradfordenergy@appmail.csr.com.au", "Bradfordenergy");
                var subject = _subject;
                var to = new EmailAddress(_to);
                var plainTextContent = _plainTextContent;
                var htmlContent = _htmlContent;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                Console.WriteLine($"Email sent with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
