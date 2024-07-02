
using System.ComponentModel.DataAnnotations;


namespace IdentityPractice3.Models.DTOs.Account
{
    public class ForgotPasswordConfirmationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
