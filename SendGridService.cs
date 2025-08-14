using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

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
    }
}
