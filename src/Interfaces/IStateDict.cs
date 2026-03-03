using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler
{
    public interface IStateDict
    {
        public DefaultExtension Z { get; }

        public IStateDict Prefab { get; }

        public IStateDict Instantiate();

        public State Get(State oldState);

        public IEnumerable<StateFunc> GetAll();

        public void Add(StateFunc stateFunc);

        public void Add(IStateDict stateDict);

        public void Add(StateIndex key, State value);

        public void Add(StateIndex key, Func<State> getValue);

        public void Add(StateIndex key, State value, Action<State> action);

        public void Add(StateIndex key, Func<State> getValue, Action<State> action);

        public void Add(StateIndex key, State value, Action action);

        public void Add(StateIndex key, Func<State> getValue, Action action);

        public void Add(ISet<StateIndex> keys, State value);

        public void Add(ISet<StateIndex> keys, Func<State> getValue);

        public void Add(IDictionary<State, State> dict);

        public void AddAll(IEnumerable<StateFunc> stateFuncs);

        public IStateDict Clone();
    }
}
