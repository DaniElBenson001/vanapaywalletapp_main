using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Services.IServices
{
    public interface IMailService
    {
        Task<bool> VerifyEmailMessage(string email, string subjectBody, string emailbody1, string emailbody2, CancellationToken cncltoken = default);
    }
}
