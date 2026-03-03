using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler
{
    public partial class DefaultExtension
    {
        public ConvertDict NoneConvert { get; private set; }

        public ConvertDict SelfConvert { get; private set; }

        public IConvertDict WildcardConvert { get; private set; }

        public IConvertDict WildcardOrSelfConvert { get; private set; }

        public IConvertDict VoidConvert { get; private set; }

        public IConvertDict VoidOrSelfConvert { get; private set; }

        public IConverter VoidConverter { get; private set; }

        public IConverter NormalConverter { get; private set; }

        public void InitConverts()
        {
            NoneConvert = new ConvertDict();
            SelfConvert = new ConvertDict()
            {
                (oldType, newType) => IType.Eq(newType, oldType) ? storage => ZI.Code.Operand(storage) : null,
            };
            WildcardConvert = new ConvertDict()
            {
                (oldType, newType) => newType is WildcardType wildcardType ? wildcardType.TryCapture(oldType) : null
            };
            WildcardOrSelfConvert = new ConvertDict()
            {
                WildcardConvert,
                SelfConvert,
            };
            VoidConvert = new ConvertDict()
            {
                { () => Void, storage => ZI.Code.None }
            };
            VoidOrSelfConvert = new ConvertDict()
            {
                VoidConvert,
                SelfConvert
            };
            VoidConverter = new ZConverter()
            {
                AutoConvert = SelfConvert,
                ImplicitConvert = VoidConvert,
                ExplicitConvert = NoneConvert,
            };
            NormalConverter = new ZConverter()
            {
                AutoConvert = WildcardOrSelfConvert,
                ImplicitConvert = VoidConvert,
                ExplicitConvert = NoneConvert,
            };
        }
    }
}
