using Microsoft.AspNetCore.Mvc;
using System.Text;
using Trudy.Helpers;
using Trudy.Helpers.Trudy.Helpers;
using Trudy.Models.ViewModels;

namespace Trudy.Controllers
{
    [AuthFilter]
    public class RsaController : Controller
    {
        private readonly string UsersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "users.txt");
        private readonly string RsaFolder = Path.Combine(Directory.GetCurrentDirectory(), "Rsa");
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Encrypt()
        {
            TempData["Algorithm"] = "RSA";
            TempData["Controller"] = "Rsa";
            return View();
        }

        [HttpPost]
        public IActionResult Encrypt(EncryptTextViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter both text and file name.";
                return View(model);
            }

            var userEmail = GetCurrentUserEmail();
            var filePath = Path.Combine(RsaFolder, model.FileName + ".txt");
            var userName = GetCurrentUserName();

            if (System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "A file with this name already exists. Please choose a different name.";
                return View(model);
            }
            try {

                var (publicKey, privateKey) = RSAHelper.GetUserRsaKeys(userEmail, UsersFilePath);

         

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                byte[] encryptedEmail = RSAHelper.Encrypt(userEmail, publicKey);
                byte[] encryptedText = RSAHelper.Encrypt(model.PlainText, publicKey);

                // Encode both as base64 to store in a text file
                var content = Convert.ToBase64String(encryptedEmail) + Environment.NewLine +
                              Convert.ToBase64String(encryptedText);

                System.IO.File.WriteAllText(filePath, content);




                TempData["Success"] = "File created and text encrypted successfully.";
                return RedirectToAction("MyEncryptedFiles");

            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Encryption failed: {ex.Message}";
                return View(model);
            }

        }


        [HttpGet]
        public IActionResult Decrypt(string fileName)
        {
            TempData["Algorithm"] = "RSA";
            TempData["Controller"] = "Rsa";

            var userEmail = GetCurrentUserEmail();


            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            var filePath = Path.Combine(RsaFolder, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var lines = System.IO.File.ReadAllLines(filePath);

            if (lines.Length < 2)
                return BadRequest("File format is incorrect.");

            var encryptedEmail = lines[0];
            var encryptedText = lines[1];

            var (publicKey, privateKey) = RSAHelper.GetUserRsaKeys(userEmail, UsersFilePath);

            var decryptedEmail = RSAHelper.Decrypt(Convert.FromBase64String(encryptedEmail), privateKey);

            if (decryptedEmail != userEmail)
                return Unauthorized();

            var decryptedText = RSAHelper.Decrypt(Convert.FromBase64String(encryptedText), privateKey);

            var model = new DecryptResultViewModel
            {
                FileName = fileName,
                DecryptedText = decryptedText,
                CreatedDate = new FileInfo(filePath).CreationTime
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult MyEncryptedFiles()
        {
            TempData["Algorithm"] = "RSA";
            TempData["Controller"] = "Rsa";
            
            var userEmail = GetCurrentUserEmail();
            var model = new List<UserEncryptedFileViewModel>();

            if (!Directory.Exists(RsaFolder))
            {
                TempData["Info"] = "No files found.";
                return View(model);
            }


            var files = Directory.GetFiles(RsaFolder, "*.txt");

            foreach (var file in files)
            {
                try
                {
                    var lines = System.IO.File.ReadAllLines(file);
                    if (lines.Length > 0)
                    {
                        var encryptedEmail = lines[0];
                        var (publicKey, privateKey) = RSAHelper.GetUserRsaKeys(userEmail, UsersFilePath);

                        var DecryptedEmail = RSAHelper.Decrypt(Convert.FromBase64String(encryptedEmail) , privateKey);
                            if (DecryptedEmail == userEmail)
                            {
                                 var fileInfo = new FileInfo(file);
                                 model.Add(new UserEncryptedFileViewModel {
                                     FileName = fileInfo.Name,
                                     CreatedDate = fileInfo.CreationTime,
                                     EncryptionMethod  = "RSA"
                                 
                                 });
                            
                            }
                    }


                }
                catch
                {
                    continue;
                }
            }

            TempData["Name"] = GetCurrentUserName();
            return View(model);



        }



        private string GetCurrentUserEmail()
        {
            return HttpContext.Session.GetString("Email");
        }
        private string GetCurrentUserName()
        {
            return HttpContext.Session.GetString("Name");
        }
    }
}
