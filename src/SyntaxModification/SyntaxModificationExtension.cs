using MyLibraries.LexicalAnalyzer;
using MyLibraries.UsefulMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using ZevviCompiler.FunctionCalls;
using ZevviCompiler.FunctionDefinitions;
using ZevviCompiler.Punctuation;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.SyntaxModification
{
    public class SyntaxModificationExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(PunctuationExtension), /*typeof(VariablesExtension),*/ typeof(FunctionCallsExtension) };

        private PunctuationExtension PuncExt => Z.GetExtension<PunctuationExtension>();

        //private VariablesExtension VarExt => Z.GetExtension<VariablesExtension>();

        private FunctionCallsExtension FuncExt => Z.GetExtension<FunctionCallsExtension>();

        public const int P_SYNTAX_R = 1000;

        public StateIndex Syntax, Syntax_Normal, Syntax_Normal_LeftParen, Syntax_Normal_LeftParen_Normal, Syntax_Normal_LeftParen_Normal_Comma,
            Syntax_Normal_LeftParen_Normal_Comma_Normal, Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen,
            Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace, Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals,
            Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace,
            Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace_Function;
        public State sSyntax, sSyntax_Normal, sSyntax_Normal_LeftParen, sSyntax_Normal_LeftParen_Normal, sSyntax_Normal_LeftParen_Normal_Comma,
            sSyntax_Normal_LeftParen_Normal_Comma_Normal, sSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen,
            sSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace;
        public Nonterminal nSyntaxDefinition;

        public StateDict SyntaxDict;

        public List<SyntaxDefinition> syntaxDefinitions = new();

        public State SSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals(int numToPop) => new(Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals, numToPop);

        public State SSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace(int numToPop) => new(Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace, numToPop);

        public State SSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace_Function(int numToPop) => new(Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace_Function, numToPop, SyntaxDefinitionMerge);

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
            Syntax = new("Syntax");
            Syntax_Normal = new("Syntax_Normal");
            Syntax_Normal_LeftParen = new("Syntax_Normal_LeftParen");
            Syntax_Normal_LeftParen_Normal = new("Syntax_Normal_LeftParen_Normals");
            Syntax_Normal_LeftParen_Normal_Comma = new("Syntax_Normal_LeftParen_Normal_Comma");
            Syntax_Normal_LeftParen_Normal_Comma_Normal = new("Syntax_Normal_LeftParen_Normal_Comma_Normal");
            Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen = new("Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen");
            Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace
                = new("Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace");
            Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals
                = new("Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals");
            Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace
                = new("Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace");
            Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace_Function
                = new("Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace_Function");

            sSyntax = new(Syntax, 1);
            sSyntax_Normal = new(Syntax_Normal, 2);
            sSyntax_Normal_LeftParen = new(Syntax_Normal_LeftParen, 3);
            sSyntax_Normal_LeftParen_Normal = new(Syntax_Normal_LeftParen_Normal, 4);
            sSyntax_Normal_LeftParen_Normal_Comma = new(Syntax_Normal_LeftParen_Normal_Comma, 5);
            sSyntax_Normal_LeftParen_Normal_Comma_Normal = new(Syntax_Normal_LeftParen_Normal_Comma_Normal, 6);
            sSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen = new(Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen, 7);
            sSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace = new(Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace, 8);
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("syntax", ZType.WithVoidConverts(Z, "syntax", new RightPrecTransition(P_SYNTAX_R, "'syntax'", SyntaxDict)));
        }

        public override void InitTransitions()
        {
            Z.normalStates.UnionWith(new[]
            {
                Syntax_Normal,
                Syntax_Normal_LeftParen_Normal,
                Syntax_Normal_LeftParen_Normal_Comma_Normal,
            });
            Z.operatorStates.UnionWith(new[]
            {
                Syntax,
                Syntax_Normal_LeftParen,
                Syntax_Normal_LeftParen_Normal_Comma,
                Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace,
            });
            //PuncExt.expectingStatementStates.Add(Syntax_Normal_LeftParen_Normals_RightParen);

            Z.NormalDict.Add(new StateDict(Z)
            {
                { Syntax, sSyntax_Normal },
                { Syntax_Normal_LeftParen, sSyntax_Normal_LeftParen_Normal },
                { Syntax_Normal_LeftParen_Normal_Comma, sSyntax_Normal_LeftParen_Normal_Comma_Normal },
                { Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace,
                    SSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals(9) },
                { Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals,
                    () => SSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals(Z.LastNumToPop + 1) },
            });

            //PuncExt.StatementDict.Add(Syntax_Normal_LeftParen_Normals_RightParen, () => SSyntax_Normal_LeftParen_Normals_RightParen_Function(Z.LastNumToPop + 1));
            PuncExt.LeftParenDict.Add(Syntax_Normal, sSyntax_Normal_LeftParen);
            PuncExt.RightParenDict.Add(Syntax_Normal_LeftParen_Normal_Comma_Normal, sSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen);

            PuncExt.LeftBraceDict.Add(Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen, sSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace);
            PuncExt.RightBraceDict.Add(Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals,
                () => SSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace(Z.LastNumToPop + 1));

            PuncExt.CommaDict.Add(Syntax_Normal_LeftParen_Normal, sSyntax_Normal_LeftParen_Normal_Comma);

            FuncExt.FunctionDict.Add(Syntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace,
                () => SSyntax_Normal_LeftParen_Normal_Comma_Normal_RightParen_LeftBrace_Normals_RightBrace_Function(Z.LastNumToPop + 1));

            SyntaxDict = new(Z) { { Z.operatorStates, sSyntax } };
        }

        public override void InitTypes()
        {
        }

        public ParseTree SyntaxDefinitionMerge(ParseTree[] subtrees)
        {
            ParseTree nameTree = subtrees[1];
            ParseTree leftPrecTree = subtrees[3];
            ParseTree rightPrecTree = subtrees[5];
            ParseTree[] syntaxDefTrees = subtrees[8..^2];
            ParseTree funcTree = subtrees[^1];

            string name = nameTree.exprType.As<IdentifierType>(type => new ZevviSyntaxError(
                $"Expected undefined identifier for syntax name, got {type}.", nameTree.GetTokens())).Identifier;

            int leftPrec;
            int rightPrec;

            if (leftPrecTree.nonterminal == Nonterminal.nIntToken)
            {
                leftPrec = int.Parse(((IParseLeaf)leftPrecTree).Token.Lexeme);
            }
            else
            {
                throw new ZevviSyntaxError("Left operator precedence in syntax definition must be an integer token.",
                    leftPrecTree.GetTokens());
            }

            if (rightPrecTree.nonterminal == Nonterminal.nIntToken)
            {
                rightPrec = int.Parse(((IParseLeaf)rightPrecTree).Token.Lexeme);
            }
            else
            {
                throw new ZevviSyntaxError("Right operator precedence in syntax definition must be an integer token.",
                    rightPrecTree.GetTokens());
            }

            List<string> identifiers = new();
            List<StateIndex> stateIndices = new();
            List<State> states = new();
            List<IStateDict> stateDicts = new();
            List<int> paramIndices = new();

            bool normalFirst = syntaxDefTrees[0].nonterminal == Nonterminal.nIdToken;

            if (normalFirst && syntaxDefTrees[1].nonterminal == Nonterminal.nIdToken)
            {
                throw new ZevviTypeError("Cannot begin syntax definition syntax with two parameters.", subtrees[..2].GetTokensRange());
            }

            ZType lastIdType = null;
            string lastQName = null;

            for (int i = 0; i < syntaxDefTrees.Length; i++)
            {
                ParseTree subtree = syntaxDefTrees[i];
                int num = i + 1;

                // if parameter appears as first part of new syntax, use normal state instead of creating a new state
                StateIndex stateIndex = num == 1 && normalFirst ? Z.Normal : new($"syntaxdef_{name}_state_{num}");
                stateIndices.Add(stateIndex);

                int k = syntaxDefinitions.Count;
                MergeFunc Merge = num == syntaxDefTrees.Length ? (subtrees => UserDefinedSyntaxMerge(subtrees, k)) : null;
                State state = new(stateIndex, num, Merge);
                states.Add(state);

                HashSet<StateIndex> prevSet = num == 1 ? Z.operatorStates : num == 2 && normalFirst ? Z.normalStates : null;
                bool usePrevSet = prevSet != null;
                StateIndex prevIndex = usePrevSet ? default : stateIndices[^2];

                if (subtree.nonterminal == Nonterminal.nStrToken)
                {
                    string raw = ((IParseLeaf)subtree).Token.Lexeme;
                    string id = Lexer<Token, TokenType>.DoEscapes(raw);

                    bool error = false;

                    try
                    {
                        Token[] tokens = Z.Tokenize(id).ToArray();

                        if (tokens.Length != 2 || tokens[0].TokenType != TokenType.Id)
                        {
                            error = true;
                        }
                    }
                    catch (ZevviLexicalError)
                    {
                        error = true;
                    }

                    if (error)
                    {
                        throw new ZevviSyntaxError($"In a syntax definition, strings must contain exactly one " +
                            $"identifier and no other tokens, but found {raw}.", subtree.GetTokens());
                    }

                    SymbolTableEntry entry = Z.symbolTable.Get(id);
                    IStateDict dict;

                    if (entry is null)
                    {
                        identifiers.Add(id);
                        dict = new StateDict(Z);
                        string qname = $"'{id}'";
                        ITransition transition = paramIndices.Count > 0 && identifiers.Count == 1 ? new LeftPrecTransition(leftPrec, qname, dict)
                            : new NormalTransition(qname, dict);
                        lastIdType = ZType.WithVoidConverts(Z, qname, transition);
                        lastQName = qname;
                        Z.symbolTable.Add(id, lastIdType);
                        stateDicts.Add(dict);
                    }
                    else
                    {
                        int n = identifiers.IndexOf(id);

                        if (n == -1)
                        {
                            throw new ZevviTypeError($"Cannot redefine identifier '{id}' in syntax statement.", subtree.GetTokens());
                        }

                        dict = stateDicts[n];
                    }

                    if (usePrevSet)
                    {
                        dict.Add(prevSet, state);
                    }
                    else
                    {
                        dict.Add(prevIndex, state);
                    }
                }
                else if (subtree.nonterminal == Nonterminal.nIdToken)
                {
                    paramIndices.Add(i);
                    Z.normalStates.Add(stateIndex);

                    if (!usePrevSet)
                    {
                        Z.operatorStates.Add(prevIndex);
                        Z.NormalDict.Add(prevIndex, state);
                    }
                }
                else
                {
                    throw new ZevviSyntaxError("Invalid syntax definition syntax: Expected a sequence of string or identifier tokens.", subtree.GetTokens());
                }
            }

            if (lastIdType == null)
            {
                throw new ZevviSyntaxError("Invalid syntax definition syntax: Syntax definitions must contain at least one string token.", syntaxDefTrees.GetTokensRange());
            }

            if (paramIndices.Count > 0 && paramIndices[^1] == syntaxDefTrees.Length - 1)
            {
                ITransition transition1 = lastIdType.Transition;
                ITransition transition2 = transition1 is NormalTransition ? new RightPrecTransition(rightPrec, lastQName, transition1.StateDict)
                    : new BothPrecTransition(leftPrec, rightPrec, lastQName, transition1.StateDict);
                lastIdType.Transition = transition2;
            }

            FunctionType function = funcTree.exprType.As<FunctionType>(type => new ZevviTypeError(
                $"Expression at the end of a syntax statement must have a function type, found {type}.", funcTree.GetTokens()));
            Nonterminal nonterminal = new($"syntaxdef_{name}");
            syntaxDefinitions.Add(new(name, identifiers, stateIndices, states, stateDicts, nonterminal, paramIndices, function));

            // TODO: Allow user to specify left and right precs for leftmost and rightmost operators, respectively
            //throw new NotImplementedException("TODO: Allow user to specify left and right precs for leftmost and rightmost operators, respectively.");

            return new ParseTree(subtrees, Z.Void, ZI.Code.None, nSyntaxDefinition);
        }

        public ParseTree UserDefinedSyntaxMerge(ParseTree[] subtrees, int k)
        {
            SyntaxDefinition def = syntaxDefinitions[k];
            List<ParseTree> paramTrees = subtrees.Sublist(def.paramIndices).ToList();
            List<ParseTree> fakeSubtrees = new()
            {
                new(Array.Empty<ParseTree>(), def.function, ZI.Code.None, Nonterminal.nIdToken),
                new Token(Z, TokenType.Id, "(", default).ToParseTree()
            };

            if (paramTrees.Count > 0)
            {
                fakeSubtrees.Add(paramTrees[0]);
            }

            for (int i = 1; i < paramTrees.Count; i++)
            {
                fakeSubtrees.Add(new Token(Z, TokenType.Id, ",", default).ToParseTree());
                fakeSubtrees.Add(paramTrees[i]);
            }

            fakeSubtrees.Add(new Token(Z, TokenType.Id, ")", default).ToParseTree());

            ParseTree fakeTree = FuncExt.FunctionCallMerge(fakeSubtrees.ToArray(), def.function.codeToCall, def.function.GetReturnType);
            return new ParseTree(subtrees, fakeTree.exprType, fakeTree.exprCode, def.nonterminal);
        }
    }
}
