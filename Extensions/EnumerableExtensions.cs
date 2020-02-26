using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RenamerCore.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Create a string by joining all the values in a collection. Wrapper around string.Join(separator, values)
        /// </summary>
        /// <param name="values"></param>
        /// <param name="separator"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string StringJoin<T>(this IEnumerable<T> values, string separator = "")
        {
            return string.Join(separator, values);
        }
    }
}