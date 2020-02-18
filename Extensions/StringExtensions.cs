using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RenamerCore.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string Clean(this string str)
        {
            str = Regex.Replace(str ?? string.Empty, @"[\._]", " ");

            return Regex.Replace(str, @"[^a-zA-Z\d\s]", string.Empty).Trim();
        }

        public static string CleanFileName(this string str)
        {
            return str.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries).StringJoin();
        }

        public static string CleanPath(this string str)
        {
            return str.Split(Path.GetInvalidPathChars(), StringSplitOptions.RemoveEmptyEntries).StringJoin();
        }

        public static string DropLastWord(this string str)
        {
            // Split the string into words
            var words = (str ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // If we only have 1 word, return nothing as we can't do any more.
            if (words.Length <= 1)
                return string.Empty;

            // Drop the last word.
            return string.Join(" ", words.Take(words.Length - 1));
        }
    }
}