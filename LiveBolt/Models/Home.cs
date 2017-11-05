using System.Collections.Generic;

namespace LiveBolt.Models
{
    public class Home
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Nickname { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] Salt { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
    }
}