using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler
{
    public readonly struct ExtensionInfo
    {
        public readonly int extensionNumber;
        public readonly Action<DefaultExtension> ExtensionLoader;

        private static int nextNumber = 0;

        public ExtensionInfo(Action<DefaultExtension> extensionLoader)
        {
            this.extensionNumber = nextNumber++;
            ExtensionLoader = extensionLoader;
        }
    }
}
