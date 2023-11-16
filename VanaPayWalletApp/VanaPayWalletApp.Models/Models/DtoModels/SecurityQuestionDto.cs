using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Models.DtoModels
{
    public class SecurityQuestionDto
    {
        public string question { get; set; } = string.Empty;
        public string answer { get; set; } = string.Empty;
    }

    public class QuestionDto
    {
        public string question { get; set; }
    }

    public class AnswerDto
    {
        public string answer { get; set; }
    }
}
