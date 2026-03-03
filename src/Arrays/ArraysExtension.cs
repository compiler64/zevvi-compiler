using System;
using System.Collections.Generic;
using System.Text;
using ZevviCompiler.OperatorSyntaxes;
using ZevviCompiler.Punctuation;
using ZevviCompiler.Transitions;
using ZevviCompiler.Variables;
//using MI = ZevviCompiler.MachineInstruction;

namespace ZevviCompiler.Arrays
{
    /// <summary>
    /// A Zevvi compiler extension with array creation and indexing syntax.
    /// Contains the left and right square brackets.<br/>
    /// Requirements: <see cref="OperatorSyntaxesExtension"/>, <see cref="PunctuationExtension"/>, <see cref="VariablesExtension"/>.
    /// </summary>
    public class ArraysExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(OperatorSyntaxesExtension), typeof(PunctuationExtension), typeof(VariablesExtension) };

        private OperatorSyntaxesExtension OpExt => Z.GetExtension<OperatorSyntaxesExtension>();

        private PunctuationExtension PuncExt => Z.GetExtension<PunctuationExtension>();

        private VariablesExtension VarExt => Z.GetExtension<VariablesExtension>();

        public OperatorOverloadDict gettableItems;
        public OperatorOverloadDict settableItems;

        public IType ZArray;

        public StateDict LeftBracketDict, RightBracketDict;

        public StateIndex LeftBracket, LeftBracket_List, LeftBracket_List_Comma, LeftBracket_List_RightBracket;
        public StateIndex Normal_LeftBracket, Normal_LeftBracket_Normal, Normal_LeftBracket_Normal_RightBracket;
        public StateIndex Normal_LeftBracket_Normal_RightBracket_Assign, Normal_LeftBracket_Normal_RightBracket_Assign_Normal;
        public StateIndex Type_LeftBracket, Type_LeftBracket_RightBracket;
        public State sLeftBracket;
        public State sNormal_LeftBracket, sNormal_LeftBracket_Normal, sNormal_LeftBracket_Normal_RightBracket;
        public State sNormal_LeftBracket_Normal_RightBracket_Assign, sNormal_LeftBracket_Normal_RightBracket_Assign_Normal;
        public State sType_LeftBracket, sType_LeftBracket_RightBracket;
        public Nonterminal nArrayLiteral, nGetArrayItem, nSetArrayItem, nArrayType;

        public IConverter UnknownTypedArrayConverter;
        public IConverter ArrayConverter;

        public State SLeftBracket_List(int numToPop)
        {
            return new(LeftBracket_List, numToPop);
        }

        public State SLeftBracket_List_Comma(int numToPop)
        {
            return new(LeftBracket_List_Comma, numToPop);
        }

        public State SLeftBracket_List_RightBracket(int numToPop)
        {
            return new(LeftBracket_List_RightBracket, numToPop, subtrees => ArrayCreationMerge(subtrees));
        }

        public override void InitConverts()
        {
            ArrayConverter = new ZConverter()
            {
                AutoConvert = new ConvertDict()
                {
                    ArrayAutoConvert,
                    Z.WildcardOrSelfConvert,
                },
                ImplicitConvert = Z.VoidConvert,
                ExplicitConvert = Z.NoneConvert,
            };

            UnknownTypedArrayConverter = new ZConverter()
            {
                AutoConvert = Z.WildcardOrSelfConvert,
                ImplicitConvert = new ConvertDict()
                {
                    UnknownTypedArrayImplicitConvert,
                    Z.VoidConvert,
                },
                ExplicitConvert = Z.NoneConvert,
            };
        }

        public override void InitStates()
        {
            nArrayLiteral = new("ArrayLiteral");
            nGetArrayItem = new("GetArrayItem");
            nSetArrayItem = new("SetArrayItem");
            nArrayType = new("ArrayType");

            LeftBracket = new("LeftBracket");
            LeftBracket_List = new("LeftBracket_List");
            LeftBracket_List_Comma = new("LeftBracket_List_Comma");
            LeftBracket_List_RightBracket = new("LeftBracket_List_RightBracket");
            Normal_LeftBracket = new("Normal_LeftBracket");
            Normal_LeftBracket_Normal = new("Normal_LeftBracket_Normal");
            Normal_LeftBracket_Normal_RightBracket = new("Normal_LeftBracket_Normal_RightBracket");
            Normal_LeftBracket_Normal_RightBracket_Assign = new("Normal_LeftBracket_Normal_RightBracket_Assign");
            Normal_LeftBracket_Normal_RightBracket_Assign_Normal = new("Normal_LeftBracket_Normal_RightBracket_Assign_Normal");
            Type_LeftBracket = new("Type_LeftBracket");
            Type_LeftBracket_RightBracket = new("Type_LeftBracket_RightBracket");

            sLeftBracket = new(LeftBracket, 1);
            sNormal_LeftBracket = new(Normal_LeftBracket, 2);
            sNormal_LeftBracket_Normal = new(Normal_LeftBracket_Normal, 3);
            sNormal_LeftBracket_Normal_RightBracket = new(Normal_LeftBracket_Normal_RightBracket, 4,
                subtrees => OpExt.OperatorCheckTypes(subtrees, "[]", gettableItems, new HashSet<int> { 0, 2 }));
            sNormal_LeftBracket_Normal_RightBracket_Assign = new(Normal_LeftBracket_Normal_RightBracket_Assign, 5);
            sNormal_LeftBracket_Normal_RightBracket_Assign_Normal = new(Normal_LeftBracket_Normal_RightBracket_Assign_Normal, 6,
                subtrees => OpExt.OperatorCheckTypes(subtrees, "[]=", settableItems, new HashSet<int> { 0, 2, 5 }));
            sType_LeftBracket = new(Type_LeftBracket, 2);
            sType_LeftBracket_RightBracket = new(Type_LeftBracket_RightBracket, 3, subtrees => ArrayTypeMerge(subtrees));
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("[", ZType.WithVoidConverts(Z, "'['", new NormalTransition("'['", LeftBracketDict)));
            Z.symbolTable.Add("]", ZType.WithVoidConverts(Z, "']'", new NormalTransition("']'", RightBracketDict)));
        }

        public override void InitTransitions()
        {
            Z.normalStates.UnionWith(new[] { LeftBracket_List, Normal_LeftBracket_Normal });
            Z.operatorStates.UnionWith(new[] { LeftBracket, LeftBracket_List_Comma, Normal_LeftBracket });

            Z.NormalDict.Add(new StateDict(Z)
            {
                { LeftBracket, SLeftBracket_List(2) },
                { LeftBracket_List_Comma, () => SLeftBracket_List(Z.LastNumToPop + 1) },
                { Normal_LeftBracket, sNormal_LeftBracket_Normal },
                { Normal_LeftBracket_Normal_RightBracket_Assign, sNormal_LeftBracket_Normal_RightBracket_Assign_Normal }
            });

            LeftBracketDict = new StateDict(Z)
            {
                { Z.operatorStates, sLeftBracket },
                { Z.normalStates, sNormal_LeftBracket },
                { VarExt.Type, sType_LeftBracket },
            };

            RightBracketDict = new StateDict(Z)
            {
                { Type_LeftBracket, sType_LeftBracket_RightBracket },
                { LeftBracket_List, () => SLeftBracket_List_RightBracket(Z.LastNumToPop + 1) },
                { Normal_LeftBracket_Normal, sNormal_LeftBracket_Normal_RightBracket },
            };

            PuncExt.CommaDict.Add(LeftBracket_List, () => SLeftBracket_List_Comma(Z.LastNumToPop + 1));

            VarExt.AssignmentDict.Add(Normal_LeftBracket_Normal_RightBracket, sNormal_LeftBracket_Normal_RightBracket_Assign);
        }

        public override void InitTypes()
        {
            gettableItems = new OperatorOverloadDict(0, 2)
            {
                types =>
                {
                    // get the item type of the array
                    IType itemType = ItemTypeOf(types[0], out ZI.CodeFunc arrayConvert);

                    if (itemType is null)
                    {
                        return null;
                    }

                    // try to convert the index to Int
                    ZI.CodeFunc indexConvert = types[1].AutoConvert(Z.Int);

                    if (indexConvert is null)
                    {
                        return null;
                    }

                    // return a get-item merge function
                    return subtrees => GetItemMerge(subtrees, itemType, arrayConvert, indexConvert);
                }
            };

            settableItems = new OperatorOverloadDict(0, 2, 5)
            {
                types =>
                {
                    // get the item type of the array
                    IType itemType = ItemTypeOf(types[0], out ZI.CodeFunc arrayConvert);

                    if (itemType is null)
                    {
                        return null;
                    }

                    // try to convert the index to Int
                    ZI.CodeFunc indexConvert = types[1].AutoConvert(Z.Int);
                    // try to convert the value to the item type
                    ZI.CodeFunc valueConvert = types[2].ImplicitConvert(itemType);

                    if (indexConvert is null || valueConvert is null)
                    {
                        return null;
                    }

                    // return a set-item merge function if the value type can implicitly convert to the item type
                    return subtrees => SetItemMerge(subtrees, arrayConvert, indexConvert, valueConvert);
                }
            };
        }

        public IType ItemTypeOf(IType arrayType, out ZI.CodeFunc convert)
        {
            // an empty wildcard type '?'
            WildcardType wildcard = new(Z, "?", type => true);
            // a wildcard array type '?[]'
            ArrayType wildcardArray = new(this, wildcard);
            // the implicit convert function from the array type to the wildcard array type
            convert = arrayType.ImplicitConvert(wildcardArray);

            // if the convert failed
            if (convert is null)
            {
                return null;
            }

            // return the type that the wildcard type captured
            return wildcard.Capture;
        }

        public ParseTree ArrayCreationMerge(ParseTree[] subtrees)
        {
            UnknownTypedArrayType unknown = UnknownTypedArrayType.ArrayCreation(this, subtrees);
            return new ParseTree(subtrees, unknown, ZI.Code.None, nArrayLiteral);
        }

#pragma warning disable CA1822 // Mark members as static
        public ParseTree GetItemMerge(ParseTree[] subtrees, IType itemType, ZI.CodeFunc arrayConvert, ZI.CodeFunc indexConvert)
        {
            ParseTree arrayTree = subtrees[0];
            ParseTree indexTree = subtrees[2];
            // convert code for array
            ZI.Code arrayCode = arrayTree.ConvertedWith(arrayConvert);
            // convert code for index
            ZI.Code indexCode = indexTree.ConvertedWith(indexConvert);
            // code for array[index]
            ZI.Code getItemCode = ArraysCode.GetItem(arrayCode.storage, indexCode.storage);
            // the ZI code for the whole expression
            ZI.Code code = arrayCode + indexCode + getItemCode;
            // return a new parse tree
            return ParseTree.CombineTrees(subtrees, itemType, code, nGetArrayItem);
        }

        public ParseTree SetItemMerge(ParseTree[] subtrees, ZI.CodeFunc arrayConvert, ZI.CodeFunc indexConvert, ZI.CodeFunc valueConvert)
        {
            ParseTree arrayTree = subtrees[0];
            ParseTree indexTree = subtrees[2];
            ParseTree valueTree = subtrees[5];
            // convert code for array
            ZI.Code arrayCode = arrayTree.ConvertedWith(arrayConvert);
            // convert code for index
            ZI.Code indexCode = indexTree.ConvertedWith(indexConvert);
            // convert code for value
            ZI.Code valueCode = valueTree.ConvertedWith(valueConvert);
            // code for array[index] = value
            ZI.Code setItemCode = ArraysCode.SetItem(arrayCode.storage, indexCode.storage, valueCode.storage);
            // the ZI code for the whole expression
            ZI.Code code = arrayCode + indexCode + valueCode + setItemCode;
            // return a new parse tree
            return ParseTree.CombineTrees(subtrees, Z.Void, code, nSetArrayItem);
        }

        public ParseTree ArrayTypeMerge(ParseTree[] subtrees)
        {
            // the type keyword that is before the "[]"
            IType typeKeyword = subtrees[0].exprType.As<TypeType>().innerType;

            // return a new parse tree which acts like a type keyword for an array type
            return new ParseTree(subtrees, new TypeType(VarExt, new ArrayType(this, typeKeyword)), ZI.Code.None, nArrayType);
        }

        public ZI.CodeFunc UnknownTypedArrayImplicitConvert(IType oldType, IType newType)
        {
            // newType must be an array type for the convert to be successful
            if (newType is not ArrayType arrayType)
            {
                return null;
            }

            // get the UnknownTypedArrayType that is being converted
            UnknownTypedArrayType unknown = oldType.As<UnknownTypedArrayType>();
            // the code to convert the initial values to the item type of the new array type
            ZI.Code itemConvertCodes = ZI.Code.None;
            // the storages of the initial values
            ZI.IOperand[] initialItems = new ZI.IOperand[unknown.itemTypes.Length];

            // for each item type, try to convert it to the item type of the new array type
            for (int i = 0; i < unknown.itemTypes.Length; i++)
            {
                IType itemType = unknown.itemTypes[i];
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
        }

        public ZI.CodeFunc ArrayAutoConvert(IType oldType, IType newType)
        {
            ArrayType arrayType = oldType.As<ArrayType>();

            return newType is ArrayType newArrayType
                && newArrayType.itemType is WildcardType wildcardType
                ? wildcardType.TryCapture(arrayType.itemType) : null;
        }
#pragma warning restore CA1822
    }
}
