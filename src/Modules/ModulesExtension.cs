using System;
using System.Collections.Generic;
using ZevviCompiler.FunctionDefinitions;
using ZevviCompiler.OperatorSyntaxes;
using ZevviCompiler.Punctuation;
using ZevviCompiler.Transitions;
using ZevviCompiler.Variables;

namespace ZevviCompiler.Modules
{
    /// <summary>
    /// A Zevvi compiler extension with the keywords and syntax for module definitions and member access.
    /// Contains the 'module' keyword and overloads the '.' operator.<br/>
    /// Required extensions: <see cref="PunctuationExtension"/>, <see cref="VariablesExtension"/>.
    /// </summary>
    public class ModulesExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(OperatorSyntaxesExtension), typeof(PunctuationExtension), typeof(VariablesExtension) };

        private OperatorSyntaxesExtension OpExt => Z.GetExtension<OperatorSyntaxesExtension>();

        private PunctuationExtension PuncExt => Z.GetExtension<PunctuationExtension>();

        private VariablesExtension VarExt => Z.GetExtension<VariablesExtension>();

        public int NumberOfNestedModules { get; private set; } = 0;

        public StateDict ModuleDict;

        public StateIndex Module, Module_Normal, Module_Normal_LeftBrace, Module_Normal_LeftBrace_Statements, Module_Normal_LeftBrace_Statements_RightBrace;
        public State sModule, sModule_Normal, sModule_Normal_LeftBrace;
        public Nonterminal nModule, nMemberAccess;

        public HashSet<Nonterminal> validModuleStatements;

        public State SModule_Normal_LeftBrace_Statements(int numToPop) => new(Module_Normal_LeftBrace_Statements, numToPop);

        public State SModule_Normal_LeftBrace_Statements_RightBrace(int numToPop) => new(Module_Normal_LeftBrace_Statements_RightBrace, numToPop, ModuleMerge);

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
            nModule = new("Module");
            nMemberAccess = new("MemberAccess");

            validModuleStatements = new()
            {
                PuncExt.nBlock,
                VarExt.nDeclaration,
                VarExt.nAssignment,
                nModule,
            };

            if (Z.TryGetExtension(out FunctionDefinitionsExtension FuncDefExt))
            {
                validModuleStatements.Add(FuncDefExt.nFunctionDefinition);
            }

            Module = new("Module");
            Module_Normal = new("Module_Normal");
            Module_Normal_LeftBrace = new("Module_Normal_LeftBrace");
            Module_Normal_LeftBrace_Statements = new("Module_Normal_LeftBrace_Statements");
            Module_Normal_LeftBrace_Statements_RightBrace = new("Module_Normal_LeftBrace_Statements_RightBrace");

            sModule = new(Module, 1);
            sModule_Normal = new(Module_Normal, 2);
            sModule_Normal_LeftBrace = new(Module_Normal_LeftBrace, 3);
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("module", ZType.WithVoidConverts(Z, "'module'", new NormalTransition("'module'", ModuleDict)));
        }

        public override void InitTransitions()
        {
            Z.normalStates.Add(Module_Normal);

            Z.operatorStates.UnionWith(new[]
            {
                Module,
                Module_Normal_LeftBrace,
                Module_Normal_LeftBrace_Statements,
            });

            Z.NormalDict.Add(new StateDict(Z)
            {
                { Module, sModule_Normal },
                { Module_Normal_LeftBrace, Z.sNormal },
                { Module_Normal_LeftBrace_Statements, Z.sNormal },
            });

            void PushModuleScope()
            {
                Stack<Scope> scopes = Z.symbolTable.scopes;
                Z.symbolTable.scopes.Push(new ModuleScope(scopes.Count > 0 ? scopes.Peek().nextVarLoc : 0));
            }

            PuncExt.LeftBraceDict.Add(Module_Normal, sModule_Normal_LeftBrace, PushModuleScope);

            PuncExt.RightBraceDict.Add(Module_Normal_LeftBrace_Statements, () => SModule_Normal_LeftBrace_Statements_RightBrace(Z.LastNumToPop + 1));

            PuncExt.StatementDict.Add(Module_Normal_LeftBrace, SModule_Normal_LeftBrace_Statements(4), CheckStatementNonterminal);

            PuncExt.StatementDict.Add(Module_Normal_LeftBrace_Statements, () => SModule_Normal_LeftBrace_Statements(Z.LastNumToPop + 1), CheckStatementNonterminal);

            ModuleDict = new(Z)
            {
                { Z.operatorStates, sModule },
            };
        }

        public override void InitTypes()
        {
            VarExt.declarationMergeFuncs.Add(_ => Z.symbolTable.InnerScope is ModuleScope ? (subtrees => VarExt.DeclarationMerge(subtrees, Z.NextGlobalVarLoc)) : null);
            PuncExt.canAccessMember.Add(types => types[0] is ModuleType && types[1] is IIdentifierType ? MemberAccessMerge : null);
        }

        public void CheckStatementNonterminal()
        {
            ParseTree statement = Z.currentTree;

            if (statement.nonterminal == PuncExt.nStatement)
            {
                statement = statement.subtrees[0];
            }

            if (!validModuleStatements.Contains(statement.nonterminal))
            {
                throw new ZevviSyntaxError($"Invalid statement in module: {statement.nonterminal}; " +
                    $"valid statements are {string.Join(", ", validModuleStatements)}.", statement.GetTokens());
            }
        }

        public ParseTree ModuleMerge(ParseTree[] subtrees)
        {
            string name = subtrees[1].GetIdentifier(type => $"Expected identifier for module name, got {type}.");

            ModuleType type = new(Z)
            {
                members = Z.symbolTable.PopScope()
            };

            Z.symbolTable.Add(name, type);

            return ParseTree.CombineTrees(subtrees, i => 3 <= i && i <= subtrees.Length - 2, PuncExt.Statement, ZI.Code.None, nModule);
        }

        public ParseTree MemberAccessMerge(ParseTree[] subtrees)
        {
            ModuleType moduleType = subtrees[0].exprType.As<ModuleType>();
            string memberName = subtrees[2].GetIdentifier(type => $"Expected identifier for member name, got {type}.");

            if (moduleType.members.entries.TryGetValue(memberName, out SymbolTableEntry entry))
            {
                return new ParseTree(subtrees, entry.type, ZI.Code.Operand(entry.storage), nMemberAccess);
            }
            else
            {
                List<Token> moduleTokens = subtrees[0].GetTokens();
                List<Token> memberTokens = subtrees[2].GetTokens();

                if (moduleTokens.Count == 1 && moduleTokens[0].TokenType == TokenType.Id)
                {
                    string module = moduleTokens[0].Lexeme;
                    throw new ZevviUnknownMemberError(module, memberName, memberTokens);
                }
                else
                {
                    throw new ZevviUnknownMemberError(memberName, memberTokens);
                }
            }
        }

        /*public ParseTree MemberDeclarationMerge(ParseTree[] subtrees)
        {
            throw new NotImplementedException();
        }*/
    }
}
