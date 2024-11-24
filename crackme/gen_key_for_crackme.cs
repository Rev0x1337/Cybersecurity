//https://crackmes.one/crackme/6422c9b833c5d447bc761e90
using System;
using System.Text;

namespace SerialKeyGenerator
{
    public static class LicenseManager
    {
        
        public static string GenerateSerialKey(string username)
        {
            return BitConverter.ToString(ComputeHash(username))
                .Replace("-", "")
                .Substring(0, 15)
                .Insert(5, "-")
                .Insert(11, "-");
        }

        
        private static byte[] ComputeHash(string input)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                return sha.ComputeHash(inputBytes);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Введите имя пользователя: ");
            string username = Console.ReadLine();

            string serialKey = LicenseManager.GenerateSerialKey(username);
            Console.WriteLine($"Серийный ключ для {username}: {serialKey}");
        }
    }
}
