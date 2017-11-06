using System;
using System.ComponentModel.DataAnnotations;

namespace LiveBolt.Models.IDMViewModels
{
    public class CreateViewModel
    {
        [Required]
        public Guid Id { get; set; }
        // TODO: [Required]
        public Guid CompanyKey { get; set; }
        [Required]
        public bool IsClosed { get; set; }
    }
}