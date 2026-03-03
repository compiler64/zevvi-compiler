using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler
{
    /// <summary>
    /// An abstract class for creating Zevvi compiler extensions.
    /// Implements ZevviCompiler.ICompilerExtension.
    /// </summary>
    public abstract class CompilerExtension : ICompilerExtension
    {
        public DefaultExtension Z { get; set; }

        public virtual void InitOther()
        {
        }

        public abstract void InitConverts();

        public abstract void InitStates();

        public abstract void InitSymbolTable();

        public abstract void InitTransitions();

        public abstract void InitTypes();

        public abstract ISet<Type> RequiredExtensions { get; }
    }
}
