using System;

namespace Trudy.Models.ViewModels
{
    public class UserEncryptedFileViewModel
    {
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string EncryptionMethod { get; set; }
    }
}
