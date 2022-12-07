using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Extensions;

internal static class StringExtensions
{
    public static string ToCamelCase(this string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return source;
        }
        return char.ToLowerInvariant(source[0]) + source[1..];
    }
}
