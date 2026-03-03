using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZevviCompiler.ZI
{
    public class Code
    {
        public const int MAX_INSTRUCTIONS_TO_DISPLAY = 5;

        public static Code None => new(null, new List<Triple>());

        public static Code Operand(IOperand operand)
        {
            return new Code(operand, new List<Triple>());
        }

        public Triple Start => triples[0];

        public Triple End => triples[^1];

        public int Length => triples.Count;

        public bool IsEmpty => triples.Count == 0;

        public bool IsNone => triples.Count == 0 && storage is null;

        public IOperand storage;
        public readonly List<Triple> triples;

        public Code(IOperand storage, List<Triple> triples)
        {
            this.storage = storage;
            this.triples = triples;
        }

        public Code(IOperand storage, params Triple[] triples)
        {
            this.storage = storage;
            this.triples = triples.ToList();
        }

        public override string ToString()
        {
            if (IsEmpty)
            {
                return storage is null ? "ZI.Code.None" : $"ZI.Code.Operand({storage})";
            }
            else
            {
                return string.Join(";  ", triples.Where((_, i) => i < MAX_INSTRUCTIONS_TO_DISPLAY).Select(triple => triple.ToStringFull()))
                    + (triples.Count > MAX_INSTRUCTIONS_TO_DISPLAY ? ";  ..." : "");
            }
        }

        public string ToMultiLineString()
        {
            return string.Join(Environment.NewLine, triples.Select(triple => triple.ToStringFull()));
        }

        public Code WithStorage(IOperand storage)
        {
            this.storage = storage;
            return this;
        }

        public ZI.Code ConvertedWith(CodeFunc convert)
        {
            return convert is null ? null : this + convert(storage);
        }

        public static Code operator +(Code left, Code right)
        {
            return new Code(right.IsNone ? left.storage : right.storage, left.triples.Concat(right.triples).ToList());
        }

        public static Code Join(IEnumerable<Code> codeList)
        {
            return codeList.Aggregate((code1, code2) => code1 + code2);
        }

        public static void WriteToFile(string filePath, Code code)
        {
            File.WriteAllText(filePath, code.ToMultiLineString() + Environment.NewLine);
        }
    }
}
