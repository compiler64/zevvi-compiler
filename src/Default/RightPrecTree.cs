namespace ZevviCompiler
{
    public class RightPrecTree : ParseTree
    {
        public int rightPrec;

        public RightPrecTree(ParseTree[] subtrees, IType exprType, ZI.Code exprCode, State state, Nonterminal nonterminal, int rightPrec)
            : base(subtrees, state, exprType, exprCode, nonterminal)
        {
            this.rightPrec = rightPrec;
        }

        public override RightPrecTree AttachRightPrec(int rightPrec)
        {
            throw new ZevviInternalCompilerError("Cannot attach second right precedence to parse tree.");
        }
    }
}
