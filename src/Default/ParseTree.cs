using System;
using System.Collections.Generic;
using System.Linq;

namespace ZevviCompiler
{
    public class ParseTree
    {
        public ParseTree[] subtrees;
        public State state;
        public IType exprType;
        public ZI.Code exprCode;
        public Nonterminal nonterminal;

        public ZI.IOperand Storage => exprCode.storage;

        public virtual bool IsLeaf => false;

        public ParseTree(ParseTree[] subtrees, State state, IType exprType, ZI.Code exprCode, Nonterminal nonterminal)
        {
            if (subtrees is null)
            {
                throw new ArgumentNullException(nameof(subtrees));
            }
            else if (exprType is null && state.numToPop != 0)
            {
                throw new ArgumentNullException(nameof(exprType));
            }
            else if (exprCode is null)
            {
                throw new ArgumentNullException(nameof(exprCode));
            }

            this.subtrees = subtrees;
            this.exprType = exprType;
            this.exprCode = exprCode;
            this.state = state;
            this.nonterminal = nonterminal;
        }

        public ParseTree(ParseTree[] subtrees, IType exprType, ZI.Code exprCode, Nonterminal nonterminal)
            : this(subtrees, State.Null, exprType, exprCode, nonterminal)
        {
        }

        public override string ToString()
        {
            string str = "{ " + string.Join<ParseTree>(" ", subtrees) + " }";
            return str.Length < 100 ? str : "{ ... }";
        }

        public virtual RightPrecTree AttachRightPrec(int rightPrec)
        {
            return new RightPrecTree(subtrees, exprType, exprCode, state, nonterminal, rightPrec);
        }

        public virtual ParseTree Reload()
        {
            return this;
        }

        public static ParseTree CombineTrees(ParseTree[] subtrees, IType returnType, ZI.Code code, Nonterminal nonterminal)
        {
            return new ParseTree(subtrees, returnType, subtrees.Select(tree => tree.exprCode).Aggregate((code1, code2) => code1 + code2) + code, nonterminal);
        }

        public static ParseTree CombineTrees(ParseTree[] subtrees, Func<int, bool> indexPredicate, IType returnType, ZI.Code code, Nonterminal nonterminal)
        {
            return new ParseTree(subtrees, returnType, subtrees.Where((item, index) => indexPredicate(index)).Select(tree => tree.exprCode).Aggregate((code1, code2) => code1 + code2) + code, nonterminal);
        }

        /*public static ParseTree CombineTrees(ParseTree[] subtrees, ISet<int> indices, ZType returnType, ZI.CodeMultiFunc opCode, ZI.CodeFunc[] convertCodes)
        {
            ZI.Code code = opCode(convertCodes.)
            return CombineTrees(subtrees, indices, returnType, opCode())
        }*/

        public static ParseTree CombineTrees(ParseTree[] subtrees, ISet<int> indices, IType returnType, ZI.CodeMultiFunc opCode, ZI.CodeFunc[] convertCodes, Nonterminal nonterminal)
        {
            ZI.Code newCode = ZI.Code.None;
            List<ZI.IOperand> storages = new();

            if (indices.Count != convertCodes.Length)
            {
                throw new ZevviInternalCompilerError("ZI.Code[] convertCodes must be the same length as indices.");
            }

            int indexNum = 0;

            for (int i = 0; i < subtrees.Length; i++)
            {
                if (indices.Contains(i))
                {
                    ParseTree subtree = subtrees[i];
                    newCode += subtree.exprCode + convertCodes[indexNum](subtree.Storage);
                    storages.Add(newCode.storage);
                    indexNum++;
                }
            }

            newCode += opCode(storages.ToArray());
            return new ParseTree(subtrees, returnType, newCode, nonterminal);
        }

        public static ParseTree CombineTrees(ParseTree[] subtrees, ISet<int> indices, IList<IType> paramTypes, IType returnType, ZI.CodeMultiFunc codeMultiFunc, Nonterminal nonterminal)
        {
            ParseTree[] args = subtrees.Where((item, index) => indices.Contains(index)).ToArray();

            ZI.Code newCode = ZI.Code.None;
            int argNum = 0;

            if (args.Length != paramTypes.Count)
            {
                int minIndex = indices.Min();
                int maxIndex = indices.Max();
                throw new ZevviTypeError($"Wrong number of arguments: found {args.Length}, expected {paramTypes.Count}.",
                    subtrees.GetTokensRange(minIndex, maxIndex));
            }

            ZI.IOperand[] storages = new ZI.IOperand[indices.Count];

            for (int i = 0; i < subtrees.Length; i++)
            {
                if (indices.Contains(i))
                {
                    ParseTree subtree = subtrees[i];
                    ZI.Code convert = subtree.ImplicitConvertTree(paramTypes[argNum]);

                    if (convert is null)
                    {
                        if (subtree.exprType is IdentifierType idType)
                        {
                            throw new ZevviUnknownIdentifierError(idType.Identifier, subtree.GetTokens());
                        }
                        throw new ZevviTypeError($"Type error in subtree {i}: Cannot convert from {subtrees[i].exprType} to {paramTypes[argNum]}.",
                            subtrees[i].GetTokens());
                    }

                    newCode += convert;
                    storages[argNum] = convert.storage;
                    argNum++;
                }
                else
                {
                    newCode += subtrees[i].exprCode;
                }
            }

            return new ParseTree(subtrees.ToArray(), returnType, newCode + codeMultiFunc(storages), nonterminal);
        }

        public ParseTree Clone()
        {
            return new ParseTree(subtrees, exprType, exprCode, nonterminal);
        }

        public ParseTree DeepClone()
        {
            return new ParseTree(subtrees.Select(tree => tree.DeepClone()).ToArray(), exprType, exprCode, nonterminal);
        }

        public ZI.Code ConvertedWith(ZI.CodeFunc convert)
        {
            return convert is null ? null : exprCode + convert(Storage);
        }

        public ZI.Code AutoConvertTree(IType newType)
        {
            return ConvertedWith(exprType.AutoConvert(newType));
        }

        public ZI.Code ImplicitConvertTree(IType newType)
        {
            return ConvertedWith(exprType.ImplicitConvert(newType));
        }

        public ZI.Code ExplicitConvertTree(IType newType)
        {
            return ConvertedWith(exprType.ExplicitConvert(newType));
        }

        public string GetIdentifier(Func<IType, string> errorMessage)
        {
            if (errorMessage == null)
            {
                return exprType.As<IIdentifierType>().Identifier;
            }
            else
            {
                return exprType.As<IIdentifierType>(
                    type => new ZevviSyntaxError(errorMessage(type), GetTokens())
                ).Identifier;
            }
        }

        public virtual List<Token> GetTokens()
        {
            List<Token> tokens = new();

            Stack<int> indices = new();
            Stack<ParseTree> trees = new();

            indices.Push(0);
            trees.Push(null);
            indices.Push(0);
            trees.Push(this);

            while (indices.Count > 1)
            {
                int index = indices.Peek();
                ParseTree tree = trees.Peek();

                if (index >= tree.subtrees.Length)
                {
                    indices.Pop();
                    trees.Pop();
                }
                else if (tree.subtrees[index] is IParseLeaf leaf)
                {
                    tokens.Add(leaf.Token);
                }
                else
                {
                    indices.Push(0);
                    trees.Push(tree.subtrees[index]);
                    continue;
                }

                indices.Push(indices.Pop() + 1);
            }

            return tokens;
        }

        public static List<Token> GetTokensRange(IList<ParseTree> trees)
        {
            List<Token> tokens = new();

            foreach (ParseTree tree in trees)
            {
                tokens.AddRange(tree.GetTokens());
            }

            return tokens;
        }

        public static List<Token> GetTokensRange(IList<ParseTree> trees, int startIndexInclusive, int endIndexInclusive)
        {
            if (endIndexInclusive < startIndexInclusive)
            {
                return new List<Token>();
            }

            List<Token> tokens = new();

            for (int i = startIndexInclusive; i <= endIndexInclusive; i++)
            {
                tokens.AddRange(trees[i].GetTokens());
            }

            return tokens;
        }
    }
}
