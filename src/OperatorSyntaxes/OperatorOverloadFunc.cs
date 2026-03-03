using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler.OperatorSyntaxes
{
    public delegate MergeFunc OperatorOverloadFunc(IList<IType> types);
}
