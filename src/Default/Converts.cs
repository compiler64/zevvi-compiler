using System;
using System.Collections.Generic;

namespace ZevviCompiler
{
    [Obsolete("Use the convert fields in DefaultExtension instead (file DefaultConverts).", error: true)]
    public static class Converts
    {
        public static ConvertDict None => new();

        public static ConvertDict Self => new()
        {
            (oldType) => new Dictionary<IType, ZI.CodeFunc>
            {
                [oldType] = storage => ZI.Code.Operand(storage)
            }
        };

        public static ConvertDict Wildcard => new()
        {
            (oldType, newType) => newType is WildcardType wildcardType ? wildcardType.TryCapture(oldType) : null
        };

        public static ConvertDict WildcardOrSelf => new()
        {
            Wildcard,
            Self
        };

        public static ConvertDict Void => new()
        {
            (oldType) => new Dictionary<IType, ZI.CodeFunc>
            {
                [oldType.Z.Void] = storage => ZI.Code.None
            }
        };

        public static ConvertDict VoidOrSelf => new()
        {
            Void,
            Self
        };

        [Obsolete("With the new ZI update, Converts.VoidLike is exactly the same as Converts.VoidOrSelf.")]
        public static readonly ConvertDict VoidLike = new()
        {
            Void,
            Self
        };
    }
}
