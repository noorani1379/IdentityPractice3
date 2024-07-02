using Microsoft.AspNetCore.Identity;

namespace IdentityPractice3.Models.Entities
{
    public class Role : IdentityRole
    {
        public string Description { get; set; }
    }
}
