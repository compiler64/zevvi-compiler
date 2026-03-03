using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZevviCompilerOld
{
    class InitTools
    {
        public readonly Manager m;

        private readonly Dictionary<string, int> currentTypeRefs;
        private readonly Dictionary<string, List<int>> futureTypeRefs;
        private readonly Dictionary<string, int> stringRefs;
        
        public InitTools(Manager m)
        {
            this.m = m;
            currentTypeRefs = new Dictionary<string, int>();
            futureTypeRefs = new Dictionary<string, List<int>>();
            stringRefs = new Dictionary<string, int>();
        }

        public int GetTypeLoc(int currentLoc, string typeName)
        {
            if (currentTypeRefs.ContainsKey(typeName))
            {
                return currentTypeRefs[typeName];
            }
            else if (futureTypeRefs.ContainsKey(typeName))
            {
                futureTypeRefs[typeName].Add(currentLoc);
            }
            else
            {
                futureTypeRefs[typeName] = new List<int>(new[] { currentLoc });
            }

            return -1;
        }

        public void StoreTypeRef(int location, string typeName)
        {
            m.Store(location, GetTypeLoc(location, typeName));
        }

        public void ReplaceFutureTypeRefs(string typeName, int loc)
        {
            currentTypeRefs[typeName] = loc;

            if (futureTypeRefs.ContainsKey(typeName))
            {
                foreach (int @ref in futureTypeRefs[typeName])
                {
                    m.Store(@ref, loc);
                }

                futureTypeRefs.Remove(typeName);
            }
        }

        public int Type(string name, int staticFieldNamesRef, int staticFieldTypesRef, int staticFieldValuesRef)
        {
            int loc = m.NextLoc(FieldLocs.TypeSize);

            StoreTypeRef(loc, "Type");
            m.Store(loc + FieldLocs.Name, String(name));
            m.Store(loc + FieldLocs.TypeNumber, m.TypeCounter++);
            m.Store(loc + FieldLocs.StaticFieldNames, staticFieldNamesRef);
            m.Store(loc + FieldLocs.StaticFieldTypes, staticFieldTypesRef);
            m.Store(loc + FieldLocs.StaticFieldValues, staticFieldValuesRef);

            ReplaceFutureTypeRefs(name, loc);
            return loc;
        }

        public int Class(string name, int fieldNamesRef, int fieldTypesRef,
            int baseClassesRef, int staticFieldNamesRef, int staticFieldTypesRef, int staticFieldValuesRef)
        {
            int loc = m.NextLoc(FieldLocs.ClassSize);

            StoreTypeRef(loc, "Class");
            m.Store(loc + FieldLocs.Name, String(name));
            m.Store(loc + FieldLocs.TypeNumber, m.TypeCounter++);
            m.Store(loc + FieldLocs.FieldNames, fieldNamesRef);
            m.Store(loc + FieldLocs.FieldTypes, fieldTypesRef);
            m.Store(loc + FieldLocs.BaseClasses, baseClassesRef);
            m.Store(loc + FieldLocs.StaticFieldNames, staticFieldNamesRef);
            m.Store(loc + FieldLocs.StaticFieldTypes, staticFieldTypesRef);
            m.Store(loc + FieldLocs.StaticFieldValues, staticFieldValuesRef);

            ReplaceFutureTypeRefs(name, loc);
            return loc;
        }

        public int Array(IList<FutureRef> values)
        {
            int length = values.Count;
            int loc = m.NextLoc(length + 1);

            m.Store(loc, length);

            for (int i = 0; i < length; i++)
            {
                m.Store(loc + i + 1, values[i].GetRef(this, loc + i + 1));
            }

            return loc;
        }

        public int TypeArray(IList<string> types)
        {
            int length = types.Count;
            int loc = m.NextLoc(length + 1);

            m.Store(loc, length);

            for (int i = 0; i < length; i++)
            {
                StoreTypeRef(loc + i + 1, types[i]);
            }

            return loc;
        }

        public int StringArray(IList<string> strings)
        {
            return Array(strings.Select(str => (FutureRef)String(str)).ToArray());
        }

        public int Array(params FutureRef[] values)
        {
            return Array((IList<FutureRef>)values);
        }

        public int TypeArray(params string[] types)
        {
            return TypeArray((IList<string>)types);
        }

        public int StringArray(params string[] strings)
        {
            return StringArray((IList<string>)strings);
        }

        public int Function(params FutureRef[] instructions)
        {
            int length = instructions.Length;
            int loc = m.NextLoc(length);

            for (int i = 0; i < length; i++)
            {
                m.Store(loc + i, instructions[i].GetRef(this, loc + i));
            }

            return loc;
        }

        public int String(string str)
        {
            if (stringRefs.ContainsKey(str))
            {
                return stringRefs[str];
            }
            else
            {
                int length = str.Length;
                int loc = m.NextLoc(length + 1);

                m.Store(loc, length);

                for (int i = 0; i < length; i++)
                {
                    m.Store(loc + i + 1, str[i]);
                }

                stringRefs[str] = loc;
                return loc;
            }
        }

        public void InitOrThrowError<T>(out T field, string typeName) where T : Ref
        {
            if (currentTypeRefs.ContainsKey(typeName))
            {
                field = (T)Ref.Create<T>(m, currentTypeRefs[typeName], null);
            }
            else
            {
                throw new ZevviException($"Error in memory initializer: Uninitialized required type: {typeName}.");
            }
        }

        public void InitManagerFields()
        {
            if (futureTypeRefs.Count > 0)
            {
                string uninit = string.Join(", ", futureTypeRefs.Keys);
                throw new ZevviException($"Error in memory initializer: Uninitialized type(s): {uninit}.");
            }

            InitOrThrowError(out m.TypeTypeRef, "Type");
            InitOrThrowError(out m.ClassTypeRef, "Class");
            InitOrThrowError(out m.IntTypeRef, "Int");
            InitOrThrowError(out m.BoolTypeRef, "Bool");
            InitOrThrowError(out m.StringTypeRef, "String");
            InitOrThrowError(out m.ArrayTypeRef, "Array");
            InitOrThrowError(out m.FuncTypeRef, "Func");
            InitOrThrowError(out m.ObjectTypeRef, "Object");
        }

        // TODOold add Function type, constructor field in Type, and InitTools.Construct(string typeName, params int[] arguments)

        public void Reset()
        {
            m.Reset();
            currentTypeRefs.Clear();
            futureTypeRefs.Clear();
        }
    }
}
