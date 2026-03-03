using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZevviCompiler.OperatorSyntaxes
{
    public class OperatorOverloadDict : IEnumerable<OperatorOverloadFunc>
    {
        private readonly List<OperatorOverloadFunc> list = new();
        public readonly ISet<int> indices;

        public OperatorOverloadFunc this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        public MergeFunc this[IEnumerable<IType> types]
        {
            get => Get(types);
        }

        public OperatorOverloadDict(params int[] indices) : this(indices.ToHashSet())
        {
        }

        public OperatorOverloadDict(ISet<int> indices)
        {
            this.indices = indices;
        }

        public MergeFunc Get(IEnumerable<IType> types)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                OperatorOverloadFunc func = list[i];
                MergeFunc mergeFunc = func(types.ToArray());

                if (mergeFunc is not null)
                {
                    return mergeFunc;
                }
            }

            return null;
        }

        public MergeFunc GetOrThrow(ParseTree[] subtrees, IEnumerable<IType> types, string operatorName)
        {
            return Get(types) ?? throw new ZevviTypeError($"Invalid operand types for operator '{operatorName}': " +
                $"{string.Join(", ", types)}.", subtrees.GetTokensRange());
        }

        public void Add(IType[] types, IType returnType, ZI.CodeMultiFunc opCode, Nonterminal nonterminal)
        {
            list.Add(types2 =>
            {
                ZI.CodeFunc[] convertCodes = ZType.AutoConvertList(types2, types);

                return convertCodes is null ? null
                    : subtrees => ParseTree.CombineTrees(subtrees, indices, returnType, opCode, convertCodes, nonterminal);
            });
        }

        /*public void Add(ZType[] types, MergeFunc mergeFunc)
        {
            list.Add(types2 => ZType.AutoConvertList(types2, types) is null ? null : mergeFunc);
        }*/

        public void Add(OperatorOverloadFunc func)
        {
            list.Add(func);
        }

        IEnumerator<OperatorOverloadFunc> IEnumerable<OperatorOverloadFunc>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}
