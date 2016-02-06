using System;
using System.IO;
using ColorCode;

namespace PdfGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var sourceCode = File.ReadAllText(@"../../Program.cs");
            var colorizedSourceCode = new CodeColorizer().Colorize(sourceCode, Languages.CSharp);
            
            Console.WriteLine(colorizedSourceCode);
        }
    }
}
