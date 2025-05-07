namespace Trudy.Helpers
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    namespace Trudy.Helpers
    {
        using System.Security.Cryptography;
        using System.Text;

        public static class TripleDESHelper
        {
            public static string EncryptString(string plainText, byte[] key, byte[] iv)
            {
                using var tripleDES = TripleDES.Create();
                tripleDES.Key = key;
                tripleDES.IV = iv;

                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, tripleDES.CreateEncryptor(), CryptoStreamMode.Write);
                using var writer = new StreamWriter(cryptoStream, Encoding.UTF8);
                writer.Write(plainText);
                writer.Flush();
                cryptoStream.FlushFinalBlock();

                return Convert.ToBase64String(memoryStream.ToArray());
            }

            public static string DecryptString(string cipherText, byte[] key, byte[] iv)
            {
                using var tripleDES = TripleDES.Create();
                tripleDES.Key = key;
                tripleDES.IV = iv;

                var cipherBytes = Convert.FromBase64String(cipherText);

                using var memoryStream = new MemoryStream(cipherBytes);
                using var cryptoStream = new CryptoStream(memoryStream, tripleDES.CreateDecryptor(), CryptoStreamMode.Read);
                using var reader = new StreamReader(cryptoStream, Encoding.UTF8);

                return reader.ReadToEnd();
            }

            public static (byte[] Key, byte[] IV) GetUserTripleDesKeys(string email, string usersFilePath)
            {
                if (!File.Exists(usersFilePath))
                    throw new FileNotFoundException("User registration file not found.");

                var allUsers = File.ReadAllLines(usersFilePath);
                foreach (var line in allUsers)
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 6 && parts[1].Trim() == email.Trim())
                    {
                        var key = Convert.FromBase64String(parts[4]);
                        var iv = Convert.FromBase64String(parts[5]);
                        return (key, iv);
                    }
                }

                throw new Exception("User keys not found for the given email.");
            }
        }
    }
}
