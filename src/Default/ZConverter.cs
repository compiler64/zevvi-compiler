using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler
{
    public class ZConverter : IConverter
    {
        public IConvertDict AutoConvert { get; init; }

        public IConvertDict ImplicitConvert { get; init; }

        public IConvertDict ExplicitConvert { get; init; }

        public void Add(IConverter converter)
        {
            AutoConvert.Add(converter.AutoConvert);
            ImplicitConvert.Add(converter.ImplicitConvert);
            ExplicitConvert.Add(converter.ExplicitConvert);
        }

        public IConverter Clone()
        {
            return new ZConverter
            {
                AutoConvert = AutoConvert.Clone(),
                ImplicitConvert = ImplicitConvert.Clone(),
                ExplicitConvert = ExplicitConvert.Clone(),
            };
        }

        public IConverter Instantiate()
        {
            return new ZConverter
            {
                AutoConvert = AutoConvert.Instantiate(),
                ImplicitConvert = ImplicitConvert.Instantiate(),
                ExplicitConvert = ExplicitConvert.Instantiate(),
            };
        }
    }
}
