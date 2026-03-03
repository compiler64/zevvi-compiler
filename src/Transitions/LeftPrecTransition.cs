using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.Transitions
{
    public class LeftPrecTransition : ITransition
    {
        public DefaultExtension Z { get; }

        public readonly ITransition innerTransition;

        public readonly int leftPrec;

        public IStateDict StateDict => innerTransition.StateDict;

        public LeftPrecTransition(int leftPrec, ITransition innerTransition)
        {
            this.leftPrec = leftPrec;
            this.innerTransition = innerTransition;
            Z = innerTransition.Z;
        }

        public LeftPrecTransition(int leftPrec, string typeName, IStateDict stateDict)
            : this(leftPrec, new NormalTransition(typeName, stateDict))
        {
        }

        public void Transition()
        {
            ParseTree currentTree = Z.PopTree();
            Z.currentTree = currentTree;

            while (true)
            {
                LinkedListNode<ParseTree> node = Z.subtrees.Last;

                while (node is not null)
                {
                    ParseTree subtree = node.Value;

                    if (subtree is RightPrecTree rsubtree)
                    {
                        int rightPrec = rsubtree.rightPrec;
                        State oldState = Z.PeekTree.state;

                        if (leftPrec == rightPrec)
                        {
                            throw new ZevviSyntaxError($"Expressions have equal operator precedences " +
                                $"({rightPrec}); parentheses required.", currentTree.GetTokens());
                        }
                        else if (leftPrec > rightPrec || !oldState.canMerge)
                        {
                            Z.PushTree(currentTree);
                            innerTransition.Transition();
                            return;
                        }
                        else
                        {
                            Z.Merge();
                            break; // break linkedlist loop, cont outer 'while'
                        }
                    }

                    node = node.Previous;
                }
            }
        }
    }
}
