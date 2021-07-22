using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator
{
    class Program
    {
        internal static IEnumerable<FileInfo> DynamoDirectoryAssemblyPaths;
        static void Main(string[] args)
        {
            Program.DynamoDirectoryAssemblyPaths = new DirectoryInfo(
                Path.GetFullPath(
                    Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..\..\..\bin\AnyCPU\Debug")))
                .EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            ShowWelcomeMessages();

            var result = Parser.Default.ParseArguments<FromDirectoryOptions, FromPackageOptions>(args);
            var text = result
                .MapResult(
                    (FromDirectoryOptions opts) => CommandHandler.HandleFromDirectory(opts),
                    (FromPackageOptions opts) => CommandHandler.HandleFromPackage(opts),
                    err => "1");
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var requestedAssembly = new AssemblyName(args.Name);
            var requestedAssemblyLocation = Program.DynamoDirectoryAssemblyPaths
                .Where(x => Path.GetFileNameWithoutExtension(x.FullName) == requestedAssembly.Name)
                .FirstOrDefault();

            Assembly assembly = null;
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            try
            {
                assembly = Assembly.LoadFrom(requestedAssemblyLocation.FullName);
            }
            catch (Exception ex)
            {
                CommandHandler.LogExceptionToConsole(ex);
            }
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            return assembly;
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
