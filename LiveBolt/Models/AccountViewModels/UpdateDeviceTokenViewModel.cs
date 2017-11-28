using System.ComponentModel.DataAnnotations;

namespace LiveBolt.Models.AccountViewModels
{
    public class UpdateDeviceTokenViewModel
    {
        [Required]
        public string DeviceToken { get; set; }
    }
}