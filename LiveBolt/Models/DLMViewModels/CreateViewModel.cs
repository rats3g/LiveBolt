using System;
using System.ComponentModel.DataAnnotations;

namespace LiveBolt.Models.DLMViewModels
{
    public class CreateViewModel
    {
        [Required]
        public Guid Id { get; set; }

        // TODO: [Reqiured]
        public Guid CompanyKey { get; set; }

        [Required]
        public bool IsLocked { get; set; }
    }
}