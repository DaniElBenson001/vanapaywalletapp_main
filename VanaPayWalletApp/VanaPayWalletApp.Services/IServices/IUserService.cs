﻿using Microsoft.AspNetCore.Mvc;
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
        Task<RegisterViewModel> Register(UserRegisterRequest request);
        Task<DataResponse<LoginViewModel>> Login(UserLoginRequest request);
        Task<UserDataEntity?> DeleteUser(int id);
    }
}