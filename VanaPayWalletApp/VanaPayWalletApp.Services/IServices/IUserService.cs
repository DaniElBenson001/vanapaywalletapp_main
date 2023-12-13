using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.Models.Entities;
using VanaPayWalletApp.Models.Models.DtoModels;
using VanaPayWalletApp.Models.Models.ViewModels;

namespace VanaPayWalletApp.Services.IServices
{
    public interface IUserService
    {
        Task<ResponseViewModel> Register(UserRegisterRequest request);
        Task<UserDataEntity?> DeleteUser(int id);
        public void CreatePasswordHash(string password,
            out byte[] passwordHash,
            out byte[] passwordSalt);
        Task<DataResponse<string>> ChangePassword(PasswordChangeDto password);
        Task<DataResponse<string>> UpdateUserDetails(UserDetailsDto userInfo);
        Task<DataResponse<string>> CreatePin(PinCreationDto pin);
        Task<DataResponse<string>> ChangePin(PinChangeDto pin);
        Task<DataResponse<string>> GetSecurityqa();
        Task<DataResponse<string>> AddSecurityqa(SecurityQuestionDto result);
        Task<DataResponse<string>> SecurityqaAvailability();

    }
}
