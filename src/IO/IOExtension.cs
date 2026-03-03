using System;
using System.Collections.Generic;
using System.Text;
using ZevviCompiler.OperatorSyntaxes;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.IO
{
    /// <summary>
    /// A Zevvi compiler extension with the 'print' command.<br/>
    /// Requirements: <see cref="OperatorSyntaxesExtension"/>.
    /// </summary>
    public class IOExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(OperatorSyntaxesExtension) };

        public OperatorSyntaxesExtension OpExt => Z.GetExtension<OperatorSyntaxesExtension>();

        public const int P_PRINT_R = 4000;

        public ZType Printable;

        public override void InitConverts()
        {
            Z.Int.Converter.ImplicitConvert.Add(() => Printable, storage => new ZI.Triple(ZI.Operator.IntToString, storage));
            Z.Float.Converter.ImplicitConvert.Add(() => Printable, storage => new ZI.Triple(ZI.Operator.FloatToString, storage));
            Z.String.Converter.ImplicitConvert.Add(() => Printable, storage => ZI.Code.None);
        }

        public override void InitStates()
        {
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("print", new OperatorType(Z, "'print'")
            {
                Transition = new RightPrecTransition(P_PRINT_R, "'print'", OpExt.PrefixDict),
                MergeSubtrees = subtrees => ParseTree.CombineTrees(subtrees, new HashSet<int> { 1 },
                    new[] { Printable }, Z.Void,
                    storages => new ZI.Triple(ZI.Operator.Output, storages[0]), OpExt.nPrefixOperator),
            });

            Z.symbolTable.Add("input", new OperatorType(Z, "'input'")
            {
                Transition = new NormalTransition("'input'", OpExt.StandaloneDict),
                MergeSubtrees = subtrees => ParseTree.CombineTrees(subtrees, new HashSet<int>(),
                    Array.Empty<IType>(), Z.String,
                    storages => new ZI.Triple(ZI.Operator.Input), OpExt.nStandaloneOperator),
            });
        }

        public override void InitTransitions()
        {
        }

        public override void InitTypes()
        {
            Printable = ZType.WithNormalConverts(Z, "Printable", new NormalTransition("Printable", Z.NormalDict));
        }
    }
}
