using MimeKit;
using System.Text.RegularExpressions;

namespace ToFood.Domain.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Valida um e-mail
    /// </summary>
    /// <param name="email">E-mail</param>
    /// <returns>True se o e-mail for válido</returns>
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Expressão regular para verificar o formato do e-mail
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!Regex.IsMatch(email, emailPattern))
            return false;

        try
        {
            // Usa a biblioteca MimeKit para validar o e-mail
            var addr = new MailboxAddress("", email);
            if (addr.Address != email)
                return false;
        }
        catch
        {
            return false;
        }

        // Caso passe por todas as validações, o e-mail é válido
        return true;
    }
}
