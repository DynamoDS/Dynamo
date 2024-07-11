using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace Md2Html
{
    internal class CMDLineOptions
    {
        [Option('h', "help", Required = false, HelpText = "Show help and exit")]
        public bool ShowHelp { get; set; }
    }

    static class Program
    {
        static void Main(string[] args)
        {
            if (CheckForHelp(args))
            {
                // Display help and exit
                return;
            }

            try
            {
                Console.WriteLine("");
                while (true)
                {
                    var line = Console.ReadLine();
                    if (line.Contains(@"<<<<<Sanitize>>>>>"))
                    {
                        Console.WriteLine(@"<<<<<Sod>>>>>");
                        Sanitize();
                    }
                    else if (line.Contains(@"<<<<<Convert>>>>>"))
                    {
                        Console.WriteLine(@"<<<<<Sod>>>>>");
                        Convert();
                    }
                    Console.WriteLine(@"<<<<<Eod>>>>>");
                }
            }
            catch (Exception e) when (e is IOException || e is OutOfMemoryException || e is ArgumentOutOfRangeException )
            {
                // Exit process
            }
        }

        static void Sanitize()
        {
            var data = GetData();

            var output = Md2Html.Sanitize(data);

            Console.WriteLine(output);
        }

        static void Convert()
        {
            var mdPath = Console.ReadLine();

            var data = GetData();

            var instance = Md2Html.Instance;
            var output = instance.ParseToHtml(data, mdPath);

            Console.WriteLine(output);
        }

        static string GetData()
        {
            using (StringWriter data = new StringWriter())
            {
                while (true)
                {
                    var line = Console.ReadLine();
                    if (line == @"<<<<<Eod>>>>>")
                    {
                        break;
                    }
                    data.WriteLine(line);
                }

                return data.ToString();
            }
        }

        static bool CheckForHelp(string[] args)
        {
            bool help = false;
            var parser = new Parser(options => {
                options.IgnoreUnknownArguments = true; options.HelpWriter = Console.Out;
                options.CaseSensitive = false;
            });
            var results = parser.ParseArguments<CMDLineOptions>(args);

            help = results.MapResult((cmdArgs) => {
                return cmdArgs.ShowHelp;
            }, errs => false);

            if (help)
            {
                Console.WriteLine(HelpText.AutoBuild(results, null, null).ToString());
                DisplayHelp();
            }

            return help;
        }
        static void DisplayHelp()
        {
            Console.WriteLine(@"");
            Console.WriteLine(@"This tool converts Markdown to Html or Sanitize html");
            Console.WriteLine(@"and reads from stdin and outputs to stdout");
            Console.WriteLine(@"");
            Console.WriteLine(@"Converting Markdown to Html");
            Console.WriteLine(@"---------------------------");
            Console.WriteLine(@"Format markdown as follows:");
            Console.WriteLine(@"<<<<<Convert>>>>>");
            Console.WriteLine(@"Markdown formatted data here");
            Console.WriteLine(@"<<<<<Eod>>>>>");
            Console.WriteLine(@"");
            Console.WriteLine(@"Output will be:");
            Console.WriteLine(@"Html formatted data");
            Console.WriteLine(@"<<<<<Eod>>>>>");
            Console.WriteLine(@"");
            Console.WriteLine(@"Sanitize Html");
            Console.WriteLine(@"-------------");
            Console.WriteLine(@"Format html data as follows:");
            Console.WriteLine(@"<<<<<Sanitize>>>>>");
            Console.WriteLine(@"Html data to sanitize here");
            Console.WriteLine(@"<<<<<Eod>>>>>");
            Console.WriteLine(@"");
            Console.WriteLine(@"Output will be:");
            Console.WriteLine(@"Sanitized Html data or empty if no sanitization was needed");
            Console.WriteLine(@"<<<<<Eod>>>>>");
            Console.WriteLine(@"");
            Console.WriteLine(@"");
        }
    }
}
