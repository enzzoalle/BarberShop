using System.Security.Cryptography;
using System.Text;

namespace App.Common;

public static class Criptografia
{
    public static string GeraHash(string valor)
    {
        var arrayHash = SHA512.HashData(Encoding.UTF8.GetBytes(valor));
        var sbHash = new StringBuilder();

        foreach (var t in arrayHash)
        {
            sbHash.Append(t.ToString("x2"));
        }

        return sbHash.ToString();
    }
}