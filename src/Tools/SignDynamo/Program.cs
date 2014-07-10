using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using DynamoCrypto;
using NDesk.Options;

namespace SignDynamo
{
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

            var cert = Utils.FindCertificateForCurrentUser(keyContainerName, StoreLocation.LocalMachine);
            if (cert == null)
            {
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

            var p = new OptionSet
            {
                {"i:|installer","The path to the installer to sign.", v=> installerPath = v},
                {"h|help", "Show this message and exit.", v=> showHelp = v != null}
            };

            var notParsed = new List<string>();

            const string helpMessage = "Try 'SignDynamo --help' for more information.";

            try
            {
                notParsed = p.Parse(args);
            }
            catch (OptionException e)
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
                ShowHelp(p);
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

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: SignDynamo [OPTIONS]");
            Console.WriteLine("Generate a signature file for a dynamo installer.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
