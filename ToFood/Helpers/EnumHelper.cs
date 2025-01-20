using System.ComponentModel;

namespace ToFood.Domain.Helpers;

internal static class EnumHelper
{
    public static TEnum ToEnum<TEnum>(this object value) where TEnum : struct, Enum
    {
        if (value != null &&
            Enum.TryParse<TEnum>(value.ToString(), true, out TEnum ret) &&
            Enum.IsDefined(typeof(TEnum), ret))
        {
            return ret;
        }

        return default;
    }

    /// <summary>
    /// Monta a descrição de um ENUM
    /// </summary>
    /// <param name="source">Enum</param>
    /// <typeparam name="T">Tipo do Enum</typeparam>
    /// <returns>Descrição do Enum</returns>
    public static string? ToEnumDescription<T>(this T source) where T : Enum
    {
        if (source == null) return null;
        var fi = source.GetType().GetField(source.ToString() ?? "");
        var attributes = fi?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
        return attributes?.Length > 0 ? attributes[0].Description : source.ToString();
    }
}
