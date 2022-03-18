using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RenamerCore.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns true if the string is null or only whitespace. Wrapper around string.IsNullOrWhiteSpace(str).
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Cleans a string by doing 2 things:
        /// 1. Replaces all dots and underscores with spaces
        /// 2. Removes all characters that are not letters, digits, or spaces
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Clean(this string str)
        {
            str = Regex.Replace(str ?? string.Empty, @"[\._]", " ");

            return Regex.Replace(str, @"[^a-zA-Z\d\s]", string.Empty).Trim();
        }

        /// <summary>
        /// Cleans a string for a file name. Uses Path.GetInvalidFileNameChars() for platform specific rules
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string CleanFileName(this string str)
        {
            // Allowed: letters, numbers, spaces, and any of: ()-&,!%'
            var pattern = @"[^a-zA-Z\d\.\(\)\-\s\&\,\!\%\']";

            return Regex.Replace(str ?? string.Empty, pattern, string.Empty);
        }

        /// <summary>
        /// Cleans a string for a path name. Uses Path.GetInvalidFileNameChars() for platform specific rules
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string CleanPath(this string str)
        {
            // Allowed: letters, numbers, spaces, and any of: ()-&,!%'
            var pattern = @"[^a-zA-Z\d\.\(\)\-\s\&\,\!\%\']";

            return Regex.Replace(str ?? string.Empty, pattern, string.Empty);
        }

        /// <summary>
        /// Returns a new string that matches the input string, but with the last word removed.
        /// If there is only one word, then an empty string is returned.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a new string that matches the input string, but with the first word removed.
        /// If there is only one word, then an empty string is returned.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DropFirstWord(this string str)
        {
            // Split the string into words
            var words = (str ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // If we only have 1 word, return nothing as we can't do any more.
            if (words.Length <= 1)
                return string.Empty;

            // Drop the first word.
            return string.Join(" ", words.Skip(1));
        }
    }
}
