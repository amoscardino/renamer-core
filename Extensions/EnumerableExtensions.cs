using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RenamerCore.Extensions
{
    public static class EnumerableExtensions
    {
        public static string StringJoin<T>(this IEnumerable<T> values, string separator = "")
        {
            return string.Join(separator, values);
        }
    }
}