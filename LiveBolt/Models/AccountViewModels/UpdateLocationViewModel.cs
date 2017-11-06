using System.ComponentModel.DataAnnotations;

namespace LiveBolt.Models.AccountViewModels
{
    public class UpdateLocationViewModel
    {
        [Required]
        public bool IsHome { get; set; }
    }
}
