using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompilerOld
{
    public class ObjectRef : Ref
    {
        public TypeRef Type => (TypeRef)LoadRelative<TypeRef>(FieldLocs.Type);

        public ObjectRef(Manager manager, int location) : base(manager, location)
        {
        }

        public ObjectRef(Manager manager, int location, TypeRef checkType) : this(manager, location)
        {
            if (!(checkType is null || Type is ClassRef classRef && classRef.Implements(checkType) || Type.Equals(checkType)))
            {
                throw new ZevviException($"Invalid ObjectRef: Expected {checkType}, found {Type}.");
            }
        }

        public int GetField(string name)
        {
            throw new NotImplementedException();  // TODOold ObjectRef.GetField
        }

        public Ref GetField<T>(string name) where T : Ref
        {

            throw new NotImplementedException();  // TODOold ObjectRef.GetField<T>
        }

        public override string ToString()
        {
            return $"ObjectRef(type={Type}, loc={location})";
        }
    }
}
