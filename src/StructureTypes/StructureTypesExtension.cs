using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZevviCompiler.Modules;
using ZevviCompiler.Punctuation;
using ZevviCompiler.Transitions;
using ZevviCompiler.Variables;

namespace ZevviCompiler.StructureTypes
{
    /// <summary>
    /// A Zevvi compiler extension with structure type definition syntax.
    /// Requirements:
    /// <see cref="PunctuationExtension"/>, <see cref="VariablesExtension"/>,
    /// <see cref="ModulesExtension"/>.
    /// </summary>
    public class StructureTypesExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(PunctuationExtension), typeof(VariablesExtension), typeof(ModulesExtension) };

        private PunctuationExtension PuncExt => Z.GetExtension<PunctuationExtension>();

        private VariablesExtension VarExt => Z.GetExtension<VariablesExtension>();

        private ModulesExtension ModExt => Z.GetExtension<ModulesExtension>();

        public StateDict StructDict;
        public StateDict NewDict;

        //public override void InitConverts()
        //{
        //    throw new NotImplementedException();
        //}

        //public override void InitStates()
        //{
        //    throw new NotImplementedException();
        //}

        //public override void InitSymbolTable()
        //{
        //    throw new NotImplementedException();
        //}

        //public override void InitTransitions()
        //{
        //    throw new NotImplementedException();
        //}

        //public override void InitTypes()
        //{
        //    throw new NotImplementedException();
        //}

        // TODO add StateIndices, States, ... to StructureTypesExtension
        public StateIndex Struct, Struct_Normal, Struct_Normal_LeftBrace, Struct_Normal_LeftBrace_Statements, Struct_Normal_LeftBrace_Statements_RightBrace;
        public State sStruct, sStruct_Normal, sStruct_Normal_LeftBrace;
        public Nonterminal nStruct;

        public HashSet<Nonterminal> validStructStatements;

        public State SStruct_Normal_LeftBrace_Statements(int numToPop) => new(Struct_Normal_LeftBrace_Statements, numToPop);

        public State SStruct_Normal_LeftBrace_Statements_RightBrace(int numToPop) => new(Struct_Normal_LeftBrace_Statements_RightBrace, numToPop, StructMerge);

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
            nStruct = new("Struct");

            validStructStatements = new()
            {
                PuncExt.nBlock,
                VarExt.nDeclaration,
                VarExt.nAssignment,
                nStruct,
                ModExt.nModule,
            };

            ModExt.validModuleStatements.Add(nStruct);

            Struct = new("Struct");
            Struct_Normal = new("Struct_Normal");
            Struct_Normal_LeftBrace = new("Struct_Normal_LeftBrace");
            Struct_Normal_LeftBrace_Statements = new("Struct_Normal_LeftBrace_Statements");
            Struct_Normal_LeftBrace_Statements_RightBrace = new("Struct_Normal_LeftBrace_Statements_RightBrace");

            sStruct = new(Struct, 1);
            sStruct_Normal = new(Struct_Normal, 2);
            sStruct_Normal_LeftBrace = new(Struct_Normal_LeftBrace, 3);
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("struct", ZType.WithVoidConverts(Z, "'struct'", new NormalTransition("'struct'", StructDict)));
        }

        public override void InitTransitions()
        {
            Z.normalStates.Add(Struct_Normal);

            Z.operatorStates.UnionWith(new[]
            {
                Struct,
                Struct_Normal_LeftBrace,
                Struct_Normal_LeftBrace_Statements,
            });

            Z.NormalDict.Add(new StateDict(Z)
            {
                { Struct, sStruct_Normal },
                { Struct_Normal_LeftBrace, Z.sNormal },
                { Struct_Normal_LeftBrace_Statements, Z.sNormal },
            });

            void PushStructScope()
            {
                Stack<Scope> scopes = Z.symbolTable.scopes;
                Z.symbolTable.scopes.Push(new StructScope(scopes.Count > 0 ? scopes.Peek().nextVarLoc : 0));
            }

            PuncExt.LeftBraceDict.Add(Struct_Normal, sStruct_Normal_LeftBrace, PushStructScope);

            PuncExt.RightBraceDict.Add(Struct_Normal_LeftBrace_Statements, () => SStruct_Normal_LeftBrace_Statements_RightBrace(Z.LastNumToPop + 1));

            PuncExt.StatementDict.Add(Struct_Normal_LeftBrace, SStruct_Normal_LeftBrace_Statements(4), CheckStatementNonterminal);

            PuncExt.StatementDict.Add(Struct_Normal_LeftBrace_Statements, () => SStruct_Normal_LeftBrace_Statements(Z.LastNumToPop + 1), CheckStatementNonterminal);

            StructDict = new(Z)
            {
                { Z.operatorStates, sStruct },
            };
        }

        public override void InitTypes()
        {
            VarExt.declarationMergeFuncs.Add(_ => Z.symbolTable.InnerScope is StructScope ? (subtrees => VarExt.DeclarationMerge(subtrees, Z.NextGlobalVarLoc)) : null);
            PuncExt.canAccessMember.Add(types => types[0] is StructureType && types[1] is IIdentifierType ? MemberAccessMerge : null);
        }

        public void CheckStatementNonterminal()
        {
            ParseTree statement = Z.currentTree;

            if (statement.nonterminal == PuncExt.nStatement)
            {
                statement = statement.subtrees[0];
            }

            if (!validStructStatements.Contains(statement.nonterminal))
            {
                throw new ZevviSyntaxError($"Invalid statement in struct: {statement.nonterminal}; " +
                    $"valid statements are {string.Join(", ", validStructStatements)}.", statement.GetTokens());
            }
        }

        public ParseTree StructMerge(ParseTree[] subtrees)
        {
            string name = subtrees[1].GetIdentifier(type => $"Expected identifier for struct name, got {type}.");

            StructureType type = new(Z, name)
            {
                members = Z.symbolTable.PopScope()
            };

            Z.symbolTable.Add(name, new TypeType(VarExt, type));

            return ParseTree.CombineTrees(subtrees, i => 3 <= i && i <= subtrees.Length - 2, PuncExt.Statement, ZI.Code.None, nStruct);
        }

        public ParseTree MemberAccessMerge(ParseTree[] subtrees)
        {
            throw new NotImplementedException("Finish MemberAccessMerge and also add 'new' statement syntax.");  // TODO finish
        }
    }
}
