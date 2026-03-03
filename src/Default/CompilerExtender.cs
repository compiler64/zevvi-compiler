using MyLibraries.LexicalAnalyzer;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ZevviCompiler
{
    public partial class DefaultExtension
    {
        private readonly List<ICompilerExtension> extensions;

        public DefaultExtension()
        {
            extensions = new List<ICompilerExtension> { this };
        }

        public static readonly Dictionary<string, ExtensionInfo> extensionDict = new()
        {
            ["OperatorSyntaxes"] = new(z => z.Extend<OperatorSyntaxes.OperatorSyntaxesExtension>()),
            ["Arithmetic"] = new(z => z.Extend<Arithmetic.ArithmeticExtension>()),
            ["Punctuation"] = new(z => z.Extend<Punctuation.PunctuationExtension>()),
            ["Variables"] = new(z => z.Extend<Variables.VariablesExtension>()),
            ["MoreAssignment"] = new(z => z.Extend<MoreAssignment.MoreAssignmentExtension>()),
            ["IfStatements"] = new(z => z.Extend<IfStatements.IfStatementsExtension>()),
            ["WhileLoops"] = new(z => z.Extend<WhileLoops.WhileLoopsExtension>()),
            ["ForLoops"] = new(z => z.Extend<ForLoops.ForLoopsExtension>()),
            ["IO"] = new(z => z.Extend<IO.IOExtension>()),
            ["Arrays"] = new(z => z.Extend<Arrays.ArraysExtension>()),
            ["FunctionCalls"] = new(z => z.Extend<FunctionCalls.FunctionCallsExtension>()),
            ["FunctionDefinitions"] = new(z => z.Extend<FunctionDefinitions.FunctionDefinitionsExtension>()),
            ["Modules"] = new(z => z.Extend<Modules.ModulesExtension>()),
            ["CompileTime"] = new(z => z.Extend<CompileTime.CompileTimeExtension>()),
            ["SyntaxModification"] = new(z => z.Extend<SyntaxModification.SyntaxModificationExtension>()),
            ["StructureTypes"] = new(z => z.Extend<StructureTypes.StructureTypesExtension>()),
        };

        public bool HasExtension<ExtensionType>() where ExtensionType : ICompilerExtension
        {
            return extensions.Any(ext => ext is ExtensionType);
        }

        public bool TryGetExtension<ExtensionType>(out ExtensionType extension) where ExtensionType : ICompilerExtension
        {
            foreach (ICompilerExtension ext in extensions)
            {
                if (ext is ExtensionType theExt)
                {
                    extension = theExt;
                    return true;
                }
            }

            extension = default;
            return false;
        }

        public ExtensionType GetExtension<ExtensionType>() where ExtensionType : ICompilerExtension
        {
            return TryGetExtension(out ExtensionType extension) ? extension
                : throw new ZevviInternalCompilerError($"Cannot get non-existent extension {typeof(ExtensionType)}.");
        }

        private ExtensionType CheckRequirements<ExtensionType>() where ExtensionType : ICompilerExtension, new()
        {
            if (HasExtension<ExtensionType>())
            {
                throw ZevviExtensionLoadError.LoadedTwiceError(typeof(ExtensionType));
            }

            ExtensionType extension = new();

            IEnumerable<Type> missing = extension.RequiredExtensions.Except(extensions.Select(ext => ext.GetType()));

            if (missing.Any())
            {
                throw ZevviExtensionLoadError.RequirementsError(typeof(ExtensionType), missing);
            }

            return extension;
        }

        public ExtensionType Extend<ExtensionType>() where ExtensionType : ICompilerExtension, new()
        {
            ExtensionType extension = CheckRequirements<ExtensionType>();
            extension.Z = this;
            extensions.Add(extension);
            return extension;
        }

        public void Extend(IList<string> extensionNames)
        {
            foreach (string ext in extensionNames)
            {
                if (!extensionDict.ContainsKey(ext))
                {
                    throw new ArgumentException($"Unknown Zevvi compiler extension: '{ext}'.");
                }

                extensionDict[ext].ExtensionLoader(this);
            }
        }

        public void ExtendAll()
        {
            foreach (ExtensionInfo info in extensionDict.Values)
            {
                info.ExtensionLoader(this);
            }
        }
    }
}
