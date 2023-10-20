using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanaPayWalletApp.Models.Entities
{
    public class SecurityQuestionDataEntity
    {
        public SecurityQuestionDataEntity()
        {
            Attempts = 3;
            Question = "Who is your Favorite Cartoon Character?";
        }
        [Key]
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int Attempts { get; set; }

        [ForeignKey("UserDataEntity")]
        public int UserId { get; set; }
        public UserDataEntity UserDataEntity { get; set; }
    }
}
