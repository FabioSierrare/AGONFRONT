using System;
using System.Security.Cryptography;
using System.Text;

namespace AGONFRONT.Utils
{
    public static class Encriptador
    {
        public static string Encriptar(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto.Trim());
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
