using Project.Services.Interfaces;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;  // تأكد من إضافة هذه المكتبة

namespace Project.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // قراءة الإعدادات من ملف appsettings.json
                var smtpServer = _config["EmailSettings:SmtpServer"];
                var port = int.Parse(_config["EmailSettings:Port"]);
                var email = _config["EmailSettings:Email"];
                var appPassword = _config["EmailSettings:AppPassword"];

                var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = port,
                    Credentials = new NetworkCredential(email, appPassword),
                    EnableSsl = true,
                };

                var message = new MailMessage
                {
                    From = new MailAddress(email, "Virual E-Commerce"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                message.To.Add(toEmail);

                // إرسال البريد الإلكتروني بشكل غير متزامن
                await smtpClient.SendMailAsync(message);
            }
            catch (SmtpException smtpEx)
            {
                // التعامل مع أخطاء SMTP بشكل منفصل
                Console.WriteLine($"SMTP Error: {smtpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // التعامل مع الأخطاء العامة
                Console.WriteLine($"General Error: {ex.Message}");
                throw;
            }
        }
    }
}


