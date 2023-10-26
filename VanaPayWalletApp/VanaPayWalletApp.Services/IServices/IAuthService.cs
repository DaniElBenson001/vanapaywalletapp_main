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
        Task<DataResponse<string>> CreatePin(PinCreationDto pin);
        Task<DataResponse<string>> VerifyPin(PinVerificationDto pin);
        Task<DataResponse<string>> ChangePin(PinChangeDto pin);
        Task<DataResponse<LoginViewModel>> Login(UserLoginRequest request);
        Task<DataResponse<string>> PinAvailability();
        Task<DataResponse<string>> SendSecurityQuestion(SecurityQuestionDto result);
    }
}
