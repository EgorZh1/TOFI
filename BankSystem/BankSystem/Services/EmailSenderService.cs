

using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;
using BankSystem.Data;
using System.Configuration;

namespace BankSystem.Services
{
    public class EmailSenderService : IEmailSender
    {
        private IConfiguration _configuration;


        public EmailSenderService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            MailAddress from = new MailAddress("egorzhdanko2@gmail.com", "2FA");
            // кому отправляем
            MailAddress to = new MailAddress(email);
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);
            // тема письма
            m.Subject = subject;
            // текст письма - включаем в него ссылку
            m.Body = message;
            m.IsBodyHtml = true;
            // адрес smtp-сервера, с которого мы и будем отправлять письмо
            SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
            smtp.EnableSsl = true;
            // логин и пароль
            smtp.Credentials = new System.Net.NetworkCredential("egorzhdanko2@gmail.com", _configuration.GetSection("EmailSendPassword")["SendPass"]);
            smtp.Send(m);

            return Task.FromResult(0);
        }
    }
}
