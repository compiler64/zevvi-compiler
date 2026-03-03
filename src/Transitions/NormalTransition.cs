using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.Transitions
{
    public class NormalTransition : ITransition
    {
        public readonly string typeName;

        public DefaultExtension Z { get; }

        public IStateDict StateDict { get; set; }

        public NormalTransition(string typeName, IStateDict stateDict)
        {
            this.typeName = typeName;
            StateDict = stateDict.Instantiate();
            Z = stateDict.Z;
        }

        public void Transition()
        {
            ParseTree currentTree = Z.PopTree();
            Z.currentTree = currentTree;

            while (true)
            {
                State oldState = Z.PeekTree.state;
                State newState = StateDict.Get(oldState);

                if (newState != State.Error)
                {
                    currentTree.state = newState;
                    Z.PushTree(currentTree);
                    return;
                }
                else if (oldState.canMerge)
                {
                    Z.Merge();
                    currentTree = currentTree.Reload();
                    Z.currentTree = currentTree;
                    Z.PushTreeAndTransition(currentTree);
                    //currentTree = Z.PopTree();
                    return;
                }
                else if (Z.PeekTree.exprType is IdentifierType idType) // unknown identifier error
                {
                    throw new ZevviUnknownIdentifierError(idType.Identifier, Z.PeekTree.GetTokens());
                }
                else if (currentTree.exprType is IdentifierType idType2) // unknown identifier error
                {
                    throw new ZevviUnknownIdentifierError(idType2.Identifier, currentTree.GetTokens());
                }
                else
                {
                    throw new ZevviSyntaxError($"Expression of type {typeName} is " +
                        $"not allowed at state {oldState}.", currentTree.GetTokens());
                }
            }
        }
    }
}
