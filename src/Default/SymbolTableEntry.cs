namespace ZevviCompiler
{
    public class SymbolTableEntry
    {
        public string identifier;
        public IType type;
        public ZI.IOperand storage;

        public SymbolTableEntry(string identifier, IType type, ZI.IOperand location = null)
        {
            this.identifier = identifier;
            this.type = type;
            this.storage = location;
        }

        public void Deconstruct(out string identifier, out IType type, out ZI.IOperand location)
        {
            identifier = this.identifier;
            type = this.type;
            location = this.storage;
        }

        public void Deconstruct(out IType type, out ZI.IOperand location)
        {
            type = this.type;
            location = this.storage;
        }

        public override string ToString()
        {
            return storage is null ? $"('{identifier}', type {type})" : $"('{identifier}', type {type}, ZI location '{storage}')";
        }
    }
}
