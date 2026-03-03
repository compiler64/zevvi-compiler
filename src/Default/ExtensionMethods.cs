using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZevviCompiler
{
    public static class ExtensionMethods
    {
        public static List<Token> GetTokensRange(this IList<ParseTree> trees)
        {
            return ParseTree.GetTokensRange(trees);
        }

        public static List<Token> GetTokensRange(this IList<ParseTree> trees, int startIndexInclusive, int endIndexInclusive)
        {
            return ParseTree.GetTokensRange(trees, startIndexInclusive, endIndexInclusive);
        }

        public static IEnumerable<IType> Types(this IEnumerable<ParseTree> parseTrees)
        {
            return parseTrees.Select(tree => tree.exprType);
        }

        public static IEnumerable<ZI.Code> Codes(this IEnumerable<ParseTree> parseTrees)
        {
            return parseTrees.Select(tree => tree.exprCode);
        }

        public static IEnumerable<ZI.IOperand> Storages(this IEnumerable<ParseTree> parseTrees)
        {
            return parseTrees.Select(tree => tree.Storage);
        }

        public static IEnumerable<ZI.IOperand> Storages(this IEnumerable<ZI.Code> parseTrees)
        {
            return parseTrees.Select(code => code.storage);
        }

        public static ZI.CodeFunc CodeFunc(this ZI.Operator op)
        {
            return storage => new ZI.Triple(op, storage);
        }

        public static ZI.CodeMultiFunc CodeMultiFunc1(this ZI.Operator op)
        {
            return storages => new ZI.Triple(op, storages[0]);
        }

        public static ZI.CodeMultiFunc CodeMultiFunc2(this ZI.Operator op)
        {
            return storages => new ZI.Triple(op, storages[0], storages[1]);
        }
    }
}
