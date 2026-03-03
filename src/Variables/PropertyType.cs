using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.Variables
{
    public class PropertyType : ZType
    {
        public readonly MergeFunc setter;

        public bool CanGet { get; }

        public bool CanSet => setter is not null;

        public PropertyType(DefaultExtension z, string name, bool canGet, MergeFunc setter)
            : base(z, name)
        {
            Transition = new NormalTransition(name, Z.NormalDict);
            CanGet = canGet;
            this.setter = setter;
        }
    }
}
