
using System.ComponentModel.DataAnnotations;


namespace IdentityPractice3.Models.DTOs.Account
{
    public class ResetPasswordDto
    {

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
       [Required]
       [DataType(DataType.Password)]
       [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }

    }
}
