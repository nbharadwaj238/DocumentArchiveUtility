using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DocumentArchiveUtility.Lib
{
    public static class Extensions
    {
        public static bool Between(this DateTime dt, DateTime start, DateTime end)
        {
            if (start < end) return dt >= start && dt <= end;
            return dt >= end && dt <= start;
        }

        public static Match RegexMatch(this string input, string pattern, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            return Regex.Match(input, pattern, regexOptions);
        }


    }
}
