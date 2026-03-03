using System.Collections.Generic;

namespace ZevviCompiler
{
    public class SymbolTable
    {
        public readonly Stack<Scope> scopes = new();

        public bool IsEmpty => !scopes.TryPeek(out _);

        public Scope InnerScope => scopes.Peek();

        public void Clear()
        {
            scopes.Clear();
        }

        public void PushScope()
        {
            scopes.Push(new Scope(scopes.Count > 0 ? scopes.Peek().nextVarLoc : 0));
        }

        public Scope PopScope()
        {
            return scopes.Pop();
        }

        public SymbolTableEntry Get(string identifier)
        {
            foreach (Scope scope in scopes)
            {
                if (scope.entries.ContainsKey(identifier))
                {
                    return scope.entries[identifier];
                }
            }

            return null;
        }

        public void Add(string identifier, IType type)
        {
            scopes.Peek().entries[identifier] = new SymbolTableEntry(identifier, type);
        }

        public void Add(string identifier, IType type, ZI.IOperand storage)
        {
            scopes.Peek().entries[identifier] = new SymbolTableEntry(identifier, type, storage);
        }

        public bool Remove(string identifier)
        {
            foreach (Scope scope in scopes)
            {
                if (scope.entries.ContainsKey(identifier))
                {
                    scope.entries.Remove(identifier);
                    return true;
                }
            }

            return false;
        }

        public ZI.Variable NextVarLoc => new(InnerScope.nextVarLoc++);
    }
}
