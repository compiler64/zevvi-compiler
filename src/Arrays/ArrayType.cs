using System;
using System.Collections.Generic;
using System.Text;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.Arrays
{
    public class ArrayType : ZType
    {
        /*public static readonly ConvertDict ArrayConvert = new()
        {
            (oldType, newType) =>
            {
                ArrayType arrayType = oldType.As<ArrayType>();
                return newType is ArrayType newArrayType
                    && newArrayType.itemType is WildcardType wildcardType
                    ? wildcardType.TryCapture(arrayType.itemType) : null;
            },
            Converts.WildcardOrSelf
        };*/

        public readonly ArraysExtension a;
        public readonly IType itemType;

        public ArrayType(ArraysExtension a, IType itemType) : base(a.Z, $"{itemType}[]")
        {
            Transition = new NormalTransition($"{itemType}[]", a.Z.NormalDict);
            Converter = a.ArrayConverter;
            this.a = a;
            this.itemType = itemType;
        }
    }
}
