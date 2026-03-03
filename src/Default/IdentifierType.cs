using System;
using ZevviCompiler.Transitions;

namespace ZevviCompiler
{
    public class IdentifierType : ZType, IIdentifierType
    {
        public string Identifier { get; }

        public IdentifierType(DefaultExtension z, string name, string identifier)
            : base(z, name)
        {
            Identifier = identifier;
            Transition = new NormalTransition(name, z.NormalDict);
        }

        public override bool Equals(object obj)
        {
            return obj is IdentifierType type &&
                   Identifier == type.Identifier;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identifier);
        }
    }
}
