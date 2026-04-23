namespace App.Common;

public static class TelefoneHelper
{
    public static string? Normalizar(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            return null;
        }

        var digits = new string(numero.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }
}