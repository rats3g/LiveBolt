using System;
using System.ComponentModel.DataAnnotations;

namespace LiveBolt.Models.HomeViewModels
{
    public class SetDLMStateViewModel
    {
        [Required]
        public Guid DLMId { get; set; }

        [Required]
        public bool Locked { get; set; }
    }
}