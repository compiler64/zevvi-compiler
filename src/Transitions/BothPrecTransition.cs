using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.Transitions
{
    public class BothPrecTransition : ITransition
    {
        public DefaultExtension Z { get; }

        public IStateDict StateDict => innerTransition.StateDict;

        public readonly ITransition innerTransition;

        public readonly int leftPrec;

        public readonly int rightPrec;

        private readonly ITransition thisTransition;

        public BothPrecTransition(int leftPrec, int rightPrec, ITransition innerTransition)
        {
            this.leftPrec = leftPrec;
            this.rightPrec = rightPrec;
            this.innerTransition = innerTransition;
            Z = innerTransition.Z;
            thisTransition = new RightPrecTransition(rightPrec, new LeftPrecTransition(leftPrec, innerTransition));
        }

        public BothPrecTransition(int leftPrec, int rightPrec, string typeName, IStateDict stateDict)
            : this(leftPrec, rightPrec, new NormalTransition(typeName, stateDict))
        {
        }

        public void Transition()
        {
            thisTransition.Transition();
        }
    }
}
