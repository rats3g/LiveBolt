using System;
using System.ComponentModel.DataAnnotations;

namespace LiveBolt.Models.DLMViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public Guid Guid { get; set; }

        [Required]
        public string Nickname { get; set; }
    }
}