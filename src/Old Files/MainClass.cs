using System;
using static ZevviCompiler.MachineInstruction;

namespace ZevviCompilerOld
{
    class MainClass
    {
        static void Main()
        {
            Console.WriteLine("The compiler is not ready yet.");
            Test();
        }

        static void Test()
        {
            Manager m = new Manager();
            DefaultInitializers.defaultInitializer(m);

            InitTools t = new InitTools(m);
            int reg = m.NextLoc(20);  // first register
            int nullRef = 0;

            t.Class
            (
                name: "LinkedListNode",
                fieldNamesRef: t.StringArray("previous", "next", "value"),
                fieldTypesRef: t.TypeArray("LinkedListNode", "LinkedListNode", "Object"),
                baseClassesRef: t.TypeArray("Object"),
                staticFieldNamesRef: t.StringArray("$Constructor"),
                staticFieldTypesRef: t.TypeArray("Func"),
                staticFieldValuesRef: t.Array(
                    t.Function // $Constructor(Object value)
                    (
                        (int)PopI, reg,
                        (int)Next,
                        (int)StoNxtI, "LinkedListNode",
                        (int)StoNxtI, nullRef,
                        (int)StoNxtI, nullRef,
                        (int)PushI, reg,
                        (int)StoNxt,
                        (int)Ret
                    ))
            );

            t.Class
            (
                name: "LinkedList",
                fieldNamesRef: t.StringArray("first", "last"),
                fieldTypesRef: t.TypeArray("LinkedListNode", "LinkedListNode"),
                baseClassesRef: t.TypeArray("Object"),
                staticFieldNamesRef: t.StringArray("$Constructor", "Get", "Set", "Add", "Insert"),
                staticFieldTypesRef: t.TypeArray("Func", "Func", "Func", "Func", "Func"),
                staticFieldValuesRef: t.Array(
                    t.Function // $Constructor(LinkedListNode first, LinkedListNode last)
                    (
                        (int)PopI, reg + 1,
                        (int)PopI, reg,
                        (int)Next,
                        (int)StoNxtI, "LinkedList", // new LinkedList
                        (int)PushI, reg, // this.first = first
                        (int)Push,
                        (int)StoNxt,
                        (int)PushI, reg + 1, // this.last = last
                        (int)Push,
                        (int)StoNxt,
                        (int)Ret
                    )
                    // TODOold add more funcs
                )
            );

            m.stack.Push(t.String("The only item in this linked list!"));
            int linkedListNode = t.GetTypeLoc(-1, "LinkedListNode");
            int linkedListNodeStaticFields = m.Load(linkedListNode + FieldLocs.StaticFieldValues);
            int linkedListNodeConstructor = m.Load(linkedListNodeStaticFields + 1);
            int main = m.Function((int)CallI, linkedListNodeConstructor, (int)Halt);
            m.Execute(main);
        }
    }
}
