using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.ZI
{
    /// <summary>
    /// A function that represents a Zevvi function or operation with one argument, e.g. a convert function when the types are already known.
    /// Equivalent to Func&lt;<see cref="IOperand"/>, <see cref="Code"/>&gt;.
    /// </summary>
    /// <param name="storage">The ZI operand that stores the argument</param>
    /// <returns>The code needed to do the operation on <paramref name="storage"/>.</returns>
    public delegate Code CodeFunc(IOperand storage);

    /// <summary>
    /// A function that represents a Zevvi function or operation.
    /// For example, the addition operator is this CodeMultiFunc:
    /// <code>storages => new ZI.Triple(ZI.Operator.Add, storages[0], storages[1])</code>
    /// Equivalent to Func&lt;<see cref="IOperand"/>[], <see cref="Code"/>&gt;
    /// </summary>
    /// <param name="storages">The ZI operands that store the arguments</param>
    /// <returns>The code needed to do the operation on <paramref name="storages"/>.</returns>
    public delegate Code CodeMultiFunc(IOperand[] storages);
}
