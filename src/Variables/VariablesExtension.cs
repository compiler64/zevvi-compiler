using System;
using System.Collections.Generic;
using System.Text;
using ZevviCompiler.OperatorSyntaxes;
using ZevviCompiler.FunctionDefinitions;

namespace ZevviCompiler.Variables
{
    /// <summary>
    /// A Zevvi compiler extension that adds variables, variable declaration and assignment syntax,
    /// and type keywords (like 'int' and 'bool').<br/>
    /// Requirements: <see cref="OperatorSyntaxesExtension"/>.
    /// </summary>
    public class VariablesExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(OperatorSyntaxesExtension) };

        private OperatorSyntaxesExtension OpExt => Z.GetExtension<OperatorSyntaxesExtension>();

        public const int P_ASSIGNMENT_L = 4000;
        public const int P_ASSIGNMENT_R = 4001;
        public const int P_TYPE_R = 5000;

        public TypeType VoidType, IntType, BoolType, FloatType, CharType, StringType;

        public OperatorOverloadDict canAssign;
        public OperatorOverloadDict declarationMergeFuncs;

        public IStateDict TypeDict;
        public IStateDict AssignmentDict;

        public StateIndex Type, Type_Normal;
        public State sType, sType_Normal;
        public Nonterminal nDeclaration, nAssignment;

        public IConverter VariableConverter;

        public override void InitConverts()
        {
            VariableConverter = new ZConverter()
            {
                AutoConvert = new ConvertDict()
                {
                    VariableAutoConvert,
                    Z.SelfConvert,
                },
                ImplicitConvert = new ConvertDict()
                {
                    VariableImplicitConvert,
                    Z.VoidConvert,
                },
                ExplicitConvert = new ConvertDict()
                {
                    VariableExplicitConvert,
                },
            };
        }

        public override void InitStates()
        {
            nDeclaration = new("Declaration");
            nAssignment = new("Assignment");

            Type = new("Type");
            Type_Normal = new("Type_Normal");

            sType = new(Type, 1);
            sType_Normal = new(Type_Normal, 2, subtrees => declarationMergeFuncs.Get(Array.Empty<IType>())(subtrees));
        }

        public override void InitSymbolTable()
        {
            /*Z.symbolTable.Add("=", new OperatorType(Z, "'='", new BothPrecTransition("'='",
                OpExt.InfixDict.Clone(), P_ASSIGNMENT_L, P_ASSIGNMENT_R),
                subtrees => OpExt.OperatorCheckTypes(subtrees, "=", canAssign, new HashSet<int> { 0, 2 })));*/
            OpExt.NewInfix("=", P_ASSIGNMENT_L, P_ASSIGNMENT_R, canAssign);
            AssignmentDict = Z.symbolTable.Get("=").type.Transition.StateDict;

            Z.symbolTable.Add("void", VoidType);
            Z.symbolTable.Add("int", IntType);
            Z.symbolTable.Add("bool", BoolType);
            Z.symbolTable.Add("float", FloatType);
            Z.symbolTable.Add("char", CharType);
            Z.symbolTable.Add("string", StringType);
        }

        public override void InitTransitions()
        {
            Z.normalStates.Add(Type_Normal);
            Z.operatorStates.Add(Type);

            Z.NormalDict.Add(Type, sType_Normal);

            TypeDict = new StateDict(Z)
            {
                { Z.operatorStates, sType }
            };
        }

        public override void InitTypes()
        {
            VoidType = new TypeType(this, Z.Void);
            IntType = new TypeType(this, Z.Int);
            BoolType = new TypeType(this, Z.Bool);
            FloatType = new TypeType(this, Z.Float);
            CharType = new TypeType(this, Z.Char);
            StringType = new TypeType(this, Z.String);

            canAssign = new OperatorOverloadDict(0, 2)
            {
                { types => types[0] is PropertyType propType ? propType.setter : null }
            };

            declarationMergeFuncs = new OperatorOverloadDict()
            {
                { _ => subtrees => DeclarationMerge(subtrees, Z.symbolTable.NextVarLoc) }
            };
        }

        public ParseTree DeclarationMerge(ParseTree[] subtrees, ZI.Variable storage)
        {
            // the type keyword used to declare the variable
            TypeType typeType = subtrees[0].exprType.As<TypeType>();
            // the type that the type keyword represents
            IType innerType = typeType.innerType;
            // the variable name
            string id = subtrees[1].GetIdentifier(type => $"Expected identifier for variable name, got {type}.");
            // the ZI operand that stores the variable
            //ZI.Variable storage = Z.symbolTable.NextVarLoc;
            // the variable type
            VariableType varType = new(this, id, innerType, storage);
            // add the variable to the symbol table
            Z.symbolTable.Add(id, varType, storage);
            // return the new parse tree
            return ParseTree.CombineTrees(subtrees, varType, ZI.Code.None, nDeclaration);
        }

        public ParseTree AssignmentMerge(ParseTree[] subtrees)
        {
            VariableType varType = subtrees[0].exprType.As<VariableType>();
            return ParseTree.CombineTrees(subtrees, new HashSet<int> { 2 }, new[] { varType.innerType }, Z.Void,
                storages => new ZI.Triple(ZI.Operator.Copy, varType.storage, storages[0]), nAssignment);
        }

#pragma warning disable CA1822
        public ZI.CodeFunc VariableAutoConvert(IType oldType, IType newType)
        {
            VariableType varType = oldType.As<VariableType>();
            ZI.CodeFunc convert = varType.innerType.Converter.AutoConvert.Get(varType.innerType, newType);
            return convert;
        }
        
        public ZI.CodeFunc VariableImplicitConvert(IType oldType, IType newType)
        {
            VariableType varType = oldType.As<VariableType>();
            ZI.CodeFunc convert = varType.innerType.Converter.ImplicitConvert.Get(varType.innerType, newType);
            return convert;
        }
        
        public ZI.CodeFunc VariableExplicitConvert(IType oldType, IType newType)
        {
            VariableType varType = oldType.As<VariableType>();
            ZI.CodeFunc convert = varType.innerType.Converter.ExplicitConvert.Get(varType.innerType, newType);
            return convert;
        }
#pragma warning restore CA1822
    }
}
