using System;

namespace Censorduck;
public static class StringExtensions
{
    public static string EscapeAsterisks(this string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        return input.Replace("*", "\\*");
    }
}
