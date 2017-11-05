using System.Collections.Generic;

namespace LiveBolt.Models.HomeViewModels
{
    public class HomeStatusViewModel
    {
        public string Name { get; set; }
        public string Nickname { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public ICollection<HomeUserViewModel> Users { get; set; }
    }
}
