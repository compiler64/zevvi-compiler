using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZevviCompiler.FunctionDefinitions;

namespace ZevviCompiler.SyntaxModification
{
    public struct SyntaxDefinition
    {
        public string name;
        public IList<string> identifiers;
        public IList<StateIndex> stateIndices;
        public IList<State> states;
        public IList<IStateDict> stateDicts;
        public Nonterminal nonterminal;
        public IList<int> paramIndices;
        public FunctionType function;

        public SyntaxDefinition(string name, IList<string> identifiers, List<StateIndex> stateIndices,
            IList<State> states, IList<IStateDict> stateDicts, Nonterminal nonterminal,
            IList<int> paramIndices, FunctionType function)
        {
            this.name = name;
            this.identifiers = identifiers;
            this.stateIndices = stateIndices;
            this.states = states;
            this.stateDicts = stateDicts;
            this.nonterminal = nonterminal;
            this.paramIndices = paramIndices;
            this.function = function;
        }
    }
}
