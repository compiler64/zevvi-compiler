using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MyLibraries.LexicalAnalyzer;
using ZevviCompiler.Transitions;

namespace ZevviCompiler
{
    public partial class DefaultExtension : ICompilerExtension
    {
        public DefaultExtension Z
        {
            get => this;
            set => throw new ZevviInternalCompilerError("Cannot set compiler of default extension.");
        }

        public ISet<Type> RequiredExtensions => ICompilerExtension.noRequirements;

        public DateTime startTime;
        public DateTime endTime;
        public TimeSpan elapsedTime;
        public TimeSpan lexTime;
        public TimeSpan parseTime;
        public TimeSpan initTime;

        public IEnumerable<Token> tokens;
        public List<Token> tokenList;
        public LinkedList<ParseTree> subtrees = new();
        public ParseTree currentTree;
        public SymbolTable symbolTable = new();

        public int nextTypeNumber = 0;
        public int nextObjectLocation = ObjectLocationStart;

        public event Action OnNextTransitionComplete;

        public const int P_EOF = 0;
        public const int P_START = 1;

        public const int ObjectLocationStart = int.MinValue;

        public int NextGlobalVarNum { get; private set; }

        public ZI.Variable NextGlobalVarLoc => new(NextGlobalVarNum++, ZI.VariableStorageType.Global);

        public IStateDict NormalDict { get; private set; }

        public IStateDict EofDict { get; private set; }

        public HashSet<StateIndex> normalStates;
        public HashSet<StateIndex> operatorStates;

        public StateIndex Initial, Normal, Normal_Eof;
        public State sInitial, sNormal, sNormal_Eof;
        public Nonterminal nProgram;

        public ZType Eof, Void, Bool, Int, Float, Char, String;

        public ZType UnknownIdentifier(string identifier) => new IdentifierType(this, $"UnknownIdentifier(\"{identifier}\")", identifier)
        {
            Converter = NormalConverter
        };

        public RightPrecTree StartTree;

        public void InitOther()
        {
            StartTree = new RightPrecTree(Array.Empty<ParseTree>(), null, ZI.Code.None, sInitial, Nonterminal.nNull, P_START);
            //PushTree(StartTree);
        }

        public void InitStates()
        {
            nProgram = new("Program");

            Initial = new("Initial");
            Normal = new("Normal");
            Normal_Eof = new("Normal_Eof");

            sInitial = new(Initial, 0);
            sNormal = new(Normal, 1);
            sNormal_Eof = new(Normal_Eof, 2,
                subtrees =>
                {
                    ZI.Code voidConvert = subtrees[0].ImplicitConvertTree(Void);

                    if (voidConvert is null)
                    {
                        throw new ZevviTypeError($"Program must be convertible to {Void}.", tokenList);
                    }

                    return new ParseTree(subtrees, subtrees[0].exprType, voidConvert + ZI.Triple.Halt, nProgram);
                });
        }

        public void InitSymbolTable()
        {
            symbolTable.PushScope();
        }

        public void InitTransitions()
        {
            NormalDict = new StateDict(this) { { Initial, sNormal } };
            EofDict = new StateDict(this) { { Normal, sNormal_Eof } };

            normalStates = new HashSet<StateIndex> { Normal };
            operatorStates = new HashSet<StateIndex> { Initial };
        }

        public void InitTypes()
        {
            Eof = new ZType(this, "Eof")
            {
                Transition = new EofTransition(this),
            };

            Void = ZType.WithVoidConverts(this, "void", new NormalTransition("void", NormalDict));
            Int = ZType.WithNormalConverts(this, "int", new NormalTransition("int", NormalDict));
            Bool = ZType.WithNormalConverts(this, "bool", new NormalTransition("bool", NormalDict));
            Float = ZType.WithNormalConverts(this, "float", new NormalTransition("float", NormalDict));
            Char = ZType.WithNormalConverts(this, "char", new NormalTransition("char", NormalDict));
            String = ZType.WithNormalConverts(this, "string", new NormalTransition("string", NormalDict));
        }

        public void InitializeAll()
        {
            foreach (ICompilerExtension extension in extensions)
            {
                extension.Initialize();
            }
        }

        public void Reset()
        {
            symbolTable.Clear();
            subtrees.Clear();
        }

        public IEnumerable<Token> Tokenize(string source)
        {
            try
            {
                return Lexer<Token, TokenType>.Tokenize(DefaultRegex.defaultRegex, source, Token.DefaultTokenCreator(this));
            }
            catch (LexerException e)
            {
                throw new ZevviLexicalError(e.position);
            }
        }

        public ParseTree Compile(string source, bool reset = true)
        {
            return Parse(Tokenize(source), reset);
        }

        public ParseTree Parse(IEnumerable<Token> tokens, bool reset = true)
        {
            try
            {
                return ParsePrivate(tokens, reset);
            }
            catch (LexerException e)
            {
                throw new ZevviLexicalError(e.position);
            }
        }

        private ParseTree ParsePrivate(IEnumerable<Token> tokens, bool reset = true)
        {
            DateTime time;
            DateTime time2;
            DateTime time3;

            startTime = DateTime.Now;
            time = DateTime.Now;

            if (reset)
            {
                symbolTable.Clear();
            }

            subtrees.Clear();

            if (symbolTable.IsEmpty)
            {
                InitializeAll();
            }

            PushTree(StartTree);

            initTime = DateTime.Now - time;

            this.tokens = tokens;
            tokenList = new List<Token>();

            lexTime = TimeSpan.Zero;
            parseTime = TimeSpan.Zero;
            time3 = DateTime.Now;

            foreach (Token token in this.tokens)
            {
                time2 = DateTime.Now;
                lexTime += time2 - time3;

                tokenList.Add(token);
                PushTreeAndTransition(token.ToParseTree());
                OnNextTransitionComplete?.Invoke();
                OnNextTransitionComplete = null;

                time3 = DateTime.Now;
                parseTime += time3 - time2;
            }

            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;

            return subtrees.First.Next.Value;
        }

        public void PushTreeAndTransition(ParseTree tree)
        {
            PushTree(tree);
            tree.exprType.Transition.Transition();
        }

        public ParseTree Compile(string source, out string displayText, bool reset = true)
        {
            ParseTree tree = Compile(source, reset);

            TimeSpan[] times = new[] { elapsedTime, initTime, lexTime, parseTime };
            IEnumerable<string> timeStrings = times.Select(time => $"{time.TotalMilliseconds:N2}");
            int maxTimeStringLength = timeStrings.Max(str => str.Length);
            string[] paddedTimeStrings = timeStrings.Select(str => str.PadLeft(maxTimeStringLength)).ToArray();

            displayText =
@$"
Compiling successful.

Elapsed time:  {paddedTimeStrings[0]} ms
Setup time:    {paddedTimeStrings[1]} ms
Lex time:      {paddedTimeStrings[2]} ms
Parse time:    {paddedTimeStrings[3]} ms

Parse tree: {tree}
Generated code: {tree.exprCode}";
            return tree;
        }

        public ParseTree DisplayCompile(string source)
        {
            ParseTree tree = Compile(source, out string displayText);
            Console.WriteLine(displayText);
            return tree;
        }

        public void PushTree(ParseTree tree)
        {
            subtrees.AddLast(tree);
        }

        public ParseTree PopTree()
        {
            ParseTree last = subtrees.Last.Value;
            subtrees.RemoveLast();
            return last;
        }

        public ParseTree PeekTree => subtrees.Last.Value;

        public ParseTree PeekPrevTree => subtrees.Last.Previous.Value;

        public int LastNumToPop => PeekTree.state.numToPop;

        public void Merge()
        {
            State oldState = PeekTree.state;
            int numToPop = oldState.numToPop;
            ParseTree[] poppedSubtrees = new ParseTree[numToPop];

            for (int i = numToPop - 1; i >= 0; i--)
            {
                poppedSubtrees[i] = PopTree();
            }

            ParseTree newSubtree = oldState.Merge(poppedSubtrees);
            PushTree(newSubtree);
            newSubtree.exprType.Transition.Transition();
        }
    }
}
