using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompilerOld
{
    public class Storage
    {
        public readonly List<int> storage;

        public int Size => storage.Count;

        public int this[int location]
        {
            get
            {
                return storage[location];
            }
            set
            {
                if (location < 0)
                {
                    throw new IndexOutOfRangeException("Index of Storage must be nonnegative.");
                }
                else if (location < Size)
                {
                    storage[location] = value;
                }
                else
                {
                    Expand(location - Size + 1);
                    storage[location] = value;
                }
            }
        }

        public Storage()
        {
            storage = new List<int>();
        }

        public void Expand(int numToAdd)
        {
            storage.AddRange(new int[numToAdd]);
        }

        public void Add(int value)
        {
            storage.Add(value);
        }

        public void Clear()
        {
            storage.Clear();
        }
    }
}
