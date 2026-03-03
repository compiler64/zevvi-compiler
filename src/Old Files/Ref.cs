using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompilerOld
{
    public class Ref
    {
        public readonly Manager manager;
        public readonly int location;

        public Ref(Manager manager, int location)
        {
            this.manager = manager;
            this.location = location;
        }

        public static Ref Create<T>(Manager manager, int location) where T : Ref
        {
            if (typeof(T) == typeof(ClassRef)) return new ClassRef(manager, location);
            else if (typeof(T) == typeof(TypeRef)) return new TypeRef(manager, location);
            else if (typeof(T) == typeof(ObjectRef)) return new ObjectRef(manager, location);
            else if (typeof(T) == typeof(Ref)) return new Ref(manager, location);
            else throw new Exception($"Error in Ref.Create<T>: Unknown T: {typeof(T)}");
        }

        public static Ref Create<T>(Manager manager, int location, TypeRef checkType) where T : Ref
        {
            if (typeof(T) == typeof(ClassRef)) return new ClassRef(manager, location, checkType);
            else if (typeof(T) == typeof(TypeRef)) return new TypeRef(manager, location, checkType);
            else if (typeof(T) == typeof(ObjectRef)) return new ObjectRef(manager, location, checkType);
            else if (typeof(T) == typeof(Ref)) return new Ref(manager, location);
            else throw new Exception($"Error in Ref.Create<T>: Unknown T: {typeof(T)}");
        }

        public int LoadRelative(int relativeLocation)
        {
            return manager.Load(location + relativeLocation);
        }

        public Ref LoadRelative<T>(int relativeLocation, TypeRef checkType = null) where T : Ref
        {
            return manager.Load<T>(location + relativeLocation, checkType);
        }

        public override string ToString()
        {
            return $"Ref(loc={location})";
        }

        public static implicit operator int(Ref @ref)
        {
            return @ref.location;
        }
    }
}
