using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler
{
    public interface IIdentifierType
    {
        public string Identifier { get; }

        /*public IIdentifierType(DefaultExtension z, string name, string identifier, TransitionFunc transition,
            ConvertDict autoConverter, ConvertDict implicitConverter, ConvertDict explicitConverter)
            : base(z, name, transition, autoConverter, implicitConverter, explicitConverter)
        {
            this.identifier = identifier;
        }

        public IIdentifierType(DefaultExtension z, string name, string identifier, TransitionFunc transition)
            : base(z, name, transition)
        {
            this.identifier = identifier;
        }*/
    }
}
