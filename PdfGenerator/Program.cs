using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ColorCode;

namespace PdfGenerator
{
    class Program
    {
        private const int CODETAGLEN = 6;

        private static void WritePdf(string filename)
        {
            var fileText = File.ReadAllText(filename);

            var codeBlocks = new Dictionary<int, int>();
            var m = Regex.Match(fileText, @"<code>");
            while (m.Success)
            {
                //Console.WriteLine("Found {0} at index {1} of Length {2}.", m.Value, m.Index, m.Length);
                var mIndex = m.Index;
                var endIndex = fileText.IndexOf("</code>", mIndex, StringComparison.Ordinal);
                if ( endIndex > 0)
                {
                    m = m.NextMatch();
                    if (endIndex < m.Index || m.Index==0)
                    {
                        codeBlocks[mIndex] = endIndex;
                    }
                    else
                    {
                        if (m.Success)
                            throw new Exception("No </code> after <code> and before next <code>");
                    }
                }
                else
                    throw new Exception("No </code> after <code>");
            }
            var builderCode = new StringBuilder(fileText);
            foreach (var element in codeBlocks.OrderByDescending(x => x.Key))
            {
                var begin = element.Key + CODETAGLEN;
                var ltIndex = fileText.IndexOf("<", begin, StringComparison.Ordinal);
                var gtIndex = fileText.IndexOf(">", begin, StringComparison.Ordinal);
                if (gtIndex >= element.Value && ltIndex >= element.Value) continue;

                var oldLength = builderCode.Length;
                builderCode.Replace("<", "&lt;", begin, element.Value - begin);
                var diffLength = builderCode.Length - oldLength;
                builderCode.Replace(">", "&lt;", begin, element.Value - begin + diffLength);
            }
            fileText = builderCode.ToString();
            var doc = XDocument.Parse(fileText);
            foreach (XElement el in doc.Root.Elements())
            {
                Console.WriteLine("{0}", el.NextNode);
                //Console.WriteLine("  Attributes:");
                //foreach (XAttribute attr in el.Attributes())
                //    Console.WriteLine("    {0}", attr);
                //Console.WriteLine("  Elements:");

                foreach (XElement element in el.Elements())
                    Console.WriteLine("    {0}: {1}", element.Name, element.Value);
            }
        }

        static void Main(string[] args)
        {
            WritePdf(@"C:\Users\Piotr\Dysk Google\C++ CPA Laboratoria\sławek\01_1.2.6.1.txt");
            //var sourceCode = File.ReadAllText(@"../../Program.cs");
            //var colorizedSourceCode = new CodeColorizer().Colorize(sourceCode, Languages.CSharp);
            
            //Console.WriteLine(colorizedSourceCode);
        }
    }
}
