using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace ZevviCompiler
{
    public class ZevviExtensionLoadError : ZevviException
    {
        public readonly Type extension;
        public readonly ImmutableArray<Type> missingExtensions;
        public readonly bool isRequirementsError;

        private ZevviExtensionLoadError(Type extension, IEnumerable<Type> missingExtensions)
            : base($"Zevvi extension requirements for {extension.Name} not met; " +
                  $"missing {string.Join(", ", missingExtensions.Select(ext => ext.Name))}")
        {
            this.extension = extension;
            this.missingExtensions = missingExtensions.ToImmutableArray();

            isRequirementsError = true;
        }

        private ZevviExtensionLoadError(Type extension)
            : base($"Zevvi extension {extension.Name} loaded twice.")
        {
            this.extension = extension;

            isRequirementsError = false;
        }

        public static ZevviExtensionLoadError RequirementsError(Type extension, IEnumerable<Type> missingExtensions)
        {
            return new ZevviExtensionLoadError(extension, missingExtensions);
        }

        public static ZevviExtensionLoadError LoadedTwiceError(Type extension)
        {
            return new ZevviExtensionLoadError(extension);
        }
    }
}
