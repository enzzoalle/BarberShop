using System.Security.Cryptography;
using System.Text;

namespace App.Common
{
    public static class Criptografia
    {
        public static string geraHash_SHA512(string valor)
        {
            var arrHash = SHA512.HashData(Encoding.UTF8.GetBytes(valor));
            var sbHash = new StringBuilder();

            foreach (var t in arrHash)
            {
                sbHash.Append(t.ToString("x2"));
            }

            return sbHash.ToString();
        }
    }
}