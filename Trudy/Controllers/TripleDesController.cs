using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Trudy.Helpers.Trudy.Helpers;
using Trudy.Models.ViewModels;

namespace Trudy.Controllers
{


    [AuthFilter]
    public class TripleDesController : Controller
    {
        private readonly string UsersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "users.txt");
        private readonly string TripleDesFolder = Path.Combine(Directory.GetCurrentDirectory(), "TripleDES");


        public IActionResult Index()
        {
            return View();
        }



        [HttpGet]
        public IActionResult Encrypt()
        {
            TempData["Algorithm"] = "Triple Des";
            TempData["Controller"] = "TripleDes";
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

            var userEmail = GetCurrentUserEmail(); // From session or auth context
            var filePath = Path.Combine(TripleDesFolder,  model.FileName + ".txt");

            // Check if file already exists
            if (System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "A file with this name already exists. Please choose a different name.";
                return View(model);
            }

            try
            {
                // Get the user’s TripleDES Key & IV
                var (key, iv) = TripleDESHelper.GetUserTripleDesKeys(userEmail, UsersFilePath);
                // Encrypt email and text


                string encryptedEmail = TripleDESHelper.EncryptString(userEmail, key, iv);
                string encryptedText = TripleDESHelper.EncryptString(model.PlainText, key, iv);



                // Save encrypted email and text in file
                var lines = new List<string> { encryptedEmail, encryptedText };
                System.IO.File.WriteAllLines(filePath, lines);

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
            TempData["Algorithm"] = "Triple Des";
            TempData["Controller"] = "TripleDes";

            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            var filePath = Path.Combine(TripleDesFolder, fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var lines = System.IO.File.ReadAllLines(filePath);
            if (lines.Length < 2)
                return BadRequest("File format is incorrect.");

            var encryptedEmail = lines[0];
            var encryptedText = lines[1];

            var userEmail = GetCurrentUserEmail();
            var (key, iv) = TripleDESHelper.GetUserTripleDesKeys(userEmail, UsersFilePath);
            var decryptedEmail = TripleDESHelper.DecryptString(encryptedEmail, key, iv);

            if (decryptedEmail != userEmail)
                return Unauthorized();

            var decryptedText = TripleDESHelper.DecryptString(encryptedText, key, iv);

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
            TempData["Algorithm"] = "Triple Des";
            TempData["Controller"] = "TripleDes";
            
            var userEmail = GetCurrentUserEmail();
            var model = new List<UserEncryptedFileViewModel>();

            if (!Directory.Exists(TripleDesFolder))
            {
                TempData["Info"] = "No files found.";
                return View(model);
            }

            var files = Directory.GetFiles(TripleDesFolder, "*.txt");

            foreach (var file in files)
            {
                try
                {
                    var lines = System.IO.File.ReadAllLines(file);
                    if (lines.Length > 0)
                    {
                        var encryptedEmail = lines[0];

                        var (key, iv) = TripleDESHelper.GetUserTripleDesKeys(userEmail, UsersFilePath);
                        var decryptedEmail = TripleDESHelper.DecryptString(encryptedEmail, key, iv);

                        if (decryptedEmail == userEmail)
                        {
                            var fileInfo = new FileInfo(file);
                            model.Add(new UserEncryptedFileViewModel
                            {
                                FileName = fileInfo.Name,
                                CreatedDate = fileInfo.CreationTime,
                                EncryptionMethod = "TripleDES"
                            });
                        }
                    }
                }
                catch
                {
                    continue; // Skip unreadable/malformed files
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
