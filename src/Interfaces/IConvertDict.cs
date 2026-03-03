using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler
{
    public interface IConvertDict
    {
        public IConvertDict Prefab { get; }

        public IConvertDict Instantiate();

        public ZI.CodeFunc Get(IType oldType, IType newType);

        public IEnumerable<ConvertFunc> GetAll();

        public void Add(ConvertFunc convertFunc);

        public void Add(IConvertDict convertDict);

        public void Add(IType type, ZI.CodeFunc codeFunc);

        public void Add(Func<IType> typeFunc, ZI.CodeFunc codeFunc);

        public IConvertDict Clone();
    }
}
