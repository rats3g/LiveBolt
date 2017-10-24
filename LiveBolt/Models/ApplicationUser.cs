using Microsoft.AspNetCore.Identity;

namespace LiveBolt.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public int? HomeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
