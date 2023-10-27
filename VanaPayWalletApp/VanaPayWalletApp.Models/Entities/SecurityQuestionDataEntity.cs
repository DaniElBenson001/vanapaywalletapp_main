﻿using System;
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
        }
        [Key]
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int Attempts { get; set; }
        public int UserId { get; set; }
    }
}