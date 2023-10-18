using System.Net;
using System.Net.Mail.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.Services.IServices;
using VanaPayWalletApp.Models.Models.DtoModels;
using Microsoft.Extensions.Logging;
using SmtpClient = System.Net.Mail.SmtpClient;
using Microsoft.Extensions.Configuration;
using NPOI.SS.Formula.Functions;

namespace VanaPayWalletApp.Services.Services
{
    public class MailService : IMailService
    {
        private readonly MailConfig _settings;
        private readonly ILogger<MailService> _logger;

        public MailService(IConfiguration config, ILogger<MailService> logger)
        {
            _settings = config.GetSection("MailConfig").Get<MailConfig>();
            _logger = logger;
        }

        public async Task<Tuple<bool, string>> SendMail(string to, string subject, string body)
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
                client.Host = _settings.Host!;
                client.Port = _settings.Port;
                client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
                client.UseDefaultCredentials = false;
                client.Send(mail);
                return Tuple.Create(true, "Email Sent Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
                return Tuple.Create(false, "Email Failed");
            }
        }

        public async Task<bool> VerifyEmailMessage(string email, string subjectBody, string emailbody1, string emailbody2, CancellationToken cncltoken = default)
        {
            try
            {
                string HtmlBody = emailbody1 + emailbody2;
                await SendMail(email, subjectBody, HtmlBody);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURED.... => {ex.Message}");
                _logger.LogInformation($"The Error occured at{DateTime.UtcNow.ToLongTimeString()}, {DateTime.UtcNow.ToLongDateString()}");
                return false;
            }
        }
    }
}
