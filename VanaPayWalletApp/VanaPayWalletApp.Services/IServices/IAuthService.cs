using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Models.Models.ViewModels;

namespace VanaPayWalletApp.Services.IServices
{
    public interface IAuthService
    {
        Task<DataResponse<string>> VerifyPin(PinVerificationDto pin);
        Task<DataResponse<LoginViewModel>> Login(UserLoginRequest request);
        Task<DataResponse<string>> PinAvailability();
        //Task<DataResponse<string>> VerifySecurityQuestion(SecurityQuestionDto result);
        public bool VerifyPasswordHash(string password,
           byte[] passwordHash,
           byte[] passwordSalt);

        public bool VerifyPinHash(string pin,
            byte[] pinHash,
            byte[] pinSalt);
    }
}
