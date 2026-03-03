using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompilerOld
{
#pragma warning disable CS0659
    public class TypeRef : ObjectRef
    {
        public int TypeNumber => LoadRelative(FieldLocs.TypeNumber);

        public Ref NameObject => LoadRelative<Ref>(FieldLocs.Name, manager.StringTypeRef);

        public string Name => manager.StringValue(NameObject);

        public TypeRef(Manager manager, int location, TypeRef checkType) : base(manager, location, checkType)
        {
        }

        public TypeRef(Manager manager, int location) : base(manager, location, manager.TypeTypeRef)
        {
        }

        public override string ToString()
        {
            return $"TypeRef({Name})";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeRef))
            {
                return false;
            }

            throw new NotImplementedException();  // TODOold add type equality checker matrix
        }
    }
}
