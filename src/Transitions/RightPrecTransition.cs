using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.Transitions
{
    public class RightPrecTransition : ITransition
    {
        public DefaultExtension Z { get; }

        public readonly ITransition innerTransition;

        public readonly int rightPrec;

        public IStateDict StateDict => innerTransition.StateDict;

        public RightPrecTransition(int rightPrec, ITransition innerTransition)
        {
            this.rightPrec = rightPrec;
            this.innerTransition = innerTransition;
            Z = innerTransition.Z;
        }

        public RightPrecTransition(int rightPrec, string typeName, IStateDict stateDict)
            : this(rightPrec, new NormalTransition(typeName, stateDict))
        {
        }

        public void Transition()
        {
            innerTransition.Transition();

            if (Z.PeekTree is not RightPrecTree)
            {
                Z.PushTree(Z.PopTree().AttachRightPrec(rightPrec));
            }
        }
    }
}
