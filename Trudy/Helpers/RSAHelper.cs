namespace Trudy.Helpers
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class RSAHelper
    {
        public static byte[] Encrypt(string plainText, string base64PublicKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(base64PublicKey), out _);

            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            return rsa.Encrypt(inputBytes, RSAEncryptionPadding.Pkcs1);
        }

        public static string Decrypt(byte[] cipherBytes, string base64PrivateKey)
        {
            //using var rsa = RSA.Create();
            //rsa.ImportRSAPrivateKey(Convert.FromBase64String(base64PrivateKey), out _);

            //byte[] outputBytes = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
            //return Encoding.UTF8.GetString(outputBytes);

            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(base64PrivateKey) , out _);

            byte[] outputBytes = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);

            return Encoding.UTF8.GetString(outputBytes);
        }

        public static (string publicKey,string privateKey) GetUserRsaKeys(string email, string usersFilePath)
        {
            if (!File.Exists(usersFilePath))
                throw new FileNotFoundException("User registration file not found.");

            var allUsers = File.ReadAllLines(usersFilePath);
            foreach (var line in allUsers)
            {
                var parts = line.Split(',');
                if (parts.Length >= 6 && parts[1].Trim() == email.Trim())
                {
                    var publicKey = parts[6];
                    var privateKey = parts[7];
                    return (publicKey, privateKey);
                }
            }

            throw new Exception("User keys not found for the given email.");
        }
    }

}
