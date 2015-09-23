using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocToMarkdown
{
    public static class StringExtensions
    {
        public static IEnumerable<string> Split(this string source, string delim)
        {
            // argument null checking etc omitted for brevity

            int oldIndex = 0, newIndex;
            while ((newIndex = source.IndexOf(delim, oldIndex, StringComparison.Ordinal)) != -1)
            {
                yield return source.Substring(oldIndex, newIndex - oldIndex);
                oldIndex = newIndex + delim.Length;
            }
            yield return source.Substring(oldIndex);
        }
    }
}
