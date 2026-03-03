using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZevviCompiler.Arithmetic;
using ZevviCompiler.OperatorSyntaxes;
using ZevviCompiler.Transitions;
using ZevviCompiler.Variables;

namespace ZevviCompiler.MoreAssignment
{
    /// <summary>
    /// A Zevvi compiler extension with the arithmetic assignment operators (like '+=').<br/>
    /// Requirements: <see cref="OperatorSyntaxesExtension"/>, <see cref="VariablesExtension"/>, <see cref="ArithmeticExtension"/>.
    /// </summary>
    public class MoreAssignmentExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type>
        {
            typeof(OperatorSyntaxesExtension),
            typeof(VariablesExtension),
            typeof(ArithmeticExtension)
        };

        private OperatorSyntaxesExtension OpExt => Z.GetExtension<OperatorSyntaxesExtension>();
        
        private VariablesExtension VarExt => Z.GetExtension<VariablesExtension>();

        private ArithmeticExtension ArithExt => Z.GetExtension<ArithmeticExtension>();

        //public const int P_INCREMENT_L = 18000;

        //public OperatorOverloadDict canIncrement, canDecrement;

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
        }

        public override void InitSymbolTable()
        {
            DefineAssignmentOperator("+=", ArithExt.canAdd);
            DefineAssignmentOperator("-=", ArithExt.canSubtract);
            DefineAssignmentOperator("*=", ArithExt.canMultiply);
            DefineAssignmentOperator("/=", ArithExt.canDivide);
            DefineAssignmentOperator("%=", ArithExt.canModulo);
            DefineAssignmentOperator("&=", ArithExt.canBitwiseAnd);
            DefineAssignmentOperator("|=", ArithExt.canBitwiseOr);
            DefineAssignmentOperator("^=", ArithExt.canBitwiseXor);
            DefineAssignmentOperator("&&=", ArithExt.canBooleanAnd);
            DefineAssignmentOperator("||=", ArithExt.canBooleanOr);
            DefineAssignmentOperator("^^=", ArithExt.canBooleanXor);

            //OpExt.NewPostfix("++", P_INCREMENT_L, canIncrement);
            //OpExt.NewPostfix("--", P_INCREMENT_L, canDecrement);
        }

        public override void InitTransitions()
        {
        }

        public override void InitTypes()
        {
            //canIncrement = new OperatorOverloadDict(0)
            //{
            //    { new[] { Z.Int }, Z.Int, new ZI.Code(new SingleMI(MachineInstruction.PushCon, 1), MachineInstruction.Add) }
            //};

            //canDecrement = new OperatorOverloadDict(0)
            //{
            //    { new[] { Z.Int }, Z.Int, new ZI.Code(new SingleMI(MachineInstruction.PushCon, 1), MachineInstruction.Sub) }
            //};
        }

        // merge function for arithmetic assignment operators
        public ParseTree OperatorAndAssignmentMerge(ParseTree[] subtrees, string operatorName, OperatorOverloadDict opDict)
        {
            // the types of the operands
            IType[] types = new[] { subtrees[0].exprType, subtrees[2].exprType };

            // find the merge function of the non-assignment operator
            MergeFunc opMergeFunc = opDict.Get(types);

            // if the operands are invalid for the non-assignment operator then throw error
            if (opMergeFunc is null)
            {
                throw new ZevviTypeError($"Invalid argument types for operator '{operatorName}': " +
                    $"{string.Join<IType>(", ", types)}.", subtrees.GetTokensRange());
            }

            // merge the subtrees using the non-assignment operator's merge function
            ParseTree opTree = opMergeFunc(subtrees);

            // now try to assign the return type of the non-assignment operator to the left operand
            MergeFunc assignMergeFunc = VarExt.canAssign.Get(new[] { types[0], opTree.exprType });

            // if they are invalid for the assignment operator then throw error
            if (assignMergeFunc is null)
            {
                throw new ZevviTypeError($"Invalid argument types for operator '{operatorName}': " +
                    $"{string.Join<IType>(", ", types)}.", subtrees.GetTokensRange());
            }

            // copy the subtrees array and the parse trees
            ParseTree[] newSubtrees = subtrees.Select(tree => tree.Clone()).ToArray();

            // set the code of the right operand of the assignment operator to the code for the non-assignment operator
            newSubtrees[2].exprCode = opTree.exprCode;

            // merge the copied subtrees using the assignment operator's merge function
            ParseTree newTree = assignMergeFunc(newSubtrees);

            // return a parse tree whose subtrees are the original subtrees, but whose type and code are from the merged tree
            return new ParseTree(subtrees, newTree.exprType, newTree.exprCode, VarExt.nAssignment);
        }

        public void DefineAssignmentOperator(string name, OperatorOverloadDict opDict)
        {
            string qname = $"'{name}'";
            Z.symbolTable.Add(name, new OperatorType(Z, qname)
            {
                Transition = new BothPrecTransition(VariablesExtension.P_ASSIGNMENT_L, VariablesExtension.P_ASSIGNMENT_R, qname, OpExt.InfixDict),
                MergeSubtrees = subtrees => OperatorAndAssignmentMerge(subtrees, name, opDict),
            });
        }
    }
}
