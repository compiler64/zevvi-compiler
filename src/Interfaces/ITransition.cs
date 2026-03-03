using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler
{
    public interface ITransition
    {
        public DefaultExtension Z { get; }

        public IStateDict StateDict { get; }

        public void Transition();
    }
}
