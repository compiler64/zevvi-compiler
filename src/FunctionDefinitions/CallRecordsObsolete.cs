using System;
using System.Collections.Generic;
using System.Text;
using MI = ZevviCompiler.MachineInstruction;

namespace ZevviCompiler.FunctionDefinitions
{
    [Obsolete("Activation records are handled by the code generator now.", error: true)]
    public static class CallRecordsObsolete
    {
        public const int CallRecordLoc = int.MaxValue / 2 + 1;
        public const int NextRecordLoc = int.MaxValue / 2;

        public static Code GetLength()
        {
            return new SingleMI(MI.PushI, CallRecordLoc);
        }

        public static Code GetLastRecord()
        {
            return GetLength() + new Code
            (
                new SingleMI(MI.AddI, CallRecordLoc),
                new SingleMI(MI.Push)
            );
        }

        public static Code AddToNextRecordLoc(int number)
        {
            return new Code
            (
                new SingleMI(MI.PushI, NextRecordLoc),
                new SingleMI(MI.AddI, number),
                new SingleMI(MI.PopI, NextRecordLoc)
            );
        }

        public static Code Add(int numParams)
        {
            return new Code
            (
                // increment the length
                new SingleMI(MI.PushI, CallRecordLoc),  // push the length
                new SingleMI(MI.AddI, 1),               // add 1 to the length
                new SingleMI(MI.Dup),                   // duplicate the sum to use later
                new SingleMI(MI.PopI, CallRecordLoc),   // set the length to the sum
                                                        // create the new CallRecord object
                new SingleMI(MI.AddI, CallRecordLoc),   // get the location of the list pointer to the new CallRecord
                new SingleMI(MI.PushI, NextRecordLoc),  // get the next record location
                new SingleMI(MI.Swap),                  // swap them for the Pop command
                new SingleMI(MI.Pop)                    // save the next record location in the new list item
            ) + AddToNextRecordLoc(numParams);
        }

        public static Code PushParameter(int paramNum)
        {
            return GetLastRecord() + new Code
            (
                new SingleMI(MI.AddI, paramNum),
                new SingleMI(MI.Push)
            );
        }

        public static Code PopParameter(int paramNum)
        {
            return GetLastRecord() + new Code
            (
                new SingleMI(MI.AddI, paramNum),
                new SingleMI(MI.Pop)
            );
        }
    }
}
