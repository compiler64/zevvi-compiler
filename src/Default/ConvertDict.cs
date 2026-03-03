using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZevviCompiler
{
    public class ConvertDict : IConvertDict, IEnumerable
    {
        private readonly List<ConvertFunc> list;

        public IConvertDict Prefab { get; }

        /*public ConvertFunc this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }*/

        public ConvertDict(IConvertDict prefab = null)
        {
            Prefab = prefab;
            list = new();
        }

        private ConvertDict(IConvertDict prefab, IEnumerable<ConvertFunc> list)
        {
            Prefab = prefab;
            this.list = list.ToList();
        }

        public IConvertDict Instantiate()
        {
            return new ConvertDict(this);
        }

        public ZI.CodeFunc Get(IType oldType, IType newType)
        {
            if (oldType is null)
            {
                throw new ArgumentNullException(nameof(oldType));
            }

            if (newType is null)
            {
                throw new ArgumentNullException(nameof(newType));
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                ConvertFunc func = list[i];
                ZI.CodeFunc codeFunc = func(oldType, newType);

                if (codeFunc is not null)
                {
                    return codeFunc;
                }
            }

            return Prefab?.Get(oldType, newType);
        }

        public IEnumerable<ConvertFunc> GetAll()
        {
            return list;
        }

        public void Add(ConvertFunc func)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            list.Add(func);
        }

        public void Add(IType type, ZI.CodeFunc codeFunc)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (codeFunc is null)
            {
                throw new ArgumentNullException(nameof(codeFunc));
            }

            list.Add((oldType, newType) => IType.Eq(type, newType) ? codeFunc : null);
        }

        public void Add(Func<IType> typeFunc, ZI.CodeFunc codeFunc)
        {
            if (typeFunc is null)
            {
                throw new ArgumentNullException(nameof(typeFunc));
            }

            if (codeFunc is null)
            {
                throw new ArgumentNullException(nameof(codeFunc));
            }

            list.Add((oldType, newType) => IType.Eq(typeFunc(), newType) ? codeFunc : null);
        }

        /*public void Add(Func<IType, (IType, ZI.CodeFunc)> convert)
        {
            list.Add((oldType, newType) =>
            {
                (IType type, ZI.CodeFunc codeFunc) = convert(oldType);
                return type.Equals(newType) ? codeFunc : null;
            });
        }*/

        /*public void Add(IDictionary<IType, ZI.CodeFunc> converts)
        {
            list.Add((oldType, newType) => converts.ContainsKey(newType) ? converts[newType] : null);
        }*/

        /*public void Add(Func<IType, IDictionary<IType, ZI.CodeFunc>> converts)
        {
            list.Add((oldType, newType) =>
            {
                IDictionary<IType, ZI.CodeFunc> convertsDict = converts(oldType);
                return convertsDict.ContainsKey(newType) ? convertsDict[newType] : null;
            });
        }*/

        public void Add(IConvertDict convertDict)
        {
            if (convertDict is null)
            {
                throw new ArgumentNullException(nameof(convertDict));
            }

            list.AddRange(convertDict.GetAll());
        }

        /*IEnumerator<ConvertFunc> IEnumerable<ConvertFunc>.GetEnumerator()
        {
            return list.GetEnumerator();
        }*/

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public IConvertDict Clone()
        {
            return new ConvertDict(Prefab, list);
        }
    }
}
