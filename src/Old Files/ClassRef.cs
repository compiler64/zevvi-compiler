using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompilerOld
{
    public class ClassRef : TypeRef
    {
        // public int Size => LoadRelative(TypeFields.Size);

        public ObjectRef FieldNames => (ObjectRef)LoadRelative<ObjectRef>(FieldLocs.FieldNames, manager.ArrayTypeRef);

        public ObjectRef FieldTypes => (ObjectRef)LoadRelative<ObjectRef>(FieldLocs.FieldTypes, manager.ArrayTypeRef);

        public ObjectRef BaseClasses => (ObjectRef)LoadRelative<ObjectRef>(FieldLocs.BaseClasses, manager.ArrayTypeRef);

        public ClassRef(Manager manager, int location, TypeRef checkType) : base(manager, location, checkType)
        {
        }

        public ClassRef(Manager manager, int location) : base(manager, location, manager.ClassTypeRef)
        {
        }

        public bool Implements(TypeRef interfaceRef)
        {
            /*
            if (Equals(interfaceRef))
            {
                return true;
            }
            else
            {
                foreach (int loc in manager.ArrayLoop(Interfaces))
                {
                    if (new TypeRef(manager, loc).Implements(interfaceRef))
                    {
                        return true;
                    }
                }
                return false;
            }
            */
            throw new NotImplementedException();  // TODOold ClassRef.Implements
        }
    }
}
