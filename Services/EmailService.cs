using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CompanyNotificationApp.Models;

namespace CompanyNotificationApp.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;
        private readonly bool _enableSsl;

        public EmailService(string smtpServer = "smtp.gmail.com", int smtpPort = 587, 
            string senderEmail = "your-email@gmail.com", string senderPassword = "your-password",
            bool enableSsl = true)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _senderEmail = senderEmail;
            _senderPassword = senderPassword;
            _enableSsl = enableSsl;
        }

        public async Task SendNotificationAsync(Company company, NotificationTask task)
        {
            if (string.IsNullOrEmpty(company.Email))
                return;

            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = _enableSsl;
                    client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

                    var message = new MailMessage
                    {
                        From = new MailAddress(_senderEmail, "Company Notification App"),
                        Subject = task.Description,
                        Body = GenerateEmailBody(company, task),
                        IsBodyHtml = true
                    };

                    message.To.Add(company.Email);
                    await client.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri posielaní emailu: {ex.Message}");
            }
        }

        private string GenerateEmailBody(Company company, NotificationTask task)
        {
            return $@"
                <h2>Upozornenie na povinnosť</h2>
                <p>Ahoj {company.Name},</p>
                <p>Upozorňujeme vás na nasledujúcu povinnos:</p>
                <p><strong>{task.Description}</strong></p>
                <p><strong>Termín do:</strong> {task.DueDate:dd.MM.yyyy}</p>
                <p>Ďakujeme,<br/>Company Notification App</p>
            ";
        }
    }
}
