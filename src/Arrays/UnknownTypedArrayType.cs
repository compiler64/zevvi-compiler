using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.Arrays
{
    public class UnknownTypedArrayType : ZType
    {
        /*public static readonly ConvertDict UnknownTypedArrayConvert = new()
        {
            (oldType, newType) =>
            {
                // newType must be an array type for the convert to be successful
                if (newType is not ArrayType arrayType)
                {
                    return null;
                }

                // get the UnknownTypedArrayType that is being converted
                UnknownTypedArrayType unknown = oldType.As<UnknownTypedArrayType>();
                //ZI.Code[] codesWithConverts = new ZI.Code[unknown.elementTypes.Length]; 
                // the code to convert the initial values to the item type of the new array type
                ZI.Code itemConvertCodes = ZI.Code.None;
                // the storages of the initial values
                ZI.IOperand[] initialItems = new ZI.IOperand[unknown.itemTypes.Length];

                // for each item type, try to convert it to the item type of the new array type
                for (int i = 0; i < unknown.itemTypes.Length; i++)
                {
                    ZType itemType = unknown.itemTypes[i];
                    ZI.IOperand itemStorage = unknown.itemCodes[i].storage;
                    ZI.CodeFunc convert = itemType.ImplicitConvert(arrayType.itemType);

                    if (convert is null)
                    {
                        return null;
                    }

                    ZI.Code itemCodeWithConvert = unknown.itemCodes[i] + convert(itemStorage);

                    //codesWithConverts[i] = unknown.elementCodes[i] + convert(elementStorage);
                    itemConvertCodes += itemCodeWithConvert;
                    initialItems[i] = itemCodeWithConvert.storage;
                }

                ZI.Code code = itemConvertCodes + ArraysCode.CreateAndInitialize(initialItems);

                //return unknown.a.CreateArrayCode(codesWithConverts);
                return _ => code;
            },
            Converts.Void
        };*/

        public readonly ArraysExtension a;
        public readonly IType[] itemTypes;
        public readonly ZI.Code[] itemCodes;

        private static string GetTypeName(IType[] itemTypes)
        {
            return "[" + string.Join<IType>(", ", itemTypes) + "]";
        }

        public UnknownTypedArrayType(ArraysExtension a, IType[] itemTypes, ZI.Code[] itemCodes)
            : base(a.Z, GetTypeName(itemTypes))
        {
            Transition = new NormalTransition(Name, Z.NormalDict);
            Converter = a.UnknownTypedArrayConverter;
            this.a = a;
            this.itemTypes = itemTypes;
            this.itemCodes = itemCodes;
        }

        public static UnknownTypedArrayType ArrayCreation(ArraysExtension a, ParseTree[] subtrees)
        {
            IEnumerable<ParseTree> elementTrees = subtrees.Where((_, index) => index % 2 == 1);
            IType[] elementTypes = elementTrees.Select(tree => tree.exprType).ToArray();
            ZI.Code[] elementCodes = elementTrees.Select(tree => tree.exprCode).ToArray();
            return new UnknownTypedArrayType(a, elementTypes, elementCodes);
        }
    }
}
