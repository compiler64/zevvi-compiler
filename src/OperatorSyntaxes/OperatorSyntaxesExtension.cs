using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZevviCompiler.Transitions;
using MyLibraries.UsefulMethods;

namespace ZevviCompiler.OperatorSyntaxes
{
    /// <summary>
    /// A Zevvi compiler extension with the syntaxes for the following operator types:<br/>
    /// <list type="bullet">
    ///     <item>binary infix</item>
    ///     <item>unary prefix</item>
    ///     <item>unary postfix</item>
    ///     <item>standalone (no operands)</item>
    ///     <item>combinations of the above (for example, the '+' operator can be binary infix or unary prefix).</item>
    /// </list>
    /// Requirements: None.
    /// </summary>
    public class OperatorSyntaxesExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type>();

        public StateDict InfixDict, PrefixDict, PostfixDict, PrefixOrInfixDict, PostfixOrInfixDict, StandaloneDict, StandaloneOrPrefixDict, StandaloneOrPostfixDict;

        public StateIndex Normal_Infix, Normal_Infix_Normal, Prefix, Prefix_Normal, Normal_Postfix;
        public StateIndex PrefixOrInfix, Normal_PrefixOrInfix, PrefixOrInfix_Normal, Normal_PrefixOrInfix_Normal;
        public StateIndex Normal_PostfixOrInfix, Normal_PostfixOrInfix_Normal;
        public StateIndex Standalone, StandaloneOrPrefix, StandaloneOrPrefix_Normal, StandaloneOrPostfix, Normal_StandaloneOrPostfix;
        public State sNormal_Infix, sNormal_Infix_Normal, sPrefix, sPrefix_Normal, sNormal_Postfix;
        public State sPrefixOrInfix, sNormal_PrefixOrInfix, sPrefixOrInfix_Normal, sNormal_PrefixOrInfix_Normal;
        public State sNormal_PostfixOrInfix, sNormal_PostfixOrInfix_Normal;
        public State sStandalone, sStandaloneOrPrefix, sStandaloneOrPrefix_Normal, sStandaloneOrPostfix, sNormal_StandaloneOrPostfix;
        public Nonterminal nStandaloneOperator, nPrefixOperator, nPostfixOperator, nInfixOperator;

        public event Action OnInfixSecondArgumentParsed;

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
            nStandaloneOperator = new("StandaloneOperator");
            nPrefixOperator = new("PrefixOperator");
            nPostfixOperator = new("PostfixOperator");
            nInfixOperator = new("InfixOperator");

            Normal_Infix = new("Normal_Infix");
            Normal_Infix_Normal = new("Normal_Infix_Normal");
            Prefix = new("Prefix");
            Prefix_Normal = new("Prefix_Normal");
            Normal_Postfix = new("Normal_Postfix");
            PrefixOrInfix = new("PrefixOrInfix");
            Normal_PrefixOrInfix = new("Normal_PrefixOrInfix");
            PrefixOrInfix_Normal = new("PrefixOrInfix_Normal");
            Normal_PrefixOrInfix_Normal = new("Normal_PrefixOrInfix_Normal");
            Normal_PostfixOrInfix = new("Normal_PostfixOrInfix");
            Normal_PostfixOrInfix_Normal = new("Normal_PostfixOrInfix_Normal");
            Standalone = new("Standalone");
            StandaloneOrPrefix = new("StandaloneOrPrefix");
            StandaloneOrPrefix_Normal = new("StandaloneOrPrefix_Normal");
            StandaloneOrPostfix = new("StandaloneOrPostfix");
            Normal_StandaloneOrPostfix = new("Normal_StandaloneOrPostfix");

            sNormal_Infix = new(Normal_Infix, 2);
            sNormal_Infix_Normal = new(Normal_Infix_Normal, 3, OpTypeMerge(1));
            sPrefix = new(Prefix, 1);
            sPrefix_Normal = new(Prefix_Normal, 2, OpTypeMerge(0));
            sNormal_Postfix = new(Normal_Postfix, 2, OpTypeMerge(1));
            sPrefixOrInfix = new(PrefixOrInfix, 1);
            sNormal_PrefixOrInfix = new(Normal_PrefixOrInfix, 2);
            sPrefixOrInfix_Normal = new(PrefixOrInfix_Normal, 2, OpTypeMerge(0));
            sNormal_PrefixOrInfix_Normal = new(Normal_PrefixOrInfix_Normal, 3, OpTypeMerge(1));
            sNormal_PostfixOrInfix = new(Normal_PostfixOrInfix, 2, OpTypeMerge(1));
            sNormal_PostfixOrInfix_Normal = new(Normal_PostfixOrInfix_Normal, 3, OpTypeMerge(1));
            sStandalone = new(Standalone, 1, OpTypeMerge(0));
            sStandaloneOrPrefix = new(StandaloneOrPrefix, 1, OpTypeMerge(0));
            sStandaloneOrPrefix_Normal = new(StandaloneOrPrefix_Normal, 2, OpTypeMerge(0));
            sStandaloneOrPostfix = new(StandaloneOrPostfix, 1, OpTypeMerge(0));
            sNormal_StandaloneOrPostfix = new(Normal_StandaloneOrPostfix, 2, OpTypeMerge(1));
        }

        public override void InitSymbolTable()
        {
        }

        public override void InitTransitions()
        {
            Z.normalStates.UnionWith(new[]
            {
                Normal_Infix_Normal, Prefix_Normal, PrefixOrInfix_Normal,
                Normal_PrefixOrInfix_Normal, Normal_PostfixOrInfix_Normal,
                StandaloneOrPrefix_Normal,
            });

            Z.operatorStates.UnionWith(new[]
            {
                Normal_Infix, Prefix, PrefixOrInfix, Normal_PrefixOrInfix,
                Normal_PostfixOrInfix, StandaloneOrPrefix,
            });

            Z.NormalDict.Add(new StateDict(Z)
            {
                { Normal_Infix, sNormal_Infix_Normal, () => OnInfixSecondArgumentParsed?.Invoke() },
                { Prefix, sPrefix_Normal },
                { PrefixOrInfix, sPrefixOrInfix_Normal },
                { Normal_PrefixOrInfix, sNormal_PrefixOrInfix_Normal },
                { Normal_PostfixOrInfix, sNormal_PostfixOrInfix_Normal },
                { StandaloneOrPrefix, sStandaloneOrPrefix_Normal },
            });

            InfixDict = new StateDict(Z)
            {
                { Z.normalStates, sNormal_Infix }
            };

            PrefixDict = new StateDict(Z)
            {
                { Z.operatorStates, sPrefix }
            };

            PostfixDict = new StateDict(Z)
            {
                { Z.normalStates, sNormal_Postfix }
            };

            PrefixOrInfixDict = new StateDict(Z)
            {
                { Z.operatorStates, sPrefixOrInfix },
                { Z.normalStates, sNormal_PrefixOrInfix }
            };

            PostfixOrInfixDict = new StateDict(Z)
            {
                { Z.normalStates, sNormal_PostfixOrInfix }
            };

            StandaloneDict = new StateDict(Z)
            {
                { Z.operatorStates, sStandalone }
            };

            StandaloneOrPrefixDict = new StateDict(Z)
            {
                { Z.operatorStates, sStandaloneOrPrefix }
            };

            StandaloneOrPostfixDict = new StateDict(Z)
            {
                { Z.operatorStates, sStandaloneOrPostfix },
                { Z.normalStates, sNormal_StandaloneOrPostfix }
            };
        }

        public override void InitTypes()
        {
        }

        public override void InitOther()
        {
            //OnInfixSecondArgumentParsed += () => Z.PushTree();
            // TODO fix or delete
        }

#pragma warning disable CA1822
        public MergeFunc OpTypeMerge(int positionOfOperator)
        {
            return subtrees => subtrees[positionOfOperator].exprType.As<OperatorType>().MergeSubtrees(subtrees);
        }

        public ParseTree OperatorCheckTypes(ParseTree[] subtrees, string operatorName, OperatorOverloadDict operatorDict, ISet<int> indices)
        {
            MergeFunc mergeFunc = operatorDict.GetOrThrow(subtrees, subtrees.Sublist(indices).Types(), operatorName);
            return mergeFunc(subtrees);
        }
#pragma warning restore CA1822

        public void NewPrefix(string name, int rightPrec, OperatorOverloadDict opDict)
        {
            string qname = "'" + name + "'";
            Z.symbolTable.Add(name, new OperatorType(Z, qname)
            {
                Transition = new RightPrecTransition(rightPrec, qname, PrefixDict),
                MergeSubtrees = subtrees => OperatorCheckTypes(subtrees, name, opDict, new HashSet<int> { 1 }),
            });
        }

        public void NewInfix(string name, int leftPrec, int rightPrec, OperatorOverloadDict opDict)
        {
            string qname = "'" + name + "'";
            Z.symbolTable.Add(name, new OperatorType(Z, qname)
            {
                Transition = new BothPrecTransition(leftPrec, rightPrec, qname, InfixDict),
                MergeSubtrees = subtrees => OperatorCheckTypes(subtrees, name, opDict, new HashSet<int> { 0, 2 }),
            });
        }

        public void NewPostfix(string name, int leftPrec, OperatorOverloadDict opDict)
        {
            string qname = "'" + name + "'";
            Z.symbolTable.Add(name, new OperatorType(Z, qname)
            {
                Transition = new LeftPrecTransition(leftPrec, qname, PostfixDict),
                MergeSubtrees = subtrees => OperatorCheckTypes(subtrees, name, opDict, new HashSet<int> { 0 }),
            });
        }

        public void NewPrefixOrInfix(string name, int leftPrec, int rightPrec, OperatorOverloadDict prefixOpDict, OperatorOverloadDict infixOpDict)
        {
            string qname = "'" + name + "'";
            Z.symbolTable.Add(name, new OperatorType(Z, qname)
            {
                Transition = new BothPrecTransition(leftPrec, rightPrec, qname, PrefixOrInfixDict),
                MergeSubtrees = subtrees =>
                {
                    if (subtrees[^1].state.index == Normal_PrefixOrInfix_Normal)
                        return OperatorCheckTypes(subtrees, name, infixOpDict, new HashSet<int> { 0, 2 });
                    else
                        return OperatorCheckTypes(subtrees, name, prefixOpDict, new HashSet<int> { 1 });
                },
            });
        }

        public void NewPostfixOrInfix(string name, int leftPrec, int rightPrec, OperatorOverloadDict postfixOpDict, OperatorOverloadDict infixOpDict)
        {
            string qname = "'" + name + "'";
            Z.symbolTable.Add(name, new OperatorType(Z, qname)
            {
                Transition = new BothPrecTransition(leftPrec, rightPrec, qname, PostfixOrInfixDict),
                MergeSubtrees = subtrees =>
                {
                    if (subtrees[^1].state.index == Normal_PostfixOrInfix_Normal)
                        return OperatorCheckTypes(subtrees, name, infixOpDict, new HashSet<int> { 0, 2 });
                    else
                        return OperatorCheckTypes(subtrees, name, postfixOpDict, new HashSet<int> { 0 });
                },
            });
        }

        public void NewStandalone(string name, OperatorOverloadDict opDict)
        {
            string qname = "'" + name + "'";
            Z.symbolTable.Add(name, new OperatorType(Z, qname)
            {
                Transition = new NormalTransition(qname, StandaloneDict),
                MergeSubtrees = subtrees => OperatorCheckTypes(subtrees, name, opDict, new HashSet<int> { }),
            });
        }

        public void NewStandaloneOrPrefix(string name, int rightPrec, OperatorOverloadDict standaloneOpDict, OperatorOverloadDict prefixOpDict)
        {
            string qname = "'" + name + "'";
            Z.symbolTable.Add(name, new OperatorType(Z, qname)
            {
                Transition = new RightPrecTransition(rightPrec, qname, StandaloneOrPrefixDict),
                MergeSubtrees = subtrees =>
                {
                    if (subtrees[^1].state.index == StandaloneOrPrefix_Normal)
                        return OperatorCheckTypes(subtrees, name, prefixOpDict, new HashSet<int> { 1 });
                    else
                        return OperatorCheckTypes(subtrees, name, standaloneOpDict, new HashSet<int> { });
                },
            });
        }
        
        public void NewStandaloneOrPostfix(string name, int leftPrec, OperatorOverloadDict standaloneOpDict, OperatorOverloadDict postfixOpDict)
        {
            string qname = "'" + name + "'";
            Z.symbolTable.Add(name, new OperatorType(Z, qname)
            {
                Transition = new LeftPrecTransition(leftPrec, qname, StandaloneDict),
                MergeSubtrees = subtrees =>
                {
                    if (subtrees[^1].state.index == Normal_StandaloneOrPostfix)
                        return OperatorCheckTypes(subtrees, name, postfixOpDict, new HashSet<int> { 0 });
                    else
                        return OperatorCheckTypes(subtrees, name, standaloneOpDict, new HashSet<int> { });
                },
            });
        }
    }
}
