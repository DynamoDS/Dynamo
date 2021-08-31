using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator
{
    class Program
    {
        internal static List<String> ReferenceAssemblyPaths = new List<string>();
        internal static bool VerboseMode { get; set; }
        private static IEnumerable<FileInfo> dynamoDirectoryAssemblyPaths;
        internal static IEnumerable<FileInfo> DynamoDirectoryAssemblyPaths
        {
            get
            {

                if (dynamoDirectoryAssemblyPaths is null)
                {
#if DEBUG
            var config = "Debug";
#else
   var config = "Release";
#endif
            var relativePathToDynamo = $@"..\..\..\..\..\bin\AnyCPU\{config}";
            Console.WriteLine($"looking for dynamo core assemblies in {relativePathToDynamo}");
                    dynamoDirectoryAssemblyPaths = new DirectoryInfo(
                Path.GetFullPath(
                    Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), relativePathToDynamo)))
                .EnumerateFiles("*.dll", SearchOption.AllDirectories);
                }

                return dynamoDirectoryAssemblyPaths;
            }
        }

        internal static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

          

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            ShowWelcomeMessages();

            var result = Parser.Default.ParseArguments<FromDirectoryOptions, FromPackageOptions>(args);
            var text = result
                .MapResult(
                    (FromDirectoryOptions opts) => CommandHandler.HandleFromDirectory(opts),
                    (FromPackageOptions opts) => CommandHandler.HandleFromPackage(opts),
                    err => "1");
            Console.WriteLine($"docs generation took {sw.Elapsed.TotalSeconds}");
# if DEBUG
            Console.ReadLine();
#endif
        }

        internal static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var requestedAssembly = new AssemblyName(args.Name);
            //concat both dynamocore paths and any reference paths the user added.
            var requestedAssemblyLocation = Program.DynamoDirectoryAssemblyPaths.Concat(Program.ReferenceAssemblyPaths.Select(x => new FileInfo(x)))
                .Where(x => Path.GetFileNameWithoutExtension(x.FullName) == requestedAssembly.Name)
                .FirstOrDefault();
           
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(requestedAssemblyLocation.FullName);
            }
            catch (Exception ex)
            {
                CommandHandler.LogExceptionToConsole(ex);
            }
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
        internal static void VerboseControlLog(string message)
        {
            if (Program.VerboseMode)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
    }
}
