using System;
using System.Collections;
using System.Collections.Generic;
using MyLibraries.LexicalAnalyzer;
using System.Linq;
using MI = ZevviCompiler.MachineInstruction;
using System.Text;

#pragma warning disable CS0659

namespace Cszc
{
    static class Z
    {
        public const int TREE_STACK_SIZE = 256;
        public const int SYMBOL_TABLE_SIZE = 256;
        public const int SCOPE_SIZE = 256;

        public static string source;
        public static IEnumerable<Token> tokens;
        public static LinkedList<ParseTree> subtrees = new(new[] { ParseTree.Start });

        public static SymbolTable symbolTable = new();

        public static int nextMark = 0;
        public static readonly Stack<(string name, ZType returnType, int callMark, int returnMark)> functions = new();

        public static DateTime startTime;
        public static DateTime endTime;
        public static DateTime time;
        public static TimeSpan elapsedTime;
        public static TimeSpan initTime;
        public static TimeSpan parseTime;

        public static void PushTree(ParseTree tree)
        {
            //subtrees[NumTrees()] = tree;
            //NumTrees()++;
            subtrees.AddLast(tree);
        }

        public static ParseTree PopTree()
        {
            //NumTrees()--;
            //return subtrees[NumTrees()];
            ParseTree last = subtrees.Last.Value;
            subtrees.RemoveLast();
            return last;
        }

        public static ParseTree PeekTree => subtrees.Last.Value;

        public static int LastNumToPop => PeekTree.state.numToPop;

        public static int NumTrees()
        {
            return subtrees.Count;
        }

        public static ParseTree Compile(string source)
        {
            return Compile(source, Lexer<Token, TokenType>.Tokenize(DefaultRegex.defaultRegex, source, Token.tokenCreator));
        }

        public static ParseTree Compile(string source, IEnumerable<Token> tokens)
        {
            Z.source = source;
            Z.tokens = tokens;

            initTime = TimeSpan.Zero;
            parseTime = TimeSpan.Zero;
            startTime = DateTime.Now;

            InitSymbolTable();

            initTime = DateTime.Now - startTime;

            foreach (Token token in tokens)
            {
                time = DateTime.Now;
                PushTree(token.ToParseTree());
                Transition();
                parseTime += DateTime.Now - time;
            }

            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;

            return subtrees.First.Next.Value;
        }

        public static void InitSymbolTable()
        {
            symbolTable.PushScope();

            symbolTable.Add("+", new OpType("'+'", () => T.BothPrecTransition("'+'",
                T.PrefixOrInfixDict, P.ADDITIVE_L, P.ADDITIVE_R),
                subtrees =>
                {
                    if (subtrees[^1].state.index == (int)S.Normal_PrefixOrInfix_Normal)
                        return CombineTrees(subtrees, new HashSet<int> { 0, 2 }, new[] { ZType.Int, ZType.Int }, ZType.Int, MI.Add);
                    else
                        return CombineTrees(subtrees, new HashSet<int> { 1 }, new[] { ZType.Int }, ZType.Int, ZI.Code.None);
                }));

            symbolTable.Add("-", new OpType("'-'", () => T.BothPrecTransition("'-'",
                T.PrefixOrInfixDict, P.ADDITIVE_L, P.ADDITIVE_R),
                subtrees =>
                {
                    if (subtrees[^1].state.index == (int)S.Normal_PrefixOrInfix_Normal)
                        return CombineTrees(subtrees, new HashSet<int> { 0, 2 }, new[] { ZType.Int, ZType.Int }, ZType.Int, MI.Sub);
                    else
                        return CombineTrees(subtrees, new HashSet<int> { 1 }, new[] { ZType.Int }, ZType.Int, MI.Neg);
                }));

            symbolTable.Add("*", new OpType("'*'", () => T.BothPrecTransition("'*'",
                T.InfixDict, P.MULTIPLICATIVE_L, P.MULTIPLICATIVE_R),
                subtrees => CombineTrees(subtrees, new HashSet<int> { 0, 2 }, new[] { ZType.Int, ZType.Int }, ZType.Int,
                MI.Mul)));

            symbolTable.Add("/", new OpType("'/'", () => T.BothPrecTransition("'/'",
                T.InfixDict, P.MULTIPLICATIVE_L, P.MULTIPLICATIVE_R),
                subtrees => CombineTrees(subtrees, new HashSet<int> { 0, 2 }, new[] { ZType.Int, ZType.Int }, ZType.Int,
                MI.Div)));

            symbolTable.Add("%", new OpType("'%'", () => T.BothPrecTransition("'%'",
                T.InfixDict, P.MULTIPLICATIVE_L, P.MULTIPLICATIVE_R),
                subtrees => CombineTrees(subtrees, new HashSet<int> { 0, 2 }, new[] { ZType.Int, ZType.Int }, ZType.Int,
                MI.Mod)));

            symbolTable.Add("(", new ZType("'('", () => T.Transition("'('",
                T.LeftParenDict), C.Default, C.Default, MetaType.simpleType));

            symbolTable.Add(")", new ZType("')'", () => T.Transition("')'",
                T.RightParenDict), C.Default, C.Default, MetaType.simpleType));

            symbolTable.Add("=", new OpType("'='", () => T.BothPrecTransition("'='",
                T.InfixDict, P.ASSIGNMENT_L, P.ASSIGNMENT_R),
                subtrees =>
                {
                    VarType varType = subtrees[0].exprType.AsVarType();
                    return CombineTrees(subtrees, new HashSet<int> { 2 }, new[] { varType.innerType }, ZType.Void,
                        new SingleMI(MI.PopI, varType.location));
                }));

            symbolTable.Add("int", new TypeType("type int", ZType.Int, C.Default, C.Default));

            symbolTable.Add(";", new OpType("';'", () => T.BothPrecTransition("';'", T.PostfixOrInfixDict,
                P.SEMICOLON_L, P.SEMICOLON_R),
                subtrees => CombineTrees(subtrees, new HashSet<int> { 0 }, new[] { ZType.Void },
                    subtrees[^1].state.index == (int)S.Normal_PostfixOrInfix_Normal ? subtrees[2].exprType : ZType.Void,
                    ZI.Code.None)));

            symbolTable.Add("{", new ZType("'{'", () =>
            {
                T.Transition("'{'", T.LeftBraceDict);
                EnterFunction();
            }, C.Default, C.Default, MetaType.simpleType));

            symbolTable.Add("}", new ZType("'}'", () =>
            {
                T.Transition("'}'", T.RightBraceDict);
                ExitFunction();
            }, C.Default, C.Default, MetaType.simpleType));

            symbolTable.Add("return", new OpType("'return'", () => T.Transition("'return'", T.PrefixDict),
                subtrees =>
                {
                    if (functions.Count == 0)
                    {
                        throw new ZevviException("Syntax error: Return statement found outside of function.");
                    }

                    var (_, returnType, _, returnMark) = functions.Peek();

                    return CombineTrees(subtrees, new HashSet<int> { 1 }, new ZType[] { returnType }, ZType.Void, new SingleMI(MI.GotoMark, returnMark));
                }));

            symbolTable.Add("if", new ZType("'if'", () => T.Transition("'if'", T.IfDict), C.Default, C.Default, MetaType.simpleType));
            symbolTable.Add("then", new ZType("'then'", () => T.RightPrecTransition("'then'",
                T.ThenDict, P.THEN_KEYWORD_R), C.Default, C.Default, MetaType.simpleType));
            symbolTable.Add("else", new ZType("'else'", () => T.RightPrecTransition("'else'",
                T.ElseDict, P.ELSE_KEYWORD_R), C.Default, C.Default, MetaType.simpleType));

            symbolTable.Add("true", ZType.Bool, new SingleMI(MI.PushCon, 1));
            symbolTable.Add("false", ZType.Bool, new SingleMI(MI.PushCon, 0));

            // TODOold InitSymbolTable
        }

        /// <summary>
        /// Called when in state Type_Normal_LeftParen_Params_RightParen to set up a function.
        /// </summary>
        public static void EnterFunction()
        {
            symbolTable.PushScope();
            ParseTree[] subtreesArray = subtrees.ToArray();
            int first = subtrees.Count - LastNumToPop; // index of first subtree in function definition
            functions.Push(
            (
                GetIdentifier(subtreesArray[first + 1].exprType),
                subtreesArray[first].exprType.AsTypeType().thisType,
                nextMark,
                nextMark + 1
            ));

            LinkedListNode<ParseTree> node = subtrees.Last;

            for (int i = 0; i < LastNumToPop; i++)
            {
                if (node.Value.state.index == (int)S.Type_Normal_LeftParen_Params_Type_Normal)
                {
                    ZType type = node.Value.exprType;
                    string id = GetIdentifier(type);
                    ZType thisType = node.Previous.Value.exprType.AsTypeType().thisType;
                    VarType varType = new("var " + thisType.name, id, thisType, symbolTable.scopes.Peek().nextVarLoc++);
                    symbolTable.Add(id, varType);
                }

                node = node.Previous;
            }
        }

        /// <summary>
        /// Called when in state Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal_RightBrace to pop scope.
        /// </summary>
        public static void ExitFunction()
        {
            symbolTable.scopes.Pop();
            functions.Pop();
        }

        public static string GetIdentifier(ZType type)
        {
            if (type.metaType == MetaType.varType)
                return type.AsVarType().identifier;
            else
                return type.AsIdentifierType().identifier;
        }

        public static ParseTree CombineTrees(IList<ParseTree> subtrees, ISet<int> indices, IList<ZType> paramTypes, ZType returnType, ZI.Code extraCode)
        {
            ParseTree[] args = subtrees.Where((item, index) => indices.Contains(index)).ToArray();

            ZI.Code newCode = ZI.Code.None;
            int argNum = 0;

            if (args.Length != paramTypes.Count)
            {
                throw new ZevviException($"Wrong number of arguments: found {args.Length}, expected {paramTypes.Count}.");
            }

            for (int i = 0; i < subtrees.Count; i++)
            {
                if (indices.Contains(i))
                {
                    ZI.Code convert = subtrees[i].exprType.ImplicitConvert(paramTypes[argNum]);

                    if (convert is null)
                    {
                        throw new ZevviException($"Type error in subtree {i}: Cannot convert from {subtrees[i].exprType} to {paramTypes[argNum]}.");
                    }

                    newCode += subtrees[i].exprCode + (ZI.Code)convert;
                    argNum++;
                }
                else
                {
                    newCode += subtrees[i].exprCode;
                }
            }

            return new ParseTree(subtrees.ToArray(), returnType, newCode + extraCode);
        }

        public static void Transition()
        {
            PeekTree.exprType.Transition();
        }

        public static void Merge(int numToPop)
        {
            ParseTree[] poppedSubtrees = new ParseTree[numToPop];
            State oldState = PeekTree.state;

            for (int i = numToPop - 1; i >= 0; i--)
            {
                poppedSubtrees[i] = PopTree();
            }

            ParseTree newSubtree = oldState.Merge(poppedSubtrees);
            PushTree(newSubtree);
            Transition();
        }
    }

    /// <summary>
    /// A function that moves the compiler to the next state.<br/>
    /// Equivalent to Action.
    /// </summary>
    delegate void TransitionFunc();

    /// <summary>
    /// A function that combines subtrees into a parse tree.
    /// Each state that allows merging has a MergeFunc.<br/>
    /// Equivalent to Func&lt;<see cref="ParseTree"/>[], <see cref="ParseTree"/>&gt;.
    /// </summary>
    /// <param name="subtrees">The subtrees array.</param>
    /// <returns>A <see cref="ParseTree"/> whose subtrees are the elements of the <paramref name="subtrees"/> array.</returns>
    delegate ParseTree MergeFunc(ParseTree[] subtrees);

    /// <summary>
    /// A function that returns the code needed to convert an object of type <paramref name="oldType"/> to <paramref name="newType"/>.
    /// This function would belong to <paramref name="oldType"/> in the field <see cref="ZType.ImplicitConverter"/> or the field <see cref="ZType.ExplicitConverter"/>.<br/>
    /// Equivalent to Func&lt;<see cref="ZType"/>, <see cref="ZType"/>, <see cref="ZI.Code"/>&gt;.
    /// </summary>
    /// <param name="oldType">The original type of the object.</param>
    /// <param name="newType">The new type for the object.</param>
    /// <returns>The code needed to convert an object of type <paramref name="oldType"/> to <paramref name="newType"/>, or <see langword="null"/> if the convert is invalid.</returns>
    delegate ZI.Code ConvertFunc(ZType oldType, ZType newType);

    /// <summary>
    /// A function that finds the return type of a function from the types of its arguments.
    /// All <see cref="FunctionType"/> objects must have a GetReturnTypeFunc.
    /// </summary>
    /// <param name="argTypes">An array of the types of the arguments passed into the <see cref="FunctionType"/>.</param>
    /// <returns>A tuple containing the return type of the <see cref="FunctionType"/> and the code.</returns>
    delegate (ZType returnType, ZI.Code[] converts) GetReturnTypeFunc(ZType[] argTypes);

    static class P
    {
        public const int EOF = 0;
        public const int START = 1;
        public const int SEMICOLON_L = 2000;
        public const int SEMICOLON_R = 2001;
        public const int THEN_KEYWORD_R = 3000;
        public const int ELSE_KEYWORD_R = 3000;
        public const int ASSIGNMENT_L = 4001;
        public const int ASSIGNMENT_R = 4000;
        public const int TYPE_R = 5000;
        public const int ADDITIVE_L = 13000;
        public const int ADDITIVE_R = 13001;
        public const int MULTIPLICATIVE_L = 14000;
        public const int MULTIPLICATIVE_R = 14001;
        public const int MAXIMUM = int.MaxValue;
    }

    enum TokenType
    {
        Eof,
        Int,
        Char,
        Str,
        Id
    }

    sealed class Token : AbstractToken<TokenType>
    {
        public Token(TokenType tokenType, string lexeme, PositionInCode position) : base(tokenType, lexeme, position)
        {
        }

        public ParseTree ToParseTree()
        {
            return LoadTokenFuncs[TokenType](this);
        }

        public static Dictionary<TokenType, Func<Token, ParseTree>> LoadTokenFuncs = new()
        {
            [TokenType.Eof] = (Token token) => new ParseLeaf(token, ZType.Eof, ZI.Code.None),
            [TokenType.Int] = (Token token) => new ParseLeaf(token, ZType.Int,
                new SingleMI(MI.PushCon, int.Parse(token.Lexeme))),
            [TokenType.Char] = null,
            [TokenType.Str] = null,
            [TokenType.Id] = (Token token) =>
            {
                (ZType type, ZI.Code code) = Z.symbolTable.Get(token.Lexeme);
                if (type is null) type = ZType.UnknownIdentifier(token.Lexeme);
                return new ParseLeaf(token, type, code);
            }
            // TODOold LoadTokenFuncs
        };

        public override string ToString()
        {
            return TokenType == TokenType.Eof ? "<eof>" : Lexeme.Contains('\n') ? "<multi-line token>" : $"'{Lexeme}'";
        }

        public static TokenCreator<Token, TokenType> tokenCreator = new        (
            eof: (position) => new Token(TokenType.Eof, "", position),
            eofType: TokenType.Eof,
            ignoreGroupName: "ignore",
            @new: (tokenType, lexeme, position) => new Token(tokenType, lexeme, position)
        );
    }

    class ParseTree
    {
        public static readonly RightPrecTree Start = new(new ParseTree[0], null, ZI.Code.None, State.Initial, P.START);

        public ParseTree[] subtrees;
        public ZType exprType;
        public ZI.Code exprCode;
        public State state;

        public ParseTree(ParseTree[] subtrees, ZType exprType, ZI.Code exprCode, State state)
        {
            this.subtrees = subtrees;
            this.exprType = exprType;
            this.exprCode = exprCode;
            this.state = state;
        }

        public ParseTree(ParseTree[] subtrees, ZType exprType, ZI.Code exprCode) : this(subtrees, exprType, exprCode, State.Null)
        {
        }

        public override string ToString()
        {
            return "{ " + string.Join<ParseTree>(" ", subtrees) + " }";
        }

        public virtual RightPrecTree AttachRightPrec(int rightPrec)
        {
            return new RightPrecTree(subtrees, exprType, exprCode, state, rightPrec);
        }
    }

    class ParseLeaf : ParseTree
    {
        public Token token;

        public ParseLeaf(Token token, ZType exprType, ZI.Code exprCode, State state) : base(new ParseTree[0], exprType, exprCode, state)
        {
            this.token = token;
        }

        public ParseLeaf(Token token, ZType exprType, ZI.Code exprCode) : this(token, exprType, exprCode, State.Null)
        {
        }

        public override string ToString()
        {
            return $"{token}";
        }

        public override RightPrecTree AttachRightPrec(int rightPrec)
        {
            return new RightPrecLeaf(token, exprType, exprCode, state, rightPrec);
        }
    }

    class RightPrecTree : ParseTree
    {
        public int rightPrec;

        public RightPrecTree(ParseTree[] subtrees, ZType exprType, ZI.Code exprCode, State state, int rightPrec) : base(subtrees, exprType, exprCode, state)
        {
            this.rightPrec = rightPrec;
        }

        public override string ToString()
        {
            return $"{base.ToString()}[prec {rightPrec}]";
        }
    }

    class RightPrecLeaf : RightPrecTree
    {
        public Token token;

        public RightPrecLeaf(Token token, ZType exprType, ZI.Code exprCode, State state, int rightPrec) : base(new ParseTree[0], exprType, exprCode, state, rightPrec)
        {
            this.token = token;
        }

        public RightPrecLeaf(Token token, ZType exprType, ZI.Code exprCode, int rightPrec) : this(token, exprType, exprCode, State.Null, rightPrec)
        {
        }

        public override string ToString()
        {
            return $"{token}[prec {rightPrec}]";
        }

        public override RightPrecTree AttachRightPrec(int rightPrec)
        {
            return new RightPrecLeaf(token, exprType, exprCode, state, rightPrec);
        }
    }

    static class C
    {
        public static readonly ConvertFunc Default = (oldType, newType) => newType.Equals(oldType) ? ZI.Code.None : (ZI.Code)null;

        public static readonly ConvertFunc VoidOrSelf = (oldType, newType) => newType.Equals(oldType) ? ZI.Code.None : newType.Equals(ZType.Void) ? MI.Remove : (ZI.Code)null;

        public static readonly ConvertFunc Variable = (oldType, newType) =>
        {
            VarType varType = oldType.AsVarType();

            if (newType.Equals(varType))
            {
                return ZI.Code.None;
            }

            if (newType.Equals(varType.innerType))
            {
                return new SingleMI(MI.PushI, varType.location);
            }

            return null;
        };

        public static readonly ConvertFunc Conditional = (oldType, newType) =>
        {
            ConditionalType ctype = oldType.AsConditionalType();
            ZI.Code thenConvert = ctype.thenType.ImplicitConvert(newType);
            ZI.Code elseConvert = ctype.elseType.ImplicitConvert(newType);

            if (!thenConvert.HasValue)
            {
                throw new ZevviException($"Type error in 'then' block: Cannot convert from {ctype.thenType} to {newType}.");
            }
            else if (!elseConvert.HasValue)
            {
                throw new ZevviException($"Type error in 'else' block: Cannot convert from {ctype.elseType} to {newType}.");
            }

            ZI.Code thenWithConvert = ctype.thenCode + thenConvert.Value;
            ZI.Code elseWithConvert = ctype.elseCode + elseConvert.Value;
            return new SingleMI(MI.IfNotRelI, thenWithConvert.Length + 2) + thenWithConvert
                + new SingleMI(MI.GotoRelI, elseWithConvert.Length + 1) + elseWithConvert;
        };
    }

    class ZType
    {
        public static readonly ZType Eof = new("Eof", () =>
        {
            T.LeftPrecTransition("Eof", new StateDict { { S.Normal, State.Normal_Eof } }, P.EOF);
            Z.Merge(Z.PeekTree.state.numToPop);
        }, C.Default, C.Default, MetaType.simpleType);

        public static readonly ZType Void = new("void", () => T.NormalTransition("void"), C.Default, C.Default, MetaType.simpleType);
        public static readonly ZType Int = new("int", () => T.NormalTransition("int"), C.VoidOrSelf, C.Default, MetaType.simpleType);
        public static readonly ZType Bool = new("bool", () => T.NormalTransition("bool"), C.VoidOrSelf, C.Default, MetaType.simpleType);

        public static ZType UnknownIdentifier(string identifier) => new IdentifierType($"UnknownIdentifier[\"{identifier}\"]", identifier);

        public readonly string name;
        public readonly TransitionFunc Transition;
        public readonly ConvertFunc ImplicitConverter;
        public readonly ConvertFunc ExplicitConverter;
        public readonly MetaType metaType;

        public ZType(string name, TransitionFunc transition, ConvertFunc implicitConverter, ConvertFunc explicitConverter, MetaType metaType)
        {
            this.name = name;
            this.metaType = metaType;

            Transition = transition;
            ImplicitConverter = implicitConverter;
            ExplicitConverter = explicitConverter;
        }

        public ZI.Code ImplicitConvert(ZType newType)
        {
            return ImplicitConverter(this, newType);
        }

        public ZI.Code ExplicitConvert(ZType newType)
        {
            return ImplicitConverter(this, newType) ?? ExplicitConverter(this, newType);
        }

        public OpType AsOpType()
        {
            if (!(this is OpType opType))
            {
                throw new ZevviException($"Error in compiler setup: Invalid target for merge function call: {this}.");
            }

            return opType;
        }

        public VarType AsVarType()
        {
            if (!(this is VarType varType))
            {
                throw new ZevviException($"Left-hand side of assignment must be a variable (found {this}).");
            }

            return varType;
        }

        public IdentifierType AsIdentifierType()
        {
            if (!(this is IdentifierType idType))
            {
                throw new ZevviException($"Identifier expected (found {this}).");
            }

            return idType;
        }

        public TypeType AsTypeType()
        {
            if (!(this is TypeType typeType))
            {
                throw new ZevviException($"Type expected (found {this}).");
            }

            return typeType;
        }

        public ConditionalType AsConditionalType()
        {
            if (!(this is ConditionalType conditionalType))
            {
                throw new ZevviException($"Error in compiler setup: Conditional statement expected (found type {this}).");
            }

            return conditionalType;
        }

        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            return obj is ZType type && metaType == type.metaType && ReferenceEquals(this, type);
        }
    }

    class OpType : ZType
    {
        public MergeFunc MergeSubtrees;

        public OpType(string name, TransitionFunc transition, MergeFunc mergeSubtrees) : base(name, transition, C.Default, C.Default, MetaType.simpleType)
        {
            MergeSubtrees = mergeSubtrees;
        }
    }

    class VarType : ZType
    {
        public readonly string identifier;
        public readonly ZType innerType;
        public readonly int location;

        public VarType(string name, string identifier, ZType innerType, int location) : base(name, innerType.Transition, C.Variable, C.Default, MetaType.varType)
        {
            this.identifier = identifier;
            this.innerType = innerType;
            this.location = location;
        }

        public override bool Equals(object obj)
        {
            return obj is VarType type &&
                   base.Equals(obj) &&
                   innerType.Equals(type.innerType) &&
                   location == type.location;
        }
    }

    class IdentifierType : ZType
    {
        public readonly string identifier;

        public IdentifierType(string name, string identifier) : base(name, () => T.NormalTransition(name), C.Default, C.Default, MetaType.identifierType)
        {
            this.identifier = identifier;
        }

        public override bool Equals(object obj)
        {
            return obj is IdentifierType type &&
                   base.Equals(obj) &&
                   identifier == type.identifier;
        }
    }

    class TypeType : ZType
    {
        public readonly ZType thisType;

        public TypeType(string name, ZType thisType, ConvertFunc implicitConverter, ConvertFunc explicitConverter) : base(name, () => T.RightPrecTransition(name, T.TypeDict, P.TYPE_R), implicitConverter, explicitConverter, MetaType.typeType)
        {
            this.thisType = thisType;
        }

        public override bool Equals(object obj)
        {
            return obj is TypeType type &&
                   base.Equals(obj) &&
                   thisType.Equals(type.thisType);
        }
    }

    class FunctionType : OpType
    {
        public readonly int location;
        public readonly GetReturnTypeFunc GetReturnType;

        public FunctionType(string name, int location, GetReturnTypeFunc getReturnType)
            : base(name, () => T.Transition(name, T.FunctionDict), State.FunctionMerge(getReturnType, location))
        {
            this.location = location;
            GetReturnType = getReturnType;
        }
    }

    class ConditionalType : ZType
    {
        public readonly ZType thenType;
        public readonly ZI.Code thenCode;
        public readonly ZType elseType;
        public readonly ZI.Code elseCode;

        public ConditionalType(ZType thenType, ZI.Code thenCode, ZType elseType, ZI.Code elseCode)
            : base($"({thenType} | {elseType})", () => T.NormalTransition($"({thenType} | {elseType})"),
            C.Conditional, C.Default, MetaType.conditionalType)
        {
            this.thenType = thenType;
            this.thenCode = thenCode;
            this.elseType = elseType;
            this.elseCode = elseCode;
        }

        public static ConditionalType IfThen(ParseTree[] subtrees)
        {
            return new ConditionalType(subtrees[3].exprType, subtrees[3].exprCode, ZType.Void, ZI.Code.None);
        }

        public static ConditionalType IfThenElse(ParseTree[] subtrees)
        {
            return new ConditionalType(subtrees[3].exprType, subtrees[3].exprCode, subtrees[5].exprType, subtrees[5].exprCode);
        }
    }

    class MetaType
    {
        public static readonly MetaType simpleType = new(/*(type1, type2) => type1 == type2*/);
        public static readonly MetaType varType = new(/*(type1, type2) => type1.AsVarType().Equals(type2.AsVarType())*/);
        public static readonly MetaType identifierType = new();
        public static readonly MetaType typeType = new(/*(type1, type2) => type1.AsTypeType().Equals(type2.AsTypeType())*/);
        public static readonly MetaType conditionalType = new();

        public static int NEXT_NUMBER = 0;

        // public Func<ZType, ZType, bool> EqualityChecker;

        public readonly int number;

        public MetaType(/*Func<ZType, ZType, bool> equalityChecker*/)
        {
            // EqualityChecker = equalityChecker;

            number = NEXT_NUMBER;
            NEXT_NUMBER++;
        }
    }

    /// <summary>
    /// Static fields of type StateDict and methods for creating transition lambdas to pass into the ZType constructor.
    /// </summary>
    static class T
    {
        /// <summary>
        /// The set of state indices for states that end in a value.
        /// </summary>
        public static HashSet<S> normalEndS = new()
        {
            S.Normal, S.Normal_Infix_Normal, S.LeftParen_Normal, S.If_Normal, S.If_Normal_Then_Normal,
            S.If_Normal_Then_Normal_Else_Normal, S.While_Normal, S.While_Normal_Do_Normal, S.Do_Normal,
            S.Do_Normal_While_Normal, S.Type_Normal, S.PrefixOrInfix_Normal, S.Prefix_Normal,
            S.Normal_PrefixOrInfix_Normal, S.Normal_PostfixOrInfix_Normal, S.Function_LeftParen_Args,
            S.Type_Normal_LeftParen_Params_Type_Normal, S.Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal,
        };

        /// <summary>
        /// The set of state indices for states that end in a value.
        /// </summary>
        public static HashSet<int> normalEnd = normalEndS.Select(item => (int)item).ToHashSet();

        /// <summary>
        /// The set of state indices for states that end in an operator (and that allow a value after them).
        /// </summary>
        public static HashSet<S> opEndS = new()
        {
            S.Initial, S.Normal_Infix, S.LeftParen, S.If, S.If_Normal_Then, S.If_Normal_Then_Normal_Else,
            S.While, S.While_Normal_Do, S.Do, S.Do_Normal_While, S.Type, S.Normal_PrefixOrInfix,
            S.Prefix, S.PrefixOrInfix, S.Normal_PostfixOrInfix, S.Function_LeftParen,
            S.Function_LeftParen_Args_Comma, S.Type_Normal_LeftParen, S.Type_Normal_LeftParen_Params_RightParen_LeftBrace,
        };

        /// <summary>
        /// The set of state indices for states that end in an operator (and that allow a value after them).
        /// </summary>
        public static HashSet<int> opEnd = opEndS.Select(item => (int)item).ToHashSet();

        /// <summary>
        /// State dictionary for values (anything that is not an operator or punctuation symbol).
        /// </summary>
        public static StateDict NormalDict = new()
        {
            { S.Initial, State.Normal },
            { S.Normal_Infix, State.Normal_Infix_Normal },
            { S.Prefix, State.Prefix_Normal },
            { S.PrefixOrInfix, State.PrefixOrInfix_Normal },
            { S.Normal_PrefixOrInfix, State.Normal_PrefixOrInfix_Normal },
            { S.Normal_PostfixOrInfix, State.Normal_PostfixOrInfix_Normal },
            { S.LeftParen, State.LeftParen_Normal },
            { S.If, State.If_Normal },
            { S.While, State.While_Normal },
            { S.Do, State.Do_Normal },
            { S.Type, State.Type_Normal },
            { S.Function_LeftParen, State.Function_LeftParen_Args(3) },
            { S.Function_LeftParen_Args_Comma, () => State.Function_LeftParen_Args(Z.LastNumToPop + 1) },
            { S.Type_Normal_LeftParen_Params_Type, () => State.Type_Normal_LeftParen_Params_Type_Normal(Z.LastNumToPop + 1) },
            { S.Type_Normal_LeftParen_Params_RightParen_LeftBrace, () => State.Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal(Z.LastNumToPop + 1) },
            { S.If, State.If_Normal },
            { S.If_Normal_Then, State.If_Normal_Then_Normal },
            { S.If_Normal_Then_Normal_Else, State.If_Normal_Then_Normal_Else_Normal },
        };

        /// <summary>
        /// State dictionary for an infix binary operator.
        /// </summary>
        public static StateDict InfixDict = new() { { normalEnd, State.Normal_Infix } };

        /// <summary>
        /// State dictionary for a prefix unary operator.
        /// </summary>
        public static StateDict PrefixDict = new() { { opEnd, State.Prefix } };

        /// <summary>
        /// State dictionary for a postfix unary operator.
        /// </summary>
        public static StateDict PostfixDict = new() { { normalEnd, State.Normal_Postfix } };

        /// <summary>
        /// State dictionary for an operator that can be prefix unary or infix binary.
        /// </summary>
        public static StateDict PrefixOrInfixDict = new() { { normalEnd, State.Normal_PrefixOrInfix }, { opEnd, State.PrefixOrInfix } };

        /// <summary>
        /// State dictionary for an operator that can be postfix unary or infix binary.
        /// </summary>
        public static StateDict PostfixOrInfixDict = new() { { normalEnd, State.Normal_PostfixOrInfix } };

        /// <summary>
        /// State dictionary for the left parenthesis.
        /// </summary>
        public static StateDict LeftParenDict = new()
        {
            { S.Function, State.Function_LeftParen },
            { S.Type_Normal, State.Type_Normal_LeftParen },
            { opEnd, State.LeftParen },
        };

        /// <summary>
        /// State dictionary for the right parenthesis.
        /// </summary>
        public static StateDict RightParenDict = new()
        {
            { S.LeftParen_Normal, State.LeftParen_Normal_RightParen },
            { S.Function_LeftParen, State.Function_LeftParen_Args_RightParen(3) },
            { S.Function_LeftParen_Args, () => State.Function_LeftParen_Args_RightParen(Z.LastNumToPop + 1) },
            { S.Type_Normal_LeftParen, State.Type_Normal_LeftParen_Params_RightParen(5) },
            { S.Type_Normal_LeftParen_Params_Type_Normal, () => State.Type_Normal_LeftParen_Params_RightParen(Z.LastNumToPop + 1) }
        };

        /// <summary>
        /// State dictionary for a type keyword.
        /// </summary>
        public static StateDict TypeDict = new()
        {
            { S.Type_Normal_LeftParen, State.Type_Normal_LeftParen_Params_Type(4) },
            { S.Type_Normal_LeftParen_Params_Type_Normal_Comma, () => State.Type_Normal_LeftParen_Params_Type(Z.LastNumToPop + 1) },
            { opEnd, State.Type },
        };

        /// <summary>
        /// State dictionary for a function.
        /// </summary>
        public static StateDict FunctionDict = new() { { opEnd, State.Function } };

        /// <summary>
        /// State dictionary for the comma.
        /// </summary>
        public static StateDict CommaDict = new()
        {
            { S.Function_LeftParen_Args, () => State.Function_LeftParen_Args_Comma(Z.LastNumToPop + 1) }
        };

        /// <summary>
        /// State dictionary for the left brace.
        /// </summary>
        public static StateDict LeftBraceDict = new()
        {
            { S.Type_Normal_LeftParen_Params_RightParen, () => State.Type_Normal_LeftParen_Params_RightParen_LeftBrace(Z.LastNumToPop + 1) }
        };

        /// <summary>
        /// State dictionary for the right brace.
        /// </summary>
        public static StateDict RightBraceDict = new()
        {
            { S.Type_Normal_LeftParen_Params_RightParen_LeftBrace, () => State.Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal_RightBrace(Z.LastNumToPop + 1) },
            { S.Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal, () => State.Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal_RightBrace(Z.LastNumToPop + 1) },
        };

        /// <summary>
        /// State dictionary for the 'if' keyword.
        /// </summary>
        public static StateDict IfDict = new()
        {
            { opEnd, State.If }
        };

        /// <summary>
        /// State dictionary for the 'then' keyword.
        /// </summary>
        public static StateDict ThenDict = new()
        {
            { S.If_Normal, State.If_Normal_Then }
        };

        /// <summary>
        /// State dictionary for the 'else' keyword.
        /// </summary>
        public static StateDict ElseDict = new()
        {
            { S.If_Normal_Then_Normal, State.If_Normal_Then_Normal_Else }
        };

        public static void NormalTransition(string typeName)
        {
            Transition(typeName, NormalDict);
        }

        public static void Transition(string typeName, Func<State, State> func)
        {
            ParseTree temp = Z.PopTree();

            while (true)
            {
                State oldState = Z.PeekTree.state;
                State newState = func(oldState);

                if (newState != State.Error)
                {
                    temp.state = newState;
                    Z.PushTree(temp);
                    return;
                }
                else if (oldState.canMerge)
                {
                    Z.Merge(oldState.numToPop);
                }
                else
                {
                    throw new ZevviException($"Syntax error: type {typeName} is not allowed at state {oldState}.");
                }
            }
        }

        public static void LeftPrecTransition(string typeName, Func<State, State> func, int leftPrec)
        {
            LeftPrecTransition(() => Transition(typeName, func), leftPrec);
        }

        public static void LeftPrecTransition(TransitionFunc InnerTransition, int leftPrec)
        {
            ParseTree temp = Z.PopTree();

            while (true)
            {
                LinkedListNode<ParseTree> node = Z.subtrees.Last;

                while (!(node is null))
                {
                    ParseTree subtree = node.Value;

                    if (subtree is RightPrecTree rsubtree)
                    {
                        int rightPrec = rsubtree.rightPrec;
                        State oldState = Z.subtrees.Last.Value.state;

                        if (leftPrec == rightPrec)
                        {
                            throw new ZevviException($"Expressions have equal precedences ({leftPrec}); parentheses required.");
                        }
                        else if (leftPrec > rightPrec || !oldState.canMerge)
                        {
                            Z.PushTree(temp);
                            InnerTransition();
                            return;
                        }
                        else
                        {
                            Z.Merge(oldState.numToPop);
                            break; // break linkedlist loop, cont outer 'while'
                        }
                    }

                    node = node.Previous;
                }
            }
        }

        public static void RightPrecTransition(string typeName, Func<State, State> func, int rightPrec)
        {
            RightPrecTransition(() => Transition(typeName, func), rightPrec);
        }

        public static void RightPrecTransition(TransitionFunc InnerTransition, int rightPrec)
        {
            InnerTransition();
            Z.PushTree(Z.PopTree().AttachRightPrec(rightPrec));
        }

        public static void BothPrecTransition(string typeName, Func<State, State> func, int leftPrec, int rightPrec)
        {
            BothPrecTransition(() => Transition(typeName, func), leftPrec, rightPrec);
        }

        public static void BothPrecTransition(TransitionFunc innerTransition, int leftPrec, int rightPrec)
        {
            RightPrecTransition(() => LeftPrecTransition(innerTransition, leftPrec), rightPrec);
        }
    }

    class StateDict : IEnumerable<Func<State, State>>
    {
        public List<Func<State, State>> list;

        public Func<State, State> this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        public State this[State state]
        {
            get => Get(state);
        }

        public StateDict()
        {
            list = new List<Func<State, State>>();
        }

        public StateDict(List<Func<State, State>> list)
        {
            this.list = list;
        }

        public State Get(State oldState)
        {
            foreach (Func<State, State> func in list)
            {
                State newState = func(oldState);

                if (newState != State.Null)
                {
                    return newState;
                }
            }

            return State.Error;
        }

        public void Add(Func<State, State> item)
        {
            list.Add(item);
        }

        public void Add(S key, State value)
        {
            list.Add((State state) => state.index == (int)key ? value : State.Null);
        }

        public void Add(S key, Func<State> getValue)
        {
            list.Add((State state) => state.index == (int)key ? getValue() : State.Null);
        }

        //public void Add(S key, Action action, State value)
        //{
        //    list.Add((State state) => { action(); return state.index == (int)key ? value : State.Null; });
        //}

        //public void Add(S key, Action action, Func<State> getValue)
        //{
        //    list.Add((State state) => { action(); return state.index == (int)key ? getValue() : State.Null; });
        //}

        public void Add(int key, State value)
        {
            list.Add((State state) => state.index == key ? value : State.Null);
        }

        public void Add(int key, Func<State> getValue)
        {
            list.Add((State state) => state.index == key ? getValue() : State.Null);
        }

        //public void Add(int key, Action action, State value)
        //{
        //    list.Add((State state) => { action(); return state.index == key ? value : State.Null; });
        //}

        public void Add(ISet<int> keys, State value)
        {
            list.Add((State state) => keys.Contains(state.index) ? value : State.Null);
        }

        public void Add(ISet<int> keys, Func<State> getValue)
        {
            list.Add((State state) => keys.Contains(state.index) ? getValue() : State.Null);
        }

        public void Add(IDictionary<State, State> dict)
        {
            list.Add((State state) => dict.ContainsKey(state) ? dict[state] : State.Null);
        }

        public void Remove(int index)
        {
            list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator<Func<State, State>> IEnumerable<Func<State, State>>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public static implicit operator Func<State, State>(StateDict dict)
        {
            return (State state) => dict[state];
        }
    }

    enum S
    {
        Null = 0,
        Error,
        Initial,
        Normal,
        Normal_Eof,
        Normal_Infix,
        Normal_Infix_Normal,
        Prefix,
        Prefix_Normal,
        Normal_Postfix,
        PrefixOrInfix,
        Normal_PrefixOrInfix,
        PrefixOrInfix_Normal,
        Normal_PrefixOrInfix_Normal,
        Normal_PostfixOrInfix,
        Normal_PostfixOrInfix_Normal,
        LeftParen,
        LeftParen_Normal,
        LeftParen_Normal_RightParen,
        If,
        If_Normal,
        If_Normal_Then,
        If_Normal_Then_Normal,
        If_Normal_Then_Normal_Else,
        If_Normal_Then_Normal_Else_Normal,
        While,
        While_Normal,
        While_Normal_Do,
        While_Normal_Do_Normal,
        Do,
        Do_Normal,
        Do_Normal_While,
        Do_Normal_While_Normal,
        Type,
        Type_Normal,
        Function,
        Function_LeftParen,
        Function_LeftParen_Args,
        Function_LeftParen_Args_Comma,
        Function_LeftParen_Args_RightParen,
        Type_Normal_LeftParen,
        Type_Normal_LeftParen_Params_RightParen_LeftBrace,
        Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal,
        Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal_RightBrace,
        Type_Normal_LeftParen_Params_Type,
        Type_Normal_LeftParen_Params_Type_Normal,
        Type_Normal_LeftParen_Params_Type_Normal_Comma,
        Type_Normal_LeftParen_Params_RightParen,
    }

    readonly struct State
    {
        public static readonly State Null = new(S.Null, 0);
        public static readonly State Error = new(S.Error, 0);

        public static readonly State Initial = new(S.Initial, 0);
        public static readonly State Normal = new(S.Normal, 1);
        public static readonly State Normal_Eof = new(S.Normal_Eof, 2,
            subtrees =>
            {
                ZI.Code voidConvert = subtrees[0].exprType.ImplicitConvert(ZType.Void);

                if (!voidConvert.HasValue)
                {
                    throw new ZevviException($"Type error: Program must be convertible to {ZType.Void}.");
                }

                return new ParseTree(subtrees, subtrees[0].exprType, subtrees[0].exprCode + voidConvert.Value + MI.Halt);
            });

        public static readonly State Normal_Infix = new(S.Normal_Infix, 2);
        public static readonly State Normal_Infix_Normal = new(S.Normal_Infix_Normal, 3, OpTypeMerge(1));

        public static readonly State Prefix = new(S.Prefix, 1);
        public static readonly State Prefix_Normal = new(S.Prefix_Normal, 2, OpTypeMerge(0));

        public static readonly State Normal_Postfix = new(S.Normal_Postfix, 2, OpTypeMerge(1));

        public static readonly State PrefixOrInfix = new(S.PrefixOrInfix, 1);
        public static readonly State Normal_PrefixOrInfix = new(S.Normal_PrefixOrInfix, 2);
        public static readonly State PrefixOrInfix_Normal = new(S.PrefixOrInfix_Normal, 2, OpTypeMerge(0));
        public static readonly State Normal_PrefixOrInfix_Normal = new(S.Normal_PrefixOrInfix_Normal, 3, OpTypeMerge(1));

        public static readonly State Normal_PostfixOrInfix = new(S.Normal_PostfixOrInfix, 2, OpTypeMerge(1));
        public static readonly State Normal_PostfixOrInfix_Normal = new(S.Normal_PostfixOrInfix_Normal, 3, OpTypeMerge(1));

        public static readonly State LeftParen = new(S.LeftParen, 1);
        public static readonly State LeftParen_Normal = new(S.LeftParen_Normal, 2);
        public static readonly State LeftParen_Normal_RightParen = new(S.LeftParen_Normal_RightParen, 3,
            subtrees => new ParseTree(subtrees, subtrees[1].exprType, subtrees[1].exprCode));

        public static readonly State If = new(S.If, 1);
        public static readonly State If_Normal = new(S.If_Normal, 2);
        public static readonly State If_Normal_Then = new(S.If_Normal_Then, 3);
        public static readonly State If_Normal_Then_Normal = new(S.If_Normal_Then_Normal, 4,
            subtrees =>
            {
                ParseTree condition = subtrees[1];
                ZI.Code conditionConvert = condition.exprType.ImplicitConvert(ZType.Bool);

                if (!conditionConvert.HasValue)
                {
                    throw new ZevviException($"Type error in subtree 1: Cannot convert from {condition.exprType} to {ZType.Bool}.");
                }

                return new ParseTree(subtrees, ConditionalType.IfThen(subtrees), condition.exprCode + conditionConvert.Value);
            });
        public static readonly State If_Normal_Then_Normal_Else = new(S.If_Normal_Then_Normal_Else, 5);
        public static readonly State If_Normal_Then_Normal_Else_Normal = new(S.If_Normal_Then_Normal_Else_Normal, 6,
            subtrees =>
            {
                ParseTree condition = subtrees[1];
                ZI.Code conditionConvert = condition.exprType.ImplicitConvert(ZType.Bool);

                if (!conditionConvert.HasValue)
                {
                    throw new ZevviException($"Type error in subtree 1: Cannot convert from {condition.exprType} to {ZType.Bool}.");
                }

                return new ParseTree(subtrees, ConditionalType.IfThenElse(subtrees), condition.exprCode + conditionConvert.Value);
            });

        public static readonly State While = new(S.While, 1);
        public static readonly State While_Normal = new(S.While_Normal, 2);
        public static readonly State While_Normal_Do = new(S.While_Normal_Do, 3);

        public static readonly State Do = new(S.Do, 1);
        public static readonly State Do_Normal = new(S.Do_Normal, 2);
        public static readonly State Do_Normal_While = new(S.Do_Normal_While, 3);

        public static readonly State Type = new(S.Type, 1);
        public static readonly State Type_Normal = new(S.Type_Normal, 2,
            subtrees =>
            {
                TypeType typeType = subtrees[0].exprType.AsTypeType();
                ZType thisType = typeType.thisType;
                string id = subtrees[1].exprType.AsIdentifierType().identifier;
                VarType varType = new("var " + thisType.name, id, thisType, Z.symbolTable.scopes.Peek().nextVarLoc++);
                Z.symbolTable.Add(id, varType);
                return Z.CombineTrees(subtrees, new HashSet<int> { }, new ZType[0], varType, ZI.Code.None);
            });

        public static readonly State Function = new(S.Function, 1);
        public static readonly State Function_LeftParen = new(S.Function_LeftParen, 2);

        public static State Function_LeftParen_Args(int numToPop) => new(S.Function_LeftParen_Args, numToPop);

        public static State Function_LeftParen_Args_Comma(int numToPop) => new(S.Function_LeftParen_Args_Comma, numToPop);

        public static State Function_LeftParen_Args_RightParen(int numToPop) => new(S.Function_LeftParen_Args_RightParen, numToPop, OpTypeMerge(0));

        public static readonly State Type_Normal_LeftParen = new(S.Type_Normal_LeftParen, 3);
        
            //subtrees =>
            //{
            //    HashSet<int> indices = new HashSet<int>();
            //    for (int i = 1; i < subtrees.Length - 1; i += 3)
            //    {
            //        indices.Add(i);
            //    }
            //    ZI.Code code = new ZI.Code(new SingleMI[] { new SingleMI(MI.Goto)})
            //    return Z.CombineTrees(subtrees, new HashSet<int> { }, new ZType[0], ZType.Void, code);
            //});

        public static State Type_Normal_LeftParen_Params_Type(int numToPop) => new(S.Type_Normal_LeftParen_Params_Type, numToPop);
        
        public static State Type_Normal_LeftParen_Params_Type_Normal(int numToPop) => new(S.Type_Normal_LeftParen_Params_Type_Normal, numToPop);
        
        public static State Type_Normal_LeftParen_Params_Type_Normal_Comma(int numToPop) => new(S.Type_Normal_LeftParen_Params_Type_Normal_Comma, numToPop);
        
        public static State Type_Normal_LeftParen_Params_RightParen(int numToPop) => new(S.Type_Normal_LeftParen_Params_RightParen, numToPop);

        public static State Type_Normal_LeftParen_Params_RightParen_LeftBrace(int numToPop) => new(
            S.Type_Normal_LeftParen_Params_RightParen_LeftBrace, numToPop);
        
        public static State Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal(int numToPop) => new(
            S.Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal, numToPop);
        
        public static State Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal_RightBrace(int numToPop) => new(
            S.Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal_RightBrace, numToPop,
            subtrees =>
            {
                StringBuilder typeNameBuilder = new("(");
                List<ZType> paramTypes = new();
                ZType returnType = subtrees[0].exprType.AsTypeType().thisType;

                for (int i = 0; i < subtrees.Length; i++)
                {
                    if (subtrees[i].state.index == (int)S.Type_Normal_LeftParen_Params_Type_Normal)
                    {
                        ZType paramType = subtrees[i - 1].exprType.AsTypeType().thisType;
                        if (paramTypes.Count > 0) typeNameBuilder.Append(", ");
                        typeNameBuilder.Append(paramType.ToString());
                        paramTypes.Add(paramType);
                    }
                }

                int callMark = Z.nextMark++;
                int returnMark = Z.nextMark++;

                typeNameBuilder.Append(") -> ");
                typeNameBuilder.Append(returnType.ToString());
                string typeName = typeNameBuilder.ToString();

                string funcName = subtrees[1].exprType.AsIdentifierType().identifier;
                OpType funcType = new FunctionType(typeName, callMark, NormalFunction(paramTypes.ToArray(), returnType));
                ZI.Code funcCode = new SingleMI(MI.PushCon, Z.symbolTable.scopes.Peek().nextVarLoc++);
                Z.symbolTable.Add(funcName, funcType, funcCode);

                ParseTree block = subtrees[^2];
                ZI.Code blockCode =
                    block.state.index == (int)S.Type_Normal_LeftParen_Params_RightParen_LeftBrace_Normal
                    ? block.exprCode 
                    : ZI.Code.None;
                ZI.Code code = new SingleMI(MI.GotoRelI, blockCode.Length + 2) + new SingleMI(MI.Mark, callMark) + blockCode + new SingleMI(MI.Mark, returnMark) + MI.Return;
                return new ParseTree(subtrees, ZType.Void, code);
            });

        public readonly int index;
        public readonly int numToPop;
        public readonly bool canMerge;
        public readonly MergeFunc Merge;

        public State(int index, int numToPop, MergeFunc Merge = null)
        {
            this.index = index;
            this.numToPop = numToPop;
            this.canMerge = Merge != null;
            this.Merge = Merge;
        }

        public State(S index, int numToPop, MergeFunc Merge = null) : this((int)index, numToPop, Merge)
        {
        }

        public override bool Equals(object other)
        {
            return other is State s && index == s.index;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(index);
        }

        public override string ToString()
        {
            try
            {
                return $"{(S)index}({numToPop})";
            }
            catch (InvalidCastException)
            {
                return $"State{index}({numToPop})";
            }
        }

        public static MergeFunc OpTypeMerge(int positionOfOperator)
        {
            return subtrees => subtrees[positionOfOperator].exprType.AsOpType().MergeSubtrees(subtrees);
        }

        public static GetReturnTypeFunc NormalFunction(ZType[] paramTypes, ZType returnType)
        {
            return argTypes =>
            {
                if (argTypes.Length != paramTypes.Length)
                {
                    return (null, null);
                }

                ZI.Code[] converts = new ZI.Code[paramTypes.Length];

                for (int i = 0; i < paramTypes.Length; i++)
                {
                    ZI.Code convertCode = argTypes[i].ImplicitConvert(paramTypes[i]);

                    if (convertCode.HasValue)
                    {
                        converts[i] = convertCode.Value;
                    }
                    else
                    {
                        return (null, null);
                    }
                }

                return (returnType, converts);

                // return argTypes.SequenceEqual(paramTypes) ? (returnType, code) : (null, ZI.Code.None);
            };
        }

        public static GetReturnTypeFunc OverloadedFunction(params GetReturnTypeFunc[] overloads)
        {
            return argTypes =>
            {
                foreach (GetReturnTypeFunc overload in overloads)
                {
                    var tuple = overload(argTypes);

                    if (!(tuple.returnType is null)) return tuple;
                }

                return (null, null);
            };
        }

        public static MergeFunc FunctionMerge(GetReturnTypeFunc getReturnType, int functionLocation)
        {
            return subtrees =>
            {
                ParseTree[] args = subtrees.Where((_, index) => index % 2 == 0 && index >= 2).ToArray();
                ZType[] argTypes = args.Select(item => item.exprType).ToArray();
                (ZType returnType, ZI.Code[] converts) = getReturnType(argTypes);

                if (returnType is null)
                {
                    throw new ZevviException($"Type error: Wrong argument types for function/operator: {string.Join<ZType>(", ", argTypes)}.");
                }

                // HashSet<int> indices = Enumerable.Range(0, argTypes.Length).Select(item => 2 * item + 2).ToHashSet();
                //return Z.CombineTrees(subtrees, new HashSet<int> { }, new ZType[0], returnType, new SingleMI(MI.CallMarkI, functionLocation));

                ZI.Code code = ZI.Code.None;

                for (int i = 0; i < argTypes.Length; i++)
                {
                    code += subtrees[2 * i + 2].exprCode + converts[i];
                }

                return new ParseTree(subtrees, returnType, code + new SingleMI(MI.CallMark, functionLocation));
            };
        }

        public static bool operator ==(State left, State right) => left.Equals(right);

        public static bool operator !=(State left, State right) => !left.Equals(right);
    }

    readonly struct SingleMI
    {
        public readonly MI instruction;
        public readonly int[] otherInts;

        public SingleMI(MI instruction, params int[] otherInts)
        {
            this.instruction = instruction;
            this.otherInts = otherInts;
        }

        public override string ToString()
        {
            return otherInts.Length == 0 ? $"{instruction}" : $"{instruction}({string.Join(", ", otherInts)})";
        }

        public static ZI.Code operator +(SingleMI left, SingleMI right)
        {
            return new ZI.Code(left, right);
        }
    }

    readonly struct ZI.Code
    {
        public static readonly ZI.Code None = new(new SingleMI[0]);

        public readonly SingleMI[] instructions;

        public int Length => instructions.Length;

        public SingleMI this[int index]
        {
            get => instructions[index];
            set => instructions[index] = value;
        }

        public ZI.Code(params SingleMI[] instructions)
        {
            this.instructions = instructions;
        }

        public static ZI.Code Combine(params ZI.Code[] instructions)
        {
            return instructions.Aggregate((c1, c2) => c1 + c2);
        }

        public static ZI.Code operator +(ZI.Code left, ZI.Code right)
        {
            return new ZI.Code(left.instructions.Concat(right.instructions).ToArray());
        }

        public static implicit operator ZI.Code(MI instruction)
        {
            return new ZI.Code(new SingleMI(instruction));
        }

        public static implicit operator ZI.Code(SingleMI instruction)
        {
            return new ZI.Code(instruction);
        }

        public override string ToString()
        {
            return instructions.Length == 0 ? "" : string.Join("; ", instructions) + ";";
        }
    }

    class SymbolTable
    {
        public readonly Stack<Scope> scopes = new();

        public void PushScope()
        {
            scopes.Push(new Scope(scopes.Count > 0 ? scopes.Peek().nextVarLoc : 0));
        }

        public SymbolTableEntry Get(string identifier)
        {
            foreach (Scope scope in scopes)
            {
                if (scope.entries.ContainsKey(identifier))
                {
                    return scope.entries[identifier];
                }
            }

            return new SymbolTableEntry(null, null, ZI.Code.None);
        }

        public void Add(string identifier, ZType type)
        {
            scopes.Peek().entries[identifier] = new SymbolTableEntry(identifier, type);
        }

        public void Add(string identifier, ZType type, ZI.Code code)
        {
            scopes.Peek().entries[identifier] = new SymbolTableEntry(identifier, type, code);
        }

        public void Remove(string identifier)
        {
            scopes.Peek().entries.Remove(identifier);
        }
    }

    class Scope
    {
        public readonly Dictionary<string, SymbolTableEntry> entries = new();
        public int firstVarLoc;
        public int nextVarLoc;

        public Scope(int firstVarLoc, int nextVarLoc)
        {
            this.firstVarLoc = firstVarLoc;
            this.nextVarLoc = nextVarLoc;
        }

        public Scope(int firstVarLoc) : this(firstVarLoc, firstVarLoc)
        {
        }
    }

    struct SymbolTableEntry
    {
        public string identifier;
        public ZType type;
        public ZI.Code code;

        public SymbolTableEntry(string identifier, ZType type)
        {
            this.identifier = identifier;
            this.type = type;
            this.code = ZI.Code.None;
        }

        public SymbolTableEntry(string identifier, ZType type, ZI.Code code)
        {
            this.identifier = identifier;
            this.type = type;
            this.code = code;
        }

        public void Deconstruct(out string identifier, out ZType type, out ZI.Code code)
        {
            identifier = this.identifier;
            type = this.type;
            code = this.code;
        }

        public void Deconstruct(out ZType type, out ZI.Code code)
        {
            type = this.type;
            code = this.code;
        }

        public override string ToString()
        {
            return code.Length > 0 ? $"('{identifier}', type {type}, code [{code}])" : $"('{identifier}', type {type})";
        }
    }

    class ZevviException : Exception
    {
        public ZevviException(string message) : base(message)
        {
        }
    }
}
