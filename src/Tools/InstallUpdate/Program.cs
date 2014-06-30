using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using DynamoCrypto;

namespace InstallUpdate
{
    /// <summary>
    /// This application verifies an installer against a signature file located
    /// in the same directory, using a public key located in a certificate
    /// store on the user's machine. If the public key does not exist in the current
    /// user's certificate store, it is added. If the installer is verified against the
    /// signature file, then it is run.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("You must specify the path of the update to install.");
                return;
            }

            var installerPath = args[0];
            if (!File.Exists(installerPath))
            {
                Console.WriteLine("The specified file path does not exist.");
                return;
            }

            // Attempt to find the Dynamo certificate.
            var cert = Utils.FindCertificateForCurrentUser("Dynamo", StoreLocation.CurrentUser);

            // If the certificate can't be found, install it
            // in the current user's certificate store.
            if (cert == null)
            {
                var certPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Dynamo.cer");
                if (!File.Exists(certPath))
                {
                    Console.WriteLine("The Dynamo certificate could not be found. Update cancelled.");
                    return;
                }

                cert = DynamoCrypto.Utils.InstallCertificateForCurrentUser(certPath);
            }

            if (cert == null)
            {
                Console.WriteLine("There was a problem with the security certificate. Update cancelled.");
                return;
            }

            // Check the download against an installed certificate.
            var pubKey = DynamoCrypto.Utils.GetPublicKeyFromCertificate(cert);
            if (pubKey == null)
            {
                Console.WriteLine("Could not verify the update download");
                RequestManualReinstall();
                return;
            }

            // Find the sig file that was downloaded
            var sigDir = Path.GetDirectoryName(installerPath);
            if (string.IsNullOrEmpty(sigDir) || !Directory.Exists(sigDir))
            {
                Console.WriteLine("A signature file could not be found to verify this update.");
                RequestManualReinstall();
                return;
            }

            var sigStub = Path.GetFileNameWithoutExtension(installerPath);
            var sigPath = Path.Combine(sigDir, sigStub + ".sig");

            if (!File.Exists(sigPath))
            {
                Console.WriteLine("A signature file could not be found to verify this update.");
                RequestManualReinstall();
                return;
            }

            if (!Utils.VerifyFile(installerPath, sigPath, pubKey))
            {
                Console.WriteLine("The update could not be verified against the signature file.");
                RequestManualReinstall();
                return;
            }

            // Run the installer
            Process.Start(installerPath);
        }

        private static void RequestManualReinstall()
        {
            Console.WriteLine("Please reinstall Dynamo manually.");
            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
        }
    }
}
