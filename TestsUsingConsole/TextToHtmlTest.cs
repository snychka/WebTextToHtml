using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Text2Html;

namespace TestsUsingConsole
{
    public class TextToHtmlTest
    {
        private static readonly string INPUT_TXT = 
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + "tornadoes.txt";
        private static readonly string OUTPUT_TXT =
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + "tornadoes.html";

        static void Main(string[] args)
        {
            //TestFile();
            //TestString();
        }

        static void TestFile()
        {
            using (TextReader reader = TextToHtml.OpenReaderFromFile(INPUT_TXT))
            {
                ConvertAndWrite(reader);
            }

        }

        static void TestString()
        {
            using (TextReader reader = TextToHtml.OpenReaderFromString(File.ReadAllText(INPUT_TXT)))
            {
                ConvertAndWrite(reader);
            }
            
        }

        static void ConvertAndWrite(TextReader reader)
        {
            using (TextWriter writer = TextToHtml.OpenWriterToFile(OUTPUT_TXT))
            {
                try
                {
                    TextToHtml converter = new TextToHtml();
                    string convertedString = converter.ReadAndConvert(reader);
                    writer.Write(convertedString);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
            }
        }

    }
}
