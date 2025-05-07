using System.ComponentModel.DataAnnotations;

namespace Trudy.Models.ViewModels
{
    public class EncryptTextViewModel
    {
        [Required]
        [Display(Name = "Enter Text to Encrypt")]
        public string PlainText { get; set; }

        [Required]
        [Display(Name = "Enter File Name")]
        public string FileName { get; set; }
    }

}
