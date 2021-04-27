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

            Func<FromDirectoryOptions, string> fromDir = opts =>
            {
                var folderPath = new DirectoryInfo(opts.InputFolderPath);
                return folderPath.FullName;
            };

            Action<string> printIfNotEmpty = txt =>
            {
                if (txt.Length == 0) { return; }
                Console.WriteLine(txt);
            };

            var handler = new CommandHandler();

            var result = Parser.Default.ParseArguments<FromDirectoryOptions, FromPackageOptions>(args);
            var text = result
                .MapResult(
                    (FromDirectoryOptions opts) => handler.HandleFromDirectory(opts),
                    (FromPackageOptions opts) => handler.HandleFromPackage(opts),
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

            var line = new String('-', 40);
            Console.WriteLine("---show help for verbs ------");
        }


        //load all Verb types using Reflection
        static Type[] LoadVerbs()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
        }

        private static string MakeError()
        {
            return "Error";
        }
    }
}
