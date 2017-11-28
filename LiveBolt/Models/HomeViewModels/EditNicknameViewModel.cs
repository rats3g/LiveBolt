using System;
using System.ComponentModel.DataAnnotations;

namespace LiveBolt.Models.HomeViewModels
{
    public class EditNicknameViewModel
    {
        [Required]
        public string Nickname { get; set; }
    }
}