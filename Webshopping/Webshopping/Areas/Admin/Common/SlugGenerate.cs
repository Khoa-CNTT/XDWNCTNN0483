namespace Webshopping.Areas.Admin.Common;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public class SlugGenerate
{
    public static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null; // Return null if input is invalid
        }

        // Example slug generation logic
        return input.ToLower().Replace(" ", "-").Replace(".", "").Replace(",", "");
    }

    public static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var ch in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC)
                     .Replace("đ", "d").Replace("Đ", "D");
    }
}