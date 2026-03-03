using System;
using System.Collections.Generic;
using System.Text;
using MI = ZevviCompiler.MachineInstruction;

namespace ZevviCompiler
{
    [Obsolete("Use the new ZevviCompiler.ZI namespace instead.")]
    static class MachineInstructionParams
    {
        public static readonly Dictionary<MI, int> numberOfMachineInstructionParameters = new()
        {
            [MI.Null] = 0,
            [MI.Push] = 0,
            [MI.Pop] = 0,
            [MI.PushI] = 1,
            [MI.PopI] = 1,
            [MI.PushCon] = 1,
            [MI.Add] = 0,
            [MI.Sub] = 0,
            [MI.Mul] = 0,
            [MI.Div] = 0,
            [MI.Mod] = 0,
            [MI.AddI] = 1,
            [MI.SubI] = 1,
            [MI.MulI] = 1,
            [MI.DivI] = 1,
            [MI.ModI] = 1,
            [MI.Neg] = 0,
            [MI.And] = 0,
            [MI.Or] = 0,
            [MI.Xor] = 0,
            [MI.Not] = 0,
            [MI.AndI] = 1,
            [MI.OrI] = 1,
            [MI.XorI] = 1,
            [MI.IsZero] = 0,
            [MI.IsPos] = 0,
            [MI.IsNeg] = 0,
            [MI.Goto] = 0,
            [MI.GotoRel] = 0,
            [MI.If] = 0,
            [MI.IfRel] = 0,
            [MI.IfNot] = 0,
            [MI.IfNotRel] = 0,
            [MI.IfPos] = 0,
            [MI.IfPosRel] = 0,
            [MI.IfNeg] = 0,
            [MI.IfNegRel] = 0,
            [MI.Call] = 0,
            [MI.CallRel] = 0,
            [MI.Return] = 0,
            [MI.GotoI] = 1,
            [MI.GotoRelI] = 1,
            [MI.IfI] = 1,
            [MI.IfRelI] = 1,
            [MI.IfNotI] = 1,
            [MI.IfNotRelI] = 1,
            [MI.IfPosI] = 1,
            [MI.IfPosRelI] = 1,
            [MI.IfNegI] = 1,
            [MI.IfNegRelI] = 1,
            [MI.CallI] = 1,
            [MI.CallRelI] = 1,
            [MI.ClrStk] = 0,
            [MI.Swap] = 0,
            [MI.Dup] = 0,
            [MI.Remove] = 0,
            [MI.SysExec] = 1,
            [MI.Mark] = 1,
            [MI.CallMark] = 1,
            [MI.GotoMark] = 1,
            [MI.Halt] = 0,
        };
    }
}
