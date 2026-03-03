using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZevviCompiler
{
    public class StateDict : IStateDict, IEnumerable, IEnumerable<StateFunc>
    {
        public DefaultExtension Z { get; }

        public IStateDict Prefab { get; }

        private readonly List<StateFunc> list;

        /*public StateFunc this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }*/

        public StateDict(DefaultExtension z, IStateDict prefab = null)
        {
            Z = z;
            Prefab = prefab;
            list = new();
        }

        private StateDict(DefaultExtension z, IStateDict prefab, List<StateFunc> list) : this(z, prefab)
        {
            this.list = list;
        }

        public IStateDict Instantiate()
        {
            return new StateDict(Z, this);
        }

        public State Get(State oldState)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                StateFunc func = list[i];
                State newState = func(oldState);

                if (newState != State.Null)
                {
                    return newState;
                }
            }

            return Prefab is null ? State.Error : Prefab.Get(oldState);
        }

        public IEnumerable<StateFunc> GetAll()
        {
            return list;
        }

        public void Add(StateFunc item)
        {
            list.Add(item);
        }

        public void Add(StateIndex key, State value)
        {
            Add((State state) => state.index == key ? value : State.Null);
        }

        public void Add(StateIndex key, Func<State> getValue)
        {
            Add((State state) => state.index == key ? getValue() : State.Null);
        }

        public void Add(StateIndex key, State value, Action<State> action)
        {
            Add((State state) =>
            {
                if (state.index == key)
                {
                    action(state);
                    return value;
                }
                else
                {
                    return State.Null;
                }
            });
        }

        public void Add(StateIndex key, Func<State> getValue, Action<State> action)
        {
            Add((State state) =>
            {
                if (state.index == key)
                {
                    action(state);
                    return getValue();
                }
                else
                {
                    return State.Null;
                }
            });
        }

        public void Add(StateIndex key, State value, Action action)
        {
            Add((State state) =>
            {
                if (state.index == key)
                {
                    action();
                    return value;
                }
                else
                {
                    return State.Null;
                }
            });
        }

        public void Add(StateIndex key, Func<State> getValue, Action action)
        {
            Add((State state) =>
            {
                if (state.index == key)
                {
                    action();
                    return getValue();
                }
                else
                {
                    return State.Null;
                }
            });
        }

        public void Add(ISet<StateIndex> keys, State value)
        {
            if (value == State.Null)
            {
                throw new ZevviInternalCompilerError("Do not add State.Null as a value in a StateDict.");
            }

            Add((State state) => keys.Contains(state.index) ? value : State.Null);
        }

        public void Add(ISet<StateIndex> keys, Func<State> getValue)
        {
            Add((State state) => keys.Contains(state.index) ? getValue() : State.Null);
        }

        public void Add(IDictionary<State, State> dict)
        {
            Add((State state) => dict.ContainsKey(state) ? dict[state] : State.Null);
        }

        public void AddAll(IEnumerable<StateFunc> stateFuncs)
        {
            foreach (StateFunc stateFunc in stateFuncs)
            {
                Add(stateFunc);
            }
        }

        public void Add(IStateDict stateDict)
        {
            AddAll(stateDict.GetAll());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator<StateFunc> IEnumerable<StateFunc>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /*IEnumerator<StateFunc> IEnumerable<StateFunc>.GetEnumerator()
        {
            return list.GetEnumerator();
        }*/

        public IStateDict Clone()
        {
            return new StateDict(Z, Prefab, list);
        }

        /*public static implicit operator StateFunc(StateDict dict)
        {
            return (State state) => dict.Get(state);
        }*/
    }
}
