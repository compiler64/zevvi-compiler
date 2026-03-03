using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.FunctionDefinitions
{
    [Obsolete("Activation records are handled by the code generator now.", error: true)]
    public static class ActivationRecords
    {
        public const int maxNumberOfRecords = 100;
        //public const int maxLengthOfAllRecords = 1000;

        public static readonly ZI.GlobalStorage numberOfRecords = ZI.GlobalStorage.New;
        public static ZI.Triple stackLocation;
        //public static readonly ZI.GlobalStorage nextLocation = ZI.GlobalStorage.New;
        //public static readonly ZI.GlobalStorage firstLocation = ZI.GlobalStorage.New;
        //public static readonly ZI.GlobalStorage lastLocation = ZI.GlobalStorage.New;

        //public static ZI.Code First => new ZI.Triple(ZI.Operator.ContentsOf, firstLocation);
        //public static ZI.Code Last => new ZI.Triple(ZI.Operator.ContentsOf, lastLocation);

        public static ZI.Code Peek => new ZI.Triple(ZI.Operator.In);

        public static ZI.Code Setup()
        {
            stackLocation = new(ZI.Operator.Allocate, new ZI.Constant<int>(maxNumberOfRecords));
            return new ZI.Code(null, stackLocation);
        }

        public static ZI.Code Create(int numLocals)
        {
            // add 1 to numberOfRecords
            ZI.Triple t1 = new(ZI.Operator.Add, numberOfRecords, new ZI.Constant<int>(1));
            // save the result in numberOfRecords
            ZI.Triple t2 = new(ZI.Operator.Copy, numberOfRecords, t1);
            // allocate space for the new record
            ZI.Triple t3 = new(ZI.Operator.Allocate, new ZI.Constant<int>(numLocals));
            // save a reference to the new record in the record stack
            ZI.Triple t4 = new(ZI.Operator.)
        }

        public static ZI.Code GetLocal(int paramNum)
        {
            throw new NotImplementedException();
        }
    }
}
