using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using CommandLine;
using CommandLine.Text;
using DynamoCrypto;

namespace SignDynamo
{
    internal class CMDLineOptions
    {
        [Option('i', "installer", Required = false, HelpText = "The path to the installer to sign.")]
        public string Installer { get; set; } = String.Empty;

        [Option('h', "help", Required = false, HelpText = "Show this message and exit.")]
        public bool ShowHelp { get; set; }
    }

    class Program
    {
        private static byte[] privateBlob = null;
        private static string installerPath;
        private const string keyContainerName = "Dynamo";

        static void Main(string[] args)
        {
            if (!ParseArguments(args))
            {
                Console.WriteLine("Could not parse the input arguments.");
                Console.WriteLine("Press any key to quit.");
                Console.ReadKey();
                return;
            }

            //First try at LocalMachine store location, if certificate is not installed check at CurrentUser
            //store location.
            var cert = Utils.FindCertificateForCurrentUser(keyContainerName, StoreLocation.LocalMachine);
            if (cert == null)
            {
                cert = Utils.FindCertificateForCurrentUser(keyContainerName, StoreLocation.CurrentUser);
                if (null == cert)
                    return;
            }

            privateBlob = Utils.GetPrivateKeyFromCertificate(cert);
            if (privateBlob == null)
            {
                return;
            }

            var sigPath = Path.Combine(Path.GetDirectoryName(installerPath),
                Path.GetFileNameWithoutExtension(installerPath) + ".sig");

            Utils.SignFile(installerPath, sigPath, privateBlob);

            Console.WriteLine("Signature generation complete.");
        }

        private static bool ParseArguments(IEnumerable<string> args)
        {
            var showHelp = false;
            var parser = new Parser(options =>
            {
                options.IgnoreUnknownArguments = true; options.HelpWriter = Console.Out;
                options.CaseSensitive = false;
            });
            var notParsed = new List<string>();

            const string helpMessage = "Try 'SignDynamo --help' for more information.";

            ParserResult<CMDLineOptions> parserResult;
            var lineOptions = new CMDLineOptions();
            var errors = new List<Error>();

            try
            {
                parserResult = Parser.Default.ParseArguments<CMDLineOptions>(args)
                                                .WithParsed(o => lineOptions = o)
                                                .WithNotParsed(e => errors = (List<Error>)e);

                errors.ForEach(x => notParsed.Add(x.ToString()));
                installerPath = lineOptions.Installer;
                showHelp = lineOptions.ShowHelp;
            }
            catch (AggregateException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(helpMessage);
                return false;
            }

            if (notParsed.Count > 0)
            {
                Console.WriteLine(String.Join(" ", notParsed.ToArray()));
                return false;
            }

            if (showHelp)
            {
                ShowHelp(parserResult);
                return false;
            }

            if (string.IsNullOrEmpty(installerPath))
            {
                Console.WriteLine("You must specify a path to an installer to be signed when using the -s flag.");
                return false;
            }

            if (!File.Exists(installerPath))
            {
                Console.Write("The specified installer path does not exist.");
                return false;
            }

            return true;
        }

        private static void ShowHelp(ParserResult<CMDLineOptions> results)
        {
            Console.WriteLine("Usage: SignDynamo [OPTIONS]");
            Console.WriteLine("Generate a signature file for a dynamo installer.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine(HelpText.AutoBuild(results, null, null).ToString());
        }
    }
}
