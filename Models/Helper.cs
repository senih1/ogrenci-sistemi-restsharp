using System.Security.Cryptography;
using System.Text;

namespace ogrenci_sistemi_restsharp.Codes.Models
{
    public class Helper
    {
        public static string Hash(string input)
        {
            using HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes("G"));
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = hmac.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }

}
