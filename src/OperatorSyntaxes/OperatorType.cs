using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler.OperatorSyntaxes
{
    public class OperatorType : ZType
    {
        public MergeFunc MergeSubtrees { get; init; }

        public OperatorType(DefaultExtension z, string name) : base(z, name)
        {
        }
    }
}
