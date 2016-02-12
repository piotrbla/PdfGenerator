using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ColorCode;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;

namespace PdfGenerator
{
    class Program
    {
        private const int CODETAGLEN = 6;

        private static string TransformAndGenerate(string fileText)
        {
            var codeBlocks = new Dictionary<int, int>();
            var m = Regex.Match(fileText, @"<code>");
            while (m.Success)
            {
                //Console.WriteLine("Found {0} at index {1} of Length {2}.", m.Value, m.Index, m.Length);
                var mIndex = m.Index;
                var endIndex = fileText.IndexOf("</code>", mIndex, StringComparison.Ordinal);
                if (endIndex > 0)
                {
                    m = m.NextMatch();
                    if (endIndex < m.Index || m.Index == 0)
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
                builderCode.Replace(">", "&gt;", begin, element.Value - begin + diffLength);
            }
            return builderCode.ToString();
        }

        const int PieSize = 16;
        private static void DrawPie(XGraphics gfx, int y)
        {
            XPen pen = new XPen(XColors.DarkBlue, 2.5);
            gfx.DrawPie(pen, XBrushes.Gold, 10, y, PieSize, PieSize, 215, 290);
        }


        private static void GeneratePdf(XDocument doc, string filename)
        {
            var document = GenerateDocInMemory(doc);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filename);
            var pathName = Path.GetDirectoryName( filename);
            var documentFilename = Path.Combine(pathName, "prod_" + fileNameWithoutExt + ".pdf");
            try
            {
                document.Save(documentFilename);
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't create pdf, close pdf software (ie. Adobe Acrobat Reader) and press key.");
                Console.ReadKey();
                document.Save(documentFilename);//if i forgot to close acrobat
            }
            
            Process.Start(documentFilename);
        }

        private static PdfDocument GenerateDocInMemory(XDocument doc)
        {
            if (doc.Root == null) throw new Exception("Error after parsing xml");

            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 14, XFontStyle.Bold);
            XTextFormatter tf = new XTextFormatter(gfx);
            var margin = 30;
            var y = margin;
            const int lineSize = 60;
            foreach (var el in doc.Root.Elements())
            {
                if (el.HasElements)
                {
                    foreach (var subElement in el.Elements())
                    {
                        var nodeText = subElement.Value;
                        var linesCount = CountLines(nodeText);
                        double blockHeight = lineSize*linesCount;
                        XRect rect = new XRect(margin, y, page.Width - margin, blockHeight);
                        DrawPie(gfx, y);
                        gfx.DrawRectangle(XBrushes.WhiteSmoke, rect);
                        tf.DrawString(ReplaceXmlTags(nodeText), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                        y += (int) blockHeight;
                    }
                }
                else
                {
                    var nodeText = el.Value;
                    var linesCount = CountLines(nodeText);
                    double blockHeight = lineSize*linesCount;
                    XRect rect = new XRect(margin, y, page.Width - margin, blockHeight);
                    gfx.DrawRectangle(XBrushes.WhiteSmoke, rect);
                    tf.DrawString(ReplaceXmlTags(nodeText), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    y += (int) blockHeight;
                }
            }
            return document;
        }

        private static int CountLines(string nodeText)
        {
            var count = 0;
            var actualCount = 0;
            for (int i = 0; i < nodeText.Length; i++)
            {
                if (nodeText[i] == '\n' || actualCount > 40)
                {
                    actualCount = 0;
                    count++;
                }
            }
            if (count < 1) count = 1;
            return count;
        }

        private static string ReplaceXmlTags(string toString)
        {
            var result = toString.Replace("&lt;", "<");
            return result.Replace("&gt;", ">");
        }

        private static void XmlToPdf(string filename)
        {
            var fileText = File.ReadAllText(filename);
            var transformedText = TransformAndGenerate(fileText);
            var doc = XDocument.Parse(transformedText);
            GeneratePdf(doc, filename);

        }


        static void Main(string[] args)
        {
            XmlToPdf(@"C:\Users\Piotr\Dysk Google\C++ CPA Laboratoria\sławek\v2\02_1.4.40.1.txt");
            //var sourceCode = File.ReadAllText(@"../../Program.cs");
            //var colorizedSourceCode = new CodeColorizer().Colorize(sourceCode, Languages.CSharp);
            
            //Console.WriteLine(colorizedSourceCode);
        }
    }
}
