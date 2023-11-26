using System.Collections.Generic;

namespace RenamerCore.Extensions;

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
        => string.Join(separator, values);
}
