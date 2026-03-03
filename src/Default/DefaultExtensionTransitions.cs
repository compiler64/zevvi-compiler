using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler
{
    /*public partial class DefaultExtension
    {
        *//*public void NormalTransition(string typeName)
        {
            Transition(typeName, NormalDict);
        }*/

        /*public void Transition(string typeName, StateDict stateDict)
        {
            ParseTree currentTree = PopTree();

            while (true)
            {
                State oldState = PeekTree.state;
                State newState = stateDict.Get(oldState);

                if (newState != State.Error)
                {
                    currentTree.state = newState;
                    PushTree(currentTree);
                    return;
                }
                else if (oldState.canMerge)
                {
                    Merge(oldState.numToPop);
                }
                else if (PeekTree.exprType is IdentifierType idType) // unknown identifier error
                {
                    throw new ZevviUnknownIdentifierError(idType.Identifier, PeekTree.GetTokens());
                }
                else
                {
                    throw new ZevviSyntaxError($"Expression of type '{typeName}' is " +
                        $"not allowed at state {oldState}.", currentTree.GetTokens());
                }
            }
        }

        public void LeftPrecTransition(string typeName, StateDict stateDict, int leftPrec)
        {
            LeftPrecTransition(() => Transition(typeName, stateDict), leftPrec);
        }

        public void LeftPrecTransition(TransitionFunc InnerTransition, int leftPrec)
        {
            ParseTree currentTree = PopTree();

            while (true)
            {
                LinkedListNode<ParseTree> node = subtrees.Last;

                while (node is not null)
                {
                    ParseTree subtree = node.Value;

                    if (subtree is RightPrecTree rsubtree)
                    {
                        int rightPrec = rsubtree.rightPrec;
                        State oldState = subtrees.Last.Value.state;

                        if (leftPrec == rightPrec)
                        {
                            throw new ZevviSyntaxError($"Expressions have equal operator precedences " +
                                $"({leftPrec}); parentheses required.", currentTree.GetTokens());
                        }
                        else if (leftPrec > rightPrec || !oldState.canMerge)
                        {
                            PushTree(currentTree);
                            InnerTransition();
                            return;
                        }
                        else
                        {
                            Merge(oldState.numToPop);
                            break; // break linkedlist loop, cont outer 'while'
                        }
                    }

                    node = node.Previous;
                }
            }
        }

        public void RightPrecTransition(string typeName, StateDict stateDict, int rightPrec)
        {
            RightPrecTransition(() => Transition(typeName, stateDict), rightPrec);
        }

        public void RightPrecTransition(TransitionFunc InnerTransition, int rightPrec)
        {
            InnerTransition();
            PushTree(PopTree().AttachRightPrec(rightPrec));
        }

        public void BothPrecTransition(string typeName, StateDict stateDict, int leftPrec, int rightPrec)
        {
            BothPrecTransition(() => Transition(typeName, stateDict), leftPrec, rightPrec);
        }

        public void BothPrecTransition(TransitionFunc innerTransition, int leftPrec, int rightPrec)
        {
            RightPrecTransition(() => LeftPrecTransition(innerTransition, leftPrec), rightPrec);
        }*//*
    }*/
}
