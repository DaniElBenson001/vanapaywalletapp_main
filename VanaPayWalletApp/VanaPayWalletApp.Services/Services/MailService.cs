using System.Net;
using System.Net.Mail.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using MailKit;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.Services.IServices;
using VanaPayWalletApp.Models.Models.DtoModels;
using Microsoft.Extensions.Logging;
using SmtpClient = System.Net.Mail.SmtpClient;

namespace VanaPayWalletApp.Services.Services
{
    public class MailService : IMailService
    {
        private readonly MailConfiguration _settings;
        private readonly IWebHostEnvironment _hostEnviroment;
        private readonly ILogger _logger;

        public MailService(MailConfiguration settings, IWebHostEnvironment hostEnvironment, ILogger logger)
        {
            _hostEnviroment = hostEnvironment;
            _settings = settings;
            _logger = logger;
        }

        public async Task<bool> SendMail(string to, string subject, string body)
        {
            try
            {
                var mail = new MailMessage();
                mail.IsBodyHtml = true;
                mail.From = new MailAddress(_settings.From!, _settings.DisplayName);
                mail.To.Add(to);
                mail.Subject = subject;
                mail.Body = body;

                var client = new SmtpClient();
                client.EnableSsl = _settings.UseSSL ? true : false;
                client.Host = _settings.Host;
                client.Port = _settings.Port;
                client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
                client.UseDefaultCredentials = false;
                client.Send(mail);
                return true;


            }
            catch(Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
                return false;
            }
        }
    }
}
