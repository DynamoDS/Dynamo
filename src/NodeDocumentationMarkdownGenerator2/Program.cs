using System;
using CommandLine;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            ShowWelcomeMessages();

            var result = Parser.Default.ParseArguments<FromDirectoryOptions, FromPackageOptions>(args);
            var text = result
                .MapResult(
                    (FromDirectoryOptions opts) => CommandHandler.HandleFromDirectory(opts),
                    (FromPackageOptions opts) => CommandHandler.HandleFromPackage(opts),
                    err => "1");
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
    }
}
