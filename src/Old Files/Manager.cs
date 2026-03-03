using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZevviCompilerOld
{
    public class Manager
    {
        // public const int ObjectGroup = 0;

        public readonly Storage memory = new Storage(); /* => storageGroups[ObjectGroup]; */

        //public int ObjSize => memory.Size;

        // public Storage GlobalStorage => storageGroups[1];
        // public Storage LocalStorage => storageGroups[^1];

        // the following are initialized when Initializer.Initialize(this) is called

        public ClassRef ObjectTypeRef;

        public TypeRef IntTypeRef;

        public ClassRef TypeTypeRef;

        public ClassRef ClassTypeRef;

        public TypeRef ArrayTypeRef;

        public TypeRef StringTypeRef;

        public TypeRef BoolTypeRef;

        public TypeRef FuncTypeRef;

        public Ref TypeCounterRef;

        // end of Initializer-initialized properties

        public int TypeCounter { get => Load(TypeCounterRef); set => Store(TypeCounterRef, value); }

        // private readonly List<Storage> storageGroups = new List<Storage>();
        public readonly Stack<int> stack = new Stack<int>();

        internal int counter;
        private bool halt;
        internal readonly Stack<int> returnLocs = new Stack<int>();

        public int Load(int location)
        {
            return memory[location];
        }

        //public int Load(Index group, int location)
        //{
        //    return storageGroups[group][location];
        //}

        public Ref Load<T>(int location, TypeRef checkType) where T : Ref
        {
            return Ref.Create<T>(this, memory[location], checkType);
        }

        public void Store(int location, int value)
        {
            memory[location] = value;
        }

        //public void Store(Index group, int location, int value)
        //{
        //    storageGroups[group][location] = value;
        //}

        public int NextLoc(int size, int value = 0)
        {
            int loc = memory.Size;
            memory.Expand(size);
            Store(loc, value);
            return loc;
        }

        public void Execute(int location)
        {
            counter = location;
            halt = false;
            returnLocs.Clear();

            while (!halt)
            {
                Machine.ExecuteInstruction(this, counter, out counter, Load(counter));
            }
        }

        public void Halt()
        {
            halt = true;
        }

        public int CreateObject(int typeRef, int size)
        {
            return NextLoc(size, typeRef);
        }

        public int CreateType(int nameRef, int staticFieldNamesRef, int staticFieldTypesRef)
        {
            int objLoc = memory.Size;
            memory.Expand(FieldLocs.TypeSize);

            Store(objLoc + FieldLocs.Type, TypeTypeRef);
            Store(objLoc + FieldLocs.Name, nameRef);
            Store(objLoc + FieldLocs.TypeNumber, TypeCounter++);
            Store(objLoc + FieldLocs.StaticFieldNames, staticFieldNamesRef);
            Store(objLoc + FieldLocs.StaticFieldTypes, staticFieldTypesRef);

            return objLoc;
        }

        public int CreateClass(int nameRef, int fieldNamesRef, int fieldTypesRef,
            int baseClasses, int staticFieldNamesRef, int staticFieldTypesRef)
        {
            int objLoc = NextLoc(FieldLocs.ClassSize);

            Store(objLoc + FieldLocs.Type, TypeTypeRef);
            Store(objLoc + FieldLocs.FieldNames, fieldNamesRef);
            Store(objLoc + FieldLocs.FieldTypes, fieldTypesRef);
            Store(objLoc + FieldLocs.BaseClasses, baseClasses);
            Store(objLoc + FieldLocs.Name, nameRef);
            Store(objLoc + FieldLocs.TypeNumber, TypeCounter++);
            Store(objLoc + FieldLocs.StaticFieldNames, staticFieldNamesRef);
            Store(objLoc + FieldLocs.StaticFieldTypes, staticFieldTypesRef);

            return objLoc;
        }

        public int Array(params int[] items)
        {
            int len = items.Length;
            int objLoc = NextLoc(len + 1, len);

            for (int i = 0; i < len; i++)
            {
                Store(objLoc + i + 1, items[i]);
            }

            return objLoc;
        }

        public int Array(params Ref[] items)
        {
            return Array(items.Select(@ref => @ref.location).ToArray());
        }

        public int StringArray(params string[] items)
        {
            return Array(items.Select(item => String(item)).ToArray());
        }

        public int String(string str)
        {
            return Array(str.Select(chr => (int)chr).ToArray());
        }

        public int Function(params int[] instructions)
        {
            int len = instructions.Length;
            int objLoc = NextLoc(len);

            for (int i = 0; i < len; i++)
            {
                Store(objLoc + i, instructions[i]);
            }

            return objLoc;
        }

        public string StringValue(int stringLoc)
        {
            int size = Load(stringLoc);
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < size; i++)
            {
                builder.Append((char)Load(stringLoc + i + 1));
            }

            return builder.ToString();
        }

        //public void PushStorage()
        //{
        //    storageGroups.Add(new Storage());
        //}

        public void Reset()
        {
            //storageGroups.Clear();
            memory.Clear();
        }
    }
}
