using System;
using System.Collections.Generic;
using System.Text;
using static ZevviCompilerOld.MachineInstruction;

namespace ZevviCompilerOld
{
    static class Machine
    {
        public static void ExecuteInstruction(Manager m, int currentLoc, out int newLoc, int instruction)
        {
            ExecuteInstruction(m, currentLoc, out newLoc, (MachineInstruction)instruction);
        }

        public static void ExecuteInstruction(Manager m, int currentLoc, out int newLoc, MachineInstruction instruction)
        {
            Console.WriteLine($"Executing instruction: {instruction}");
            int i;
            int j;
            switch (instruction)
            {
                case Null:
                    newLoc = currentLoc + 1;
                    break;
                case Push:
                    m.stack.Push(m.Load(m.stack.Pop()));
                    newLoc = currentLoc + 1;
                    break;
                //case PushLoc:
                //    m.stack.Push(m.Load(^1, m.stack.Pop()));
                //    newLoc = currentLoc + 1;
                //    break;
                //case PushGlb:
                //    m.stack.Push(m.Load(1, m.stack.Pop()));
                //    newLoc = currentLoc + 1;
                //    break;
                case Pop:
                    m.Store(m.stack.Pop(), m.stack.Pop());
                    newLoc = currentLoc + 1;
                    break;
                case PushI:
                    m.stack.Push(m.Load(m.Load(currentLoc + 1)));
                    newLoc = currentLoc + 2;
                    break;
                case PushCon:
                    m.stack.Push(m.Load(currentLoc + 1));
                    newLoc = currentLoc + 2;
                    break;
                case PopI:
                    m.Store(m.Load(currentLoc + 1), m.stack.Pop());
                    newLoc = currentLoc + 2;
                    break;
                //case PopLocI:
                //    m.Store(^1, m.Load(currentLoc + 1), m.stack.Pop());
                //    newLoc = currentLoc + 2;
                //    break;
                //case PopGlbI:
                //    m.Store(1, m.Load(currentLoc + 1), m.stack.Pop());
                //    newLoc = currentLoc + 2;
                //    break;
                case StoreI:
                    m.Store(m.Load(currentLoc + 1), m.Load(currentLoc + 2));
                    newLoc = currentLoc + 3;
                    break;
                //case StoLocI:
                //    m.Store(^1, m.Load(currentLoc + 1), m.Load(currentLoc + 2));
                //    newLoc = currentLoc + 3;
                //    break;
                //case StoGlbI:
                //    m.Store(1, m.Load(currentLoc + 1), m.Load(currentLoc + 2));
                //    newLoc = currentLoc + 3;
                //    break;
                case StoNxt:
                    m.memory.Expand(1);
                    m.Store(m.memory.Size - 1, m.stack.Pop());
                    newLoc = currentLoc + 1;
                    break;
                case StoNxtI:
                    m.memory.Expand(1);
                    m.Store(m.memory.Size - 1, m.Load(currentLoc + 1));
                    newLoc = currentLoc + 2;
                    break;
                case Next:
                    m.stack.Push(m.memory.Size);
                    newLoc = currentLoc + 1;
                    break;
                //case NxtLoc:
                //    m.stack.Push(m.LocalStorage.Size);
                //    newLoc = currentLoc + 1;
                //    break;
                //case NxtGlb:
                //    m.stack.Push(m.GlobalStorage.Size);
                //    newLoc = currentLoc + 1;
                //    break;
                case Add:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    m.stack.Push(i + j);
                    newLoc = currentLoc + 1;
                    break;
                case Sub:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    m.stack.Push(i - j);
                    newLoc = currentLoc + 1;
                    break;
                case Mul:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    m.stack.Push(i * j);
                    newLoc = currentLoc + 1;
                    break;
                case Div:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    m.stack.Push(i / j);
                    newLoc = currentLoc + 1;
                    break;
                case Mod:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    m.stack.Push(i % j);
                    newLoc = currentLoc + 1;
                    break;
                case AddI:
                    m.stack.Push(m.stack.Pop() + m.Load(currentLoc + 1));
                    newLoc = currentLoc + 2;
                    break;
                case SubI:
                    m.stack.Push(m.stack.Pop() - m.Load(currentLoc + 1));
                    newLoc = currentLoc + 2;
                    break;
                case MulI:
                    m.stack.Push(m.stack.Pop() * m.Load(currentLoc + 1));
                    newLoc = currentLoc + 2;
                    break;
                case DivI:
                    m.stack.Push(m.stack.Pop() / m.Load(currentLoc + 1));
                    newLoc = currentLoc + 2;
                    break;
                case ModI:
                    m.stack.Push(m.stack.Pop() % m.Load(currentLoc + 1));
                    newLoc = currentLoc + 2;
                    break;
                case Neg:
                    m.stack.Push(-m.stack.Pop());
                    newLoc = currentLoc + 1;
                    break;
                case And:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    m.stack.Push(i & j);
                    newLoc = currentLoc + 1;
                    break;
                case Or:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    m.stack.Push(i | j);
                    newLoc = currentLoc + 1;
                    break;
                case Xor:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    m.stack.Push(i ^ j);
                    newLoc = currentLoc + 1;
                    break;
                case Not:
                    m.stack.Push(m.stack.Pop() == 0 ? 1 : 0);
                    newLoc = currentLoc + 1;
                    break;
                case AndI:
                    m.stack.Push(m.stack.Pop() & m.Load(currentLoc + 1));
                    newLoc = currentLoc + 2;
                    break;
                case OrI:
                    m.stack.Push(m.stack.Pop() | m.Load(currentLoc + 1));
                    newLoc = currentLoc + 2;
                    break;
                case XorI:
                    m.stack.Push(m.stack.Pop() ^ m.Load(currentLoc + 1));
                    newLoc = currentLoc + 2;
                    break;
                case Goto:
                    newLoc = m.stack.Pop();
                    break;
                case GotoRel:
                    newLoc = currentLoc + m.stack.Pop();
                    break;
                case Call:
                    m.returnLocs.Push(currentLoc + 1);
                    newLoc = m.stack.Pop();
                    break;
                case CallRel:
                    m.returnLocs.Push(currentLoc + 1);
                    newLoc = currentLoc + m.stack.Pop();
                    break;
                case Ret:
                    newLoc = m.returnLocs.Pop();
                    break;
                case GotoI:
                    newLoc = m.Load(currentLoc + 1);
                    break;
                case GotoRelI:
                    newLoc = currentLoc + m.Load(currentLoc + 1);
                    break;
                case CallI:
                    m.returnLocs.Push(currentLoc + 2);
                    newLoc = m.Load(currentLoc + 1);
                    break;
                case CallRelI:
                    m.returnLocs.Push(currentLoc + 1);
                    newLoc = currentLoc + m.Load(currentLoc + 1);
                    break;
                case PrgCtr:
                    m.stack.Push(currentLoc);
                    newLoc = currentLoc + 1;
                    break;
                case ClrStk:
                    m.stack.Clear();
                    newLoc = currentLoc + 1;
                    break;
                case Swap:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    m.stack.Push(j);
                    m.stack.Push(i);
                    newLoc = currentLoc + 1;
                    break;
                case Dup:
                    m.stack.Push(m.stack.Peek());
                    newLoc = currentLoc + 1;
                    break;
                case Print:
                    Console.Write(m.StringValue(m.stack.Peek()));
                    newLoc = currentLoc + 1;
                    break;
                case Halt:
                    m.Halt();
                    newLoc = -1;
                    break;
                case If:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    if (i != 0)
                        newLoc = j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfRel:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    if (i != 0)
                        newLoc = currentLoc + j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfNot:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    if (i == 0)
                        newLoc = j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfNotRel:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    if (i == 0)
                        newLoc = currentLoc + j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfPos:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    if (i > 0)
                        newLoc = j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfPosRel:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    if (i > 0)
                        newLoc = currentLoc + j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfNeg:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    if (i < 0)
                        newLoc = j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfNegRel:
                    j = m.stack.Pop();
                    i = m.stack.Pop();
                    if (i < 0)
                        newLoc = currentLoc + j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfI:
                    j = m.Load(currentLoc + 1);
                    i = m.stack.Pop();
                    if (i != 0)
                        newLoc = j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfRelI:
                    j = m.Load(currentLoc + 1);
                    i = m.stack.Pop();
                    if (i != 0)
                        newLoc = currentLoc + j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfNotI:
                    j = m.Load(currentLoc + 1);
                    i = m.stack.Pop();
                    if (i == 0)
                        newLoc = j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfNotRelI:
                    j = m.Load(currentLoc + 1);
                    i = m.stack.Pop();
                    if (i == 0)
                        newLoc = currentLoc + j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfPosI:
                    j = m.Load(currentLoc + 1);
                    i = m.stack.Pop();
                    if (i > 0)
                        newLoc = j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfPosRelI:
                    j = m.Load(currentLoc + 1);
                    i = m.stack.Pop();
                    if (i > 0)
                        newLoc = currentLoc + j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfNegI:
                    j = m.Load(currentLoc + 1);
                    i = m.stack.Pop();
                    if (i < 0)
                        newLoc = j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case IfNegRelI:
                    j = m.Load(currentLoc + 1);
                    i = m.stack.Pop();
                    if (i < 0)
                        newLoc = currentLoc + j;
                    else
                        newLoc = currentLoc + 1;
                    break;
                case Remove:
                    m.stack.Pop();
                    newLoc = currentLoc + 1;
                    break;
                default:
                    throw new ZevviException($"Unknown machine code instruction number: {instruction}.");
            }
        }
    }
}
