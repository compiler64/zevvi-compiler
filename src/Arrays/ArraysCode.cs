using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.Arrays
{
    public static class ArraysCode
    {
        /// <summary>
        /// ZI code for creating an array.
        /// </summary>
        /// <param name="length">The ZI storage that stores the length of the array.</param>
        /// <returns>The ZI code.</returns>
        public static ZI.Code Create(ZI.IOperand length)
        {
            // add one to the length, because the array length is stored before the values
            ZI.Code c1 = ZI.IOperand.AddConst(length, 1);
            // allocate space for array
            ZI.Triple t2 = new(ZI.Operator.Allocate, c1.storage);
            // store the length as the first field in the array
            ZI.Triple t3 = new(ZI.Operator.SetContents, t2, length);
            // return the allocated space
            return c1 + new ZI.Code(t2, t2, t3);
            // Code
            // (1): length + 1
            // (2): allocate (1)
            // (3): *(2) := length
        }

        /// <summary>
        /// ZI code for getting an item in an array by index.
        /// </summary>
        /// <param name="array">The ZI storage that stores the array.</param>
        /// <param name="index">The ZI storage that stores the index.</param>
        /// <returns>The ZI code.</returns>
        public static ZI.Code GetItem(ZI.IOperand array, ZI.IOperand index)
        {
            // add one to the index, because the array length is stored before the values
            ZI.Code c1 = ZI.IOperand.AddConst(index, 1);
            // get the value at the index in the array
            ZI.Triple t2 = new(ZI.Operator.IndexContents, array, index);
            // return the value
            return c1 + t2;
            // Code
            // (1): index + 1
            // (2): (*array)[(1)]
        }

        /// <summary>
        /// ZI code for setting an item in an array by index.
        /// </summary>
        /// <param name="array">The ZI storage that stores the array.</param>
        /// <param name="index">The ZI storage that stores the index.</param>
        /// <param name="value">The ZI storage that stores the new value.</param>
        /// <returns>The ZI code.</returns>
        public static ZI.Code SetItem(ZI.IOperand array, ZI.IOperand index, ZI.IOperand value)
        {
            // add one to the index, because the array length is stored before the values
            ZI.Code c1 = ZI.IOperand.AddConst(index, 1);
            // get the location of array[index]
            ZI.Triple t2 = new(ZI.Operator.IndexLocation, array, c1.storage);
            // set array[index] to the new value
            ZI.Triple t3 = new(ZI.Operator.SetContents, t2, value);
            // return void
            return c1 + new ZI.Code(null, t2, t3);
            // Code
            // (1): index + 1
            // (2): &(*array)[(1)]
            // (3): *(2) := value
        }

        /// <summary>
        /// ZI code for initializing an array.
        /// </summary>
        /// <param name="array">The ZI storage that stores the array.</param>
        /// <param name="initialItems">The ZI storages that store the initial items of the array.</param>
        /// <returns>The ZI code.</returns>
        public static ZI.Code Initialize(ZI.IOperand array, IList<ZI.IOperand> initialItems)
        {
            // the number of values to initialize
            int length = initialItems.Count;
            // the code for initialization
            ZI.Code code = ZI.Code.None;

            // for each i from 0 to length - 1, set array[i] to the i-th initial value
            for (int i = 0; i < length; i++)
            {
                code += SetItem(array, new ZI.Constant<int>(i), initialItems[i]);
            }

            // return void
            return code.WithStorage(null);
        }

        /// <summary>
        /// ZI code for array creation and initialization (used for array literals).
        /// </summary>
        /// <param name="initialItems">The ZI storages that store the initial items of the array.</param>
        /// <returns>The ZI code.</returns>
        public static ZI.Code CreateAndInitialize(IList<ZI.IOperand> initialItems)
        {
            // code for array creation
            ZI.Code creationCode = Create(new ZI.Constant<int>(initialItems.Count));
            // code for array initialization
            ZI.Code initializationCode = Initialize(creationCode.storage, initialItems);
            // return void
            return (creationCode + initializationCode).WithStorage(creationCode.storage);
        }
    }
}
