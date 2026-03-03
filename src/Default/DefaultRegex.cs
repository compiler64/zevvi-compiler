using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ZevviCompiler
{
    public static class DefaultRegex
    {
        public static readonly Regex defaultRegex = new(
@"^(?<ignore>(//.*?\n)|(/\*(.|\r|\n)*?\*/)|([ \t\r\n]+))|(?<Char>'([^'\\]|\\.)')|(?<Str>\x22([^\x22\\]|\\.)*\x22)|(?<Float>[0-9]*\.[0-9]+(e(\+|-)?[0-9]+)?)|(?<Int>[0-9]+)|(?<Id>([a-zA-Z_$][a-zA-Z0-9_$]*)|([+\-*/%<=>:.&|!^~]+)|([\\;?@#,()\[\]{}]))",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    }
}
