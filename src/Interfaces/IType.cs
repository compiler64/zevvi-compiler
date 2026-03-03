using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler
{
    public interface IType
    {
        public DefaultExtension Z { get; }

        public string Name { get; }

        public ITransition Transition { get; set; }

        public IConverter Converter { get; }

        public sealed MetaType As<MetaType>(Func<IType, Exception> throwIfFail = null)
        {
            return this is MetaType type ? type : throw throwIfFail?.Invoke(this)
                ?? new ZevviInternalCompilerError($"Type {this} is not a {typeof(MetaType)}.");
        }

        public ZI.CodeFunc AutoConvert(IType newType);

        public ZI.CodeFunc ImplicitConvert(IType newType);

        public ZI.CodeFunc ExplicitConvert(IType newType);

        public static bool Eq(IType left, IType right)
        {
            return left is null ? throw new ArgumentNullException(nameof(left))
                : right is null ? throw new ArgumentNullException(nameof(right))
                : left.Equals(right);
        }
    }
}
