using System;
using System.ComponentModel.DataAnnotations;

namespace LiveBolt.Models.IDMViewModels
{
    public class EditNicknameViewModel
    {
        [Required]
        public Guid Guid { get; set; }

        [Required]
        public string Nickname { get; set; }
    }
}