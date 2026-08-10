using System.Text.RegularExpressions;

namespace App.Common;

public static class TextoHelper
{
    public static string? NormalizarTelefone(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            return null;
        }

        var digits = new string(numero.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }

    public static string RetornaDescricaoBool(bool? condicao)
    {
        string descricao = "Não";

        if (condicao is true)
        {
            descricao = "Sim";
        }

        return descricao;
    }

    public static string FormataCpfCnpj(string? cpfCnpj)
    {
        if (string.IsNullOrWhiteSpace(cpfCnpj))
        {
            return "Não informado";
        }

        string apenasNumeros = Regex.Replace(cpfCnpj, "[^0-9]", "");

        if (apenasNumeros.Length == 11)
        {
            return Regex.Replace(apenasNumeros, @"(\d{3})(\d{3})(\d{3})(\d{2})", "$1.$2.$3-$4");
        }

        if (apenasNumeros.Length == 14)
        {
            return Regex.Replace(apenasNumeros, @"(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})", "$1.$2.$3/$4-$5");
        }

        return cpfCnpj;
    }
}