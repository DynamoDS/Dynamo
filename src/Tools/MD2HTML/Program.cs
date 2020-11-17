using System;
using System.IO;

namespace MD2HTML
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("");

            while (true)
            {
                var line = Console.ReadLine();
                if (line == "<<<<<Sanitize>>>>>")
                {
                    Sanitize();
                }
                else if (line == "<<<<<Convert>>>>>")
                {
                    Convert();
                }
                Console.WriteLine("<<<<<Eod>>>>>");
            }
        }

        static void Sanitize()
        {
            StringWriter data = new StringWriter();
            GetData(ref data);

            string output = data.ToString();
            MarkdownHandler.RemoveScriptTagsFromString(ref output);

            Console.WriteLine(output);
        }

        static void Convert()
        {
            var mdPath = Console.ReadLine();

            StringWriter data = new StringWriter();
            GetData(ref data);

            var instance = MarkdownHandler.Instance;
            StringWriter output = new StringWriter();
            instance.ParseToHtml(ref output, data.ToString(), mdPath);

            Console.WriteLine(output.ToString());
        }

        static void GetData(ref StringWriter data)
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "<<<<<Eod>>>>>")
                {
                    break;
                }
                data.WriteLine(line);
            }
        }
    }
}
