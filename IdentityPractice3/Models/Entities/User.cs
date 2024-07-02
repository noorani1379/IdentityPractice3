using Microsoft.AspNetCore.Identity;

namespace IdentityPractice3.Models.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
