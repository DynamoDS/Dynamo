using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

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
                Console.WriteLine(Resources.UpdaterPathRequiredMessage);
                return;
            }

            var installerPath = args[0];
            if (!File.Exists(installerPath))
            {
                Console.WriteLine(Resources.UpdaterPathNotFoundMessage);
                return;
            }

            int processId = -1;
            if (args.Length > 1)
            {
                if (!Int32.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out processId))
                {
                    Console.WriteLine(Resources.HostApplicationIdParseErrorMessage);
                    return;
                }
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
                    Console.WriteLine(Resources.MissingDynamoCertificateMessage);
                    return;
                }

                cert = DynamoCrypto.Utils.InstallCertificateForCurrentUser(certPath);
            }

            if (cert == null)
            {
                Console.WriteLine(Resources.SecurityCertificateErrorMessage);
                return;
            }

            // Check the download against an installed certificate.
            var pubKey = DynamoCrypto.Utils.GetPublicKeyFromCertificate(cert);
            if (pubKey == null)
            {
                Console.WriteLine(Resources.UpdateDownloadVerificationFailedMessage);
                RequestManualReinstall();
                return;
            }

            // Find the sig file that was downloaded
            var sigDir = Path.GetDirectoryName(installerPath);
            if (string.IsNullOrEmpty(sigDir) || !Directory.Exists(sigDir))
            {
                Console.WriteLine(Resources.MissingSignatureFileMessage);
                RequestManualReinstall();
                return;
            }

            var sigStub = Path.GetFileNameWithoutExtension(installerPath);
            var sigPath = Path.Combine(sigDir, sigStub + ".sig");

            if (!File.Exists(sigPath))
            {
                Console.WriteLine(Resources.MissingSignatureFileMessage);
                RequestManualReinstall();
                return;
            }

            if (!Utils.VerifyFile(installerPath, sigPath, pubKey))
            {
                Console.WriteLine(Resources.SignatureVerificationFailureMessage);
                RequestManualReinstall();
                return;
            }

            if (processId != -1)
            {
                bool cancel = false;
                int tryCount = 0;
                while (CheckHostProcessEnded(processId, out cancel) == false)
                {
                    if (cancel || tryCount == 5)
                    {
                        Console.WriteLine(Resources.UpdateCancellationMessage);
                        return;
                    }

                    // Allow the user 5 chances to get this right
                    // then cancel.
                    tryCount++;
                }
            }

            // Run the installer
            Process.Start(installerPath, "/UPDATE");
        }

        private static bool CheckHostProcessEnded(int processId, out bool requestCancel)
        {
            requestCancel = false;

            Process hostProcess = null;
            try
            {
                hostProcess = Process.GetProcessById(processId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // If the host process is still running...
            if (hostProcess != null)
            {
                var message = hostProcess.ProcessName + Resources.CloseContinuationMessage;

                if (MessageBox.Show(
                    new Form { TopMost = true },
                    message,
                    Resources.CheckHostProcessWindowTitle,
                    MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    requestCancel = true;
                    return true;
                }

                return false;
            }

            return true;
        }

        private static void RequestManualReinstall()
        {
            Console.WriteLine(Resources.ManualReinstallMessage);
            Console.WriteLine(Resources.ProcessQuitMessage);
            Console.ReadKey();
        }
    }
}
