using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Trudy.Helpers;
using Trudy.Models.ViewModels;
//using System.Security.Cryptography;


namespace Trudy.Controllers
{
    public class AccountController : Controller
    {
        private readonly string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "users.txt");

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check your info again";
                var errorsFromModelState = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Errors"] = errorsFromModelState;
                return View(model);
            }

        
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (System.IO.File.Exists(filePath))
            {
                var existingUsers = System.IO.File.ReadAllLines(filePath);
                foreach (var userLine in existingUsers)
                {
                    var parts = userLine.Split(',');
                    if (parts.Length >= 2 && parts[1].Equals(model.Email ))
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                        TempData["Error"] = "Please check your info again";
                        var errorsFromModelState = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                        TempData["Errors"] = errorsFromModelState;
                        return View(model);
                    }
                }
            }

            string hashedPassword = HashHelper.ComputeSha256Hash(model.Password);

            // Triple Des Keys
            using var tripleDES = TripleDES.Create();
            tripleDES.GenerateKey();
            tripleDES.GenerateIV();
            string tripleDesKey = Convert.ToBase64String(tripleDES.Key);
            string tripleDesIV = Convert.ToBase64String(tripleDES.IV);

            //  RSA Keys
            using var rsa = RSA.Create();
            string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

            // Store data (CSV format)
            string line = $"{model.Name},{model.Email},{hashedPassword},{model.Phone},{tripleDesKey},{tripleDesIV},{publicKey},{privateKey}";

            System.IO.File.AppendAllText(filePath, line + Environment.NewLine);

          //  TempData["Success"] = "User registered successfully!";

            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("Email", model.Email);
            HttpContext.Session.SetString("Name", model.Name);


            TempData["Success"] = $"Welcome, {model.Name}!";

            return RedirectToAction("HomePage", "Home");
        }





        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Login(LoginViewModel model)
        {

            // anot important because if path not exist will be routed to the sign up action that will create the directory if now exits.
            //Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check your login info.";
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Errors"] = errors;
                return View(model);
            }

            if (!System.IO.File.Exists(filePath))
            {
                  TempData["Error"] = "No user data found. Please register first";
               // TempData["Errors"] = new List<string> { "No user data found. Please register first." };
                return View(model);
            }

            var lines = System.IO.File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    string name = parts[0];
                    string email = parts[1];
                    string hashedPassword = parts[2];

                    if (email.Equals(model.Email ))
                    {
                        string enteredHash = HashHelper.ComputeSha256Hash(model.Password);
                        if (enteredHash == hashedPassword)
                        {

                            HttpContext.Session.SetString("IsAuthenticated", "true");
                            HttpContext.Session.SetString("Email", model.Email);
                            HttpContext.Session.SetString("Name", parts[0]);


                            TempData["Success"] = $"Welcome, {name}!";
       
                            return RedirectToAction("HomePage" , "Home");
                        }
                        else
                        {
                            TempData["Error"] = "Login failed.";
                            TempData["Errors"] = new List<string> { "Incorrect Email or password." };
                            return View(model);
                        }
                    }
                }
            }

            TempData["Error"] = "Login failed.";
            TempData["Errors"] = new List<string> { "Incorrect Email or password." };
            return View(model);
        }

        [HttpGet]
        [AuthFilter]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check your Info.";
                TempData["Errors"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return View(model);
            }

            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "No user data found.";
                return View(model);
            }

            var lines = System.IO.File.ReadAllLines(filePath).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                var parts = lines[i].Split(',');

                if (parts.Length >= 8)
                {
                    string name = parts[0];
                    string email = parts[1];
                    string storedHashedPassword = parts[2];

                    if (email.Equals(model.Email))
                    {
                        string currentHashed = HashHelper.ComputeSha256Hash(model.CurrentPassword);
                        if (storedHashedPassword == currentHashed)
                        {
                            string newHashed = HashHelper.ComputeSha256Hash(model.NewPassword);

                            // Keep all other fields the same, only update password
                            parts[2] = newHashed;
                            lines[i] = string.Join(",", parts);

                            System.IO.File.WriteAllLines(filePath, lines);
                            TempData["Success"] = "Password changed successfully.";
                            return RedirectToAction("HomePage" , "Home");
                        }
                        else
                        {
                            TempData["Error"] = "Incorrect Email or Password.";
                            return View(model);
                        }
                    }
                }
            }

            TempData["Error"] = "Incorrect Email or Password.";
            return View(model);
        }



        public IActionResult Reset()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset(ResetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check your info.";
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Errors"] = errors;
                return View(model);
            }

            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "No user data found. Please register first.";
                return View(model);
            }

            var lines = System.IO.File.ReadAllLines(filePath).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                var parts = lines[i].Split(',');

                if (parts.Length >= 8)
                {
                    string name = parts[0];
                    string email = parts[1];

                    if (email.Equals(model.Email) && name.Equals(model.Name))
                    {
                        string hashedNewPassword = HashHelper.ComputeSha256Hash(model.Password);
                        parts[2] = hashedNewPassword;
                        lines[i] = string.Join(",", parts);

                        System.IO.File.WriteAllLines(filePath, lines);
                        TempData["Success"] = "Password has been reset successfully.";
                        return RedirectToAction("LogIn");
                    }
                }
            }

            TempData["Error"] = "Reset password failed.";
            TempData["Errors"] = new List<string> { "Incorrect Name or Email." };
            return View(model);
        }



        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Redirect to login page
            return RedirectToAction("Login", "Account");
        }


    }

}



