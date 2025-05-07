using System.ComponentModel.DataAnnotations;

namespace Trudy.Models.ViewModels
{
    public class ResetViewModel
    {

        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name must contain only letters and spaces.")]
        [Display(Name = "Enter your Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Enter your Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [PasswordComplexity]
        [Required(ErrorMessage = "The new Password is required.")]

        [Display(Name =  "New Password")]
        public string Password { get; set; }
    }
}
