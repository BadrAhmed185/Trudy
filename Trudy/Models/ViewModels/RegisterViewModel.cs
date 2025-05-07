using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
namespace Trudy.Models.ViewModels
{


    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name must contain only letters and spaces.")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email format is invalid.")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression("^0\\d{10}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقمًا ويبدأ بصفر")]
        public string Phone { get; set; }

        [DataType(DataType.Password)]
        [PasswordComplexity] // your custom validator from earlier
        [Required(ErrorMessage = "Password is required.")]

        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }



    public class PasswordComplexityAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;
            if (!string.IsNullOrEmpty(password))
            {

                if (password.Length < 8)
                    return new ValidationResult("Password must be at least 8 characters long.");

                if (!Regex.IsMatch(password, "[A-Z]"))
                    return new ValidationResult("Password must contain at least one uppercase letter.");

                if (!Regex.IsMatch(password, "[a-z]"))
                    return new ValidationResult("Password must contain at least one lowercase letter.");

                if (!Regex.IsMatch(password, "[0-9]"))
                    return new ValidationResult("Password must contain at least one digit.");

                if (!Regex.IsMatch(password, "[\\W_]")) // \W = non-word characters
                    return new ValidationResult("Password must contain at least one special character.");
            }

            return ValidationResult.Success;
        }
    }


}
