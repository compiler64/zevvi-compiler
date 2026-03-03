using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler
{
    /// <summary>
    /// A Zevvi compiler extension.
    /// Extensions must define four no-parameter methods that return void:
    /// <list type="bullet">
    /// <item>InitStates - Initializes the <see cref="StateIndex"/> and <see cref="State"/> objects used by this extension.</item>
    /// <item>InitSymbolTable - Adds Zevvi operators in this extension to the symbol table,
    /// or modifies existing items in the symbol table.</item>
    /// <item>InitTransitions - Initializes the <see cref="StateDict"/> objects used by this extension,
    /// modifies existing <see cref="StateDict"/> objects from other extensions, and/or adds state indices to the
    /// <see cref="DefaultExtension.normalStates"/> and <see cref="DefaultExtension.operatorStates"/> sets.</item>
    /// <item>InitTypes - Initializes the Zevvi types (<see cref="ZType"/> objects) used by this extension.</item>
    /// </list>
    /// </summary>
    public interface ICompilerExtension
    {
        public static readonly ISet<Type> noRequirements = new HashSet<Type> { };

        public DefaultExtension Z { get; set; }

        public void Initialize()
        {
            InitStates();
            InitTransitions();
            InitConverts();
            InitTypes();
            InitSymbolTable();
            InitOther();
        }

        public void InitConverts();

        public void InitOther();

        public void InitStates();

        public void InitSymbolTable();

        public void InitTransitions();

        public void InitTypes();

        public ISet<Type> RequiredExtensions { get; }
    }
}
