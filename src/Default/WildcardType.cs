using System;
using System.Collections.Generic;
using System.Text;
using ZevviCompiler.Transitions;

namespace ZevviCompiler
{
    public class WildcardType : ZType
    {
        public Func<IType, bool> Predicate { get; init; }

        public IType Capture { get; private set; }

        public WildcardType(DefaultExtension z, string name, Func<IType, bool> predicate)
            : base(z, name)
        {
            Transition = new NormalTransition(Name, Z.NormalDict);
            Predicate = predicate;
        }

        public ZI.CodeFunc TryCapture(IType type)
        {
            if (Predicate(type))
            {
                Capture = type;
                return storage => ZI.Code.None;
            }
            else
            {
                return null;
            }
        }
    }
}
