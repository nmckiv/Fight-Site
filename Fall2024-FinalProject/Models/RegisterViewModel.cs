using System.ComponentModel.DataAnnotations;
namespace Fall2024_FinalProject.Models

{
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public byte[]? ProfilePhoto { get; set; }
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
