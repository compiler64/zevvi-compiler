using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompilerOld
{
    public delegate void Initializer(Manager m);

    static class DefaultInitializers
    {
        public static Initializer defaultInitializer = (Manager m) =>
        {
            // m.PushStorage(); // object storage
            InitTools t = new InitTools(m);

            int nullRef = m.NextLoc(1);
            t.StoreTypeRef(nullRef, "Void");

            m.TypeCounterRef = new Ref(m, m.NextLoc(1, 0));

            t.Class
            (
                name: "Void",
                fieldNamesRef: t.StringArray("type"),
                fieldTypesRef: t.TypeArray("Type"),
                staticFieldNamesRef: t.StringArray("null"),
                staticFieldTypesRef: t.TypeArray("Void"),
                baseClassesRef: t.TypeArray("Object"),
                staticFieldValuesRef: t.Array(nullRef)
            );

            t.Type
            (
                name: "Bool",
                staticFieldNamesRef: t.StringArray(),
                staticFieldTypesRef: t.TypeArray(),
                staticFieldValuesRef: t.Array()
            );

            t.Type
            (
                name: "Int",
                staticFieldNamesRef: t.StringArray("MinValue", "MaxValue"),
                staticFieldTypesRef: t.TypeArray("Int", "Int"),
                staticFieldValuesRef: t.Array(int.MinValue, int.MaxValue)
            );
            
            t.Type
            (
                name: "String",
                staticFieldNamesRef: t.StringArray(),
                staticFieldTypesRef: t.TypeArray(),
                staticFieldValuesRef: t.Array()
            );
            
            t.Type
            (
                name: "Array",
                staticFieldNamesRef: t.StringArray(),
                staticFieldTypesRef: t.TypeArray(),
                staticFieldValuesRef: t.Array()
            );
            
            t.Type
            (
                name: "Func",
                staticFieldNamesRef: t.StringArray(),
                staticFieldTypesRef: t.TypeArray(),
                staticFieldValuesRef: t.Array()
            );

            t.Class
            (
                name: "Type",
                fieldNamesRef: t.StringArray("type", "name", "typeNumber", "staticFieldNames", "staticFieldTypes", "staticFieldValues"),
                fieldTypesRef: t.TypeArray("Class", "String", "Int", "Array", "Array", "Array"),
                baseClassesRef: t.TypeArray("Object"),
                staticFieldNamesRef: t.StringArray(),
                staticFieldTypesRef: t.TypeArray(),
                staticFieldValuesRef: t.Array()
            );

            t.Class
            (
                name: "Class",
                fieldNamesRef: t.StringArray("type", "name", "typeNumber", "staticFieldNames", "staticFieldTypes", "staticFieldValues", "fieldNames", "fieldTypes", "baseClasses"),
                fieldTypesRef: t.TypeArray("Class", "String", "Int", "Array", "Array", "Array", "Array", "Array"),
                baseClassesRef: t.TypeArray("Object", "Type"),
                staticFieldNamesRef: t.StringArray(),
                staticFieldTypesRef: t.TypeArray(),
                staticFieldValuesRef: t.Array()
            );

            t.Class
            (
                name: "Object",
                fieldNamesRef: t.StringArray("type"),
                fieldTypesRef: t.TypeArray("Type"),
                baseClassesRef: t.TypeArray(),
                staticFieldNamesRef: t.StringArray(),
                staticFieldTypesRef: t.TypeArray(),
                staticFieldValuesRef: t.Array()
            );

            t.InitManagerFields();
        };

        public static Initializer compilerInitializer = (Manager m) =>
        {
            InitTools t = new InitTools(m);

            t.Type
            (
                name: "Token",
                staticFieldNamesRef: t.StringArray(),
                staticFieldTypesRef: t.TypeArray(),
                staticFieldValuesRef: t.Array()
            );
        };
    }
}
