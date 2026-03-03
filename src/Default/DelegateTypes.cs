using System;
using System.Collections.Generic;

namespace ZevviCompiler
{
    /// <summary>
    /// An entry in a <see cref="StateDict"/>.<br/>
    /// Equivalent to Func&lt;<see cref="State"/>, <see cref="State"/>&gt;.
    /// </summary>
    /// <param name="oldState">The old state.</param>
    /// <returns>The new state, or <see langword="null"/> if this entry is not applicable in the old state.</returns>
    public delegate State StateFunc(State oldState);
    
    /// <summary>
    /// A function that moves the compiler to the next state.<br/>
    /// Equivalent to Action.
    /// </summary>
    [Obsolete("Use the new transition system in the ZevviCompiler.Transitions namespace.")]
    public delegate void TransitionFunc();

    /// <summary>
    /// A function that combines subtrees into a parse tree.
    /// Each state that allows merging has a MergeFunc.<br/>
    /// Equivalent to Func&lt;<see cref="ParseTree"/>[], <see cref="ParseTree"/>&gt;.
    /// </summary>
    /// <param name="subtrees">The subtrees array.</param>
    /// <returns>A <see cref="ParseTree"/> whose subtrees are the elements of the <paramref name="subtrees"/> array.</returns>
    public delegate ParseTree MergeFunc(ParseTree[] subtrees);

    /// <summary>
    /// A function that returns the code function needed to convert an object of type <paramref name="oldType"/> to <paramref name="newType"/>.
    /// This function would belong to <paramref name="oldType"/> in the field <see cref="ZType.ImplicitConverter"/> or the field <see cref="ZType.ExplicitConverter"/>.<br/>
    /// Equivalent to Func&lt;<see cref="IType"/>, <see cref="IType"/>, <see cref="ZI.IOperand"/>, <see cref="ZI.Code"/>&gt;.
    /// </summary>
    /// <param name="oldType">The original type of the object.</param>
    /// <param name="newType">The new type for the object.</param>
    /// <returns>The code function needed to convert an object of type <paramref name="oldType"/> to <paramref name="newType"/>, or <see langword="null"/> if the convert is invalid.</returns>
    public delegate ZI.CodeFunc ConvertFunc(IType oldType, IType newType);
}
