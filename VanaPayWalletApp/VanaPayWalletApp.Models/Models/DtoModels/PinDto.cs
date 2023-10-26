



using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.DtoModels
{
    public class PinCreationDto
    {
        [Required, StringLength(4, ErrorMessage = "Please enter an numeric string of 4 characters")]
        public string UserPin { get; set; }
        [Required, Compare("UserPin"), StringLength(4, ErrorMessage = "Please enter an numeric string of 4 characters")]
        public string confirmUserPin { get; set; }
    }

    public class PinVerificationDto
    {
        [Required, StringLength(4, ErrorMessage = "Please enter an numeric string of 4 characters")]
        public string pin { get; set; }
    }

    public class PinChangeDto
    {
        [Required, StringLength(4, ErrorMessage = "Please enter an numeric string of 4 characters")]
        public string OldPin { get; set; }
        [Required, StringLength(4, ErrorMessage = "Please enter an numeric string of 4 characters")]
        public string NewPin { get; set; }
        [Required, StringLength(4, ErrorMessage = "Please enter an numeric  string of 4 characters"), Compare("ConfirmNewPin")]
        public string ConfirmNewPin { get; set; }
    }
}
