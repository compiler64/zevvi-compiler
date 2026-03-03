using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.Transitions
{
    public class EofTransition : ITransition
    {
        public DefaultExtension Z { get; }

        public IStateDict StateDict { get; }

        private readonly ITransition thisTransition;

        public EofTransition(DefaultExtension z)
        {
            Z = z;
            StateDict = z.EofDict;
            thisTransition = new LeftPrecTransition(DefaultExtension.P_EOF, "Eof", z.EofDict);
        }

        public void Transition()
        {
            thisTransition.Transition();

            if (Z.PeekPrevTree.exprType is IdentifierType idType)
            {
                throw new ZevviUnknownIdentifierError(idType.Identifier, Z.PeekPrevTree.GetTokens());
            }

            Z.Merge();
        }
    }
}
