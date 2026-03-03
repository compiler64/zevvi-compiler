using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler
{
    public interface IConverter
    {
        public IConvertDict AutoConvert { get; }

        public IConvertDict ImplicitConvert { get; }

        public IConvertDict ExplicitConvert { get; }

        public void Add(IConverter converter);

        public IConverter Clone();

        public IConverter Instantiate();
    }
}
