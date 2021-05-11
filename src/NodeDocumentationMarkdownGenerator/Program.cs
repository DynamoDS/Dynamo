using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator
{
    class Program
    {
        
        static void Main(string[] args)
        {
            ShowWelcomeMessages();

            Action<string> printIfNotEmpty = txt =>
            {
                if (txt.Length == 0) { return; }
                Console.WriteLine(txt);
            };

            var result = Parser.Default.ParseArguments<FromDirectoryOptions, FromPackageOptions>(args);
            var text = result
                .MapResult(
                    (FromDirectoryOptions opts) => CommandHandler.HandleFromDirectory(opts),
                    (FromPackageOptions opts) => CommandHandler.HandleFromPackage(opts),
                    _ => MakeError());

            printIfNotEmpty(text);
        }

        private static void ShowWelcomeMessages()
        {
            var header = @"
 _______                                                        
/       \                                                       
$$$$$$$  | __    __  _______    ______   _____  ____    ______  
$$ |  $$ |/  |  /  |/       \  /      \ /     \/    \  /      \ 
$$ |  $$ |$$ |  $$ |$$$$$$$  | $$$$$$  |$$$$$$ $$$$  |/$$$$$$  |
$$ |  $$ |$$ |  $$ |$$ |  $$ | /    $$ |$$ | $$ | $$ |$$ |  $$ |
$$ |__$$ |$$ \__$$ |$$ |  $$ |/$$$$$$$ |$$ | $$ | $$ |$$ \__$$ |
$$    $$/ $$    $$ |$$ |  $$ |$$    $$ |$$ | $$ | $$ |$$    $$/ 
$$$$$$$/   $$$$$$$ |$$/   $$/  $$$$$$$/ $$/  $$/  $$/  $$$$$$/  
          /  \__$$ |                                            
          $$    $$/                                             
           $$$$$$/                                                          
            ";
            Console.WriteLine(header);
        }

        private static string MakeError()
        {
            return "Error";
        }
    }
}
