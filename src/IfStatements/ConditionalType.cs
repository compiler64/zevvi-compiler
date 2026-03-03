using System;

namespace ZevviCompiler.IfStatements
{
    [Obsolete("If statements no longer use conditional types.")]
    public class ConditionalType : ZType
    {
        public static readonly ConvertDict ConditionalConvert = new()
        {
            (oldType, newType) =>
            {
                ConditionalType ctype = oldType.As<ConditionalType>();
                ZI.Code thenConvert = ctype.thenType.ImplicitConvert(newType)(ctype.thenStorage);
                ZI.Code elseConvert = ctype.elseType.ImplicitConvert(newType)(ctype.elseStorage);

                //if (!thenConvert.HasValue)
                //{
                //    throw new ZevviTypeError($"Type error in 'then' block: Cannot convert from " +
                //        $"{ctype.thenType} to {newType}.", );
                //}
                //else if (!elseConvert.HasValue)
                //{
                //    throw new ZevviTypeError($"Type error in 'else' block: Cannot convert from " +
                //        $"{ctype.elseType} to {newType}.", );
                //}
                if (thenConvert is null || elseConvert is null)
                {
                    return null;
                }

                ZI.Code thenWithConvert = ctype.thenCode + thenConvert;
                ZI.Code elseWithConvert = ctype.elseCode + elseConvert;
                ZI.Triple afterElse = ZI.Triple.DoNothing;
                return storage => new ZI.Triple(ZI.Operator.IfZero, elseWithConvert.Start)
                    /*new SingleMI(MachineInstruction.IfNotRelI, thenWithConvert.Length + 2)*/
                    + thenWithConvert
                    + new ZI.Triple(ZI.Operator.Goto, afterElse)
                    /*new SingleMI(MachineInstruction.GotoRelI, elseWithConvert.Length + 1)*/
                    + elseWithConvert;
            }
        };

        public readonly ZType thenType;
        public readonly ZI.IOperand thenStorage;
        public readonly ZI.Code thenCode;
        public readonly ZType elseType;
        public readonly ZI.IOperand elseStorage;
        public readonly ZI.Code elseCode;

        public ConditionalType(DefaultExtension z, ZType thenType, ZI.Code thenCode, ZType elseType, ZI.Code elseCode)
            : base(z, $"({thenType} | {elseType})", () => z.NormalTransition($"({thenType} | {elseType})"),
            ConditionalConvert, Converts.Self, Converts.None)
        {
            this.thenType = thenType;
            this.thenCode = thenCode;
            this.elseType = elseType;
            this.elseCode = elseCode;
        }

        //public static ConditionalType IfThen(DefaultExtension z, ParseTree[] subtrees)
        //{
        //    return new ConditionalType(z, subtrees[4].exprType, subtrees[4].exprCode, z.Void, ZI.Code.None);
        //}

        //public static ConditionalType IfThenElse(DefaultExtension z, ParseTree[] subtrees)
        //{
        //    return new ConditionalType(z, subtrees[4].exprType, subtrees[4].exprCode, subtrees[6].exprType, subtrees[6].exprCode);
        //}
    }
}
