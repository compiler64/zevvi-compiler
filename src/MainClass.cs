using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZevviCompiler.Arithmetic;
using ZevviCompiler.Constants;
using ZevviCompiler.IfStatements;
using ZevviCompiler.IO;
using ZevviCompiler.MoreAssignment;
using ZevviCompiler.OperatorSyntaxes;
using ZevviCompiler.Punctuation;
using ZevviCompiler.Variables;
using ZevviCompiler.WhileLoops;
using MyLibraries.CommandParser;
using System.Linq;

namespace ZevviCompiler
{
    public static class MainClass
    {
        public static readonly CommandLineApplication application = new("ZevviCompiler", 1,
@"
Usage of ZevviCompiler.exe:
    ZevviCompiler <sourcefile> [-x <extensions>] [-d]                   | Compiles a file.
    ZevviCompiler <sourcefile> -o <outputfile> [-x <extensions>] [-d]   | Compiles a file to the specified output file.
    ZevviCompiler -h                                                    | Displays this help message
    ZevviCompiler /?                                                    | Displays this help message
Use '-d' flag to print compile information.
Extensions list should be in double quotes.  If '-x' parameter is not specified, uses every built-in extension.",
            new Dictionary<string, int>
            {
                ["-h"] = 0,
                ["/?"] = 0,
                ["-o"] = 1,
                ["-d"] = 0,
                ["-x"] = 1,
                ["-test"] = 0,
                ["-debug"] = 0,
            });

        public static void Main(string[] args)
        {
            CommandLine commandLine = application.Parse(args);
            
            if (commandLine.ContainsExtra(0))
            {
                FilePath sourcePath = new(commandLine.GetExtraWord(0));
                FilePath outputPath = commandLine.Contains("-o")
                    ? new FilePath(commandLine.GetParameter("-o")[0])
                    : sourcePath.ChangeExtension("zi");
                string[] extensions = commandLine.Contains("-x")
                    ? commandLine.GetParameter("-x")[0].Split(" ")
                    : DefaultExtension.extensionDict.Keys.ToArray();

                if (commandLine.Contains("-debug"))
                {
                    CompileFile(sourcePath.Path, outputPath.Path, extensions, commandLine.Contains("-d"));
                }
                else
                {
                    try
                    {
                        CompileFile(sourcePath.Path, outputPath.Path, extensions, commandLine.Contains("-d"));
                    }
                    catch (ZevviException e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }
                    catch (FileNotFoundException e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine($"An unknown internal error has occured.  Message: '{e.Message}'");
                    }
                }
            }
            else
            {
                if (commandLine.Contains("-h") || commandLine.Contains("/?"))
                {
                    application.DisplayHelp();
                }
                else if (commandLine.Contains("-test"))
                {
                    Test();
                }
                else
                {
                    Console.Error.WriteLine("Error in command-line arguments for ZevviCompiler.exe.");
                    application.DisplayHelp();
                }
            }
        }

        public static string CompileFile(string sourcePath, bool display)
        {
            return CompileFile(sourcePath, (IList<string>)null, display);
        }

        public static string CompileFile(string sourcePath, IList<string> extensions, bool display)
        {
            string outputPath = new FilePath(sourcePath).ChangeExtension("zi").Path;
            return CompileFile(sourcePath, outputPath, extensions, display);
        }

        public static string CompileFile(string sourcePath, string outputPath, bool display)
        {
            return CompileFile(sourcePath, outputPath, null, display);
        }

        public static string CompileFile(string sourcePath, string outputPath, IList<string> extensions, bool display)
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"File '{sourcePath}' does not exist or is not a file.");
            }

            string sourceCode = File.ReadAllText(sourcePath);
            ParseTree tree = CompileText(sourceCode, extensions, display, out string displayText);
            ZI.Code.WriteToFile(outputPath, tree.exprCode);
            return displayText;
        }

        public static ParseTree CompileText(string sourceCode, IList<string> extensions, bool display, out string displayText)
        {
            DefaultExtension z = new();

            z.Extend(extensions ?? DefaultExtension.extensionDict.Keys.ToArray());

            displayText = "";
            ParseTree tree = display ? z.DisplayCompile(sourceCode) : z.Compile(sourceCode, out displayText);
            return tree;
        }

        public static void Test()
        {
            string source = File.ReadAllText(@"C:\Users\Max\Documents\ZevviLang\test.zv");
            DefaultExtension z = new();
            z.ExtendAll();
            ParseTree tree = z.DisplayCompile(source);

            ZI.Code code = tree.exprCode;

            Console.WriteLine("ZI.Code:\n" + code.ToMultiLineString());
            Console.WriteLine("Interpreting...");
            ZI.Interpreter.Interpret(code, z);
        }
    }
}
