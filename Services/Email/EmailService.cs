
using RestSharp;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace UsersApp.Services.Email
{
    public class EmailService : IEmailService
    {

        private readonly string _apiKey;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public EmailService(IConfiguration configuration)
        {
            _apiKey = configuration["Brevo:ApiKey"];
            _senderEmail = configuration["Brevo:SenderEmail"];
            _senderName = configuration["Brevo:SenderName"];
        }


        public async Task SendEmailAsync(string recipientEmail, string subject, string content)
        {
            var client = new RestClient("https://api.brevo.com/v3/smtp/email");
            var request = new RestRequest("/", Method.Post);

            // إعداد الهيدر
            request.AddHeader("accept", "application/json");
            request.AddHeader("api-key", _apiKey);
            request.AddHeader("content-type", "application/json");

            // إعداد الجسم
            var body = new
            {
                sender = new { email = _senderEmail, name = _senderName },
                to = new[] { new { email = recipientEmail } },
                subject,
                htmlContent = content
            };

            request.AddJsonBody(body);

            try
            {
                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    Console.WriteLine($"Error sending email: {response.Content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");

            }
        }
    }
}



