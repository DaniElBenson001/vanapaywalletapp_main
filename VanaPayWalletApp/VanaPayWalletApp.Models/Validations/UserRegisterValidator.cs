using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanaPayWalletApp.Models.Models.DtoModels;

namespace VanaPayWalletApp.Models.Validations
{
    public class UserRegisterValidator : AbstractValidator<UserRegisterRequest>
    {
        public UserRegisterValidator()
        {
            RuleFor(x => x.firstName).NotEmpty();
            RuleFor(x => x.lastName).NotEmpty();
            RuleFor(x => x.username).NotEmpty().MinimumLength(8);
            RuleFor(x => x.email).NotEmpty().EmailAddress();
            RuleFor(x => x.address).NotEmpty();
            RuleFor(x => x.password).NotEmpty().MinimumLength(8);
        }
    }
}
