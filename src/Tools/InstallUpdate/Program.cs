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
                Console.WriteLine("You must specify the path of the update.");
                return;
            }

            var installerPath = args[0];
            if (!File.Exists(installerPath))
            {
                Console.WriteLine("The specified file path does not exist.");
                return;
            }

            int processId = -1;
            if (args.Length > 1)
            {
                if (!Int32.TryParse(
                    args[1],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out processId))
                {
                    Console.WriteLine("The host application process id could not be parsed from the specified input.");
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

            if (processId != -1)
            {
                bool cancel = false;
                while (CheckHostProcessEnded(processId, out cancel) == false)
                {
                    if (cancel)
                    {
                        Console.WriteLine("Update was cancelled.");
                        return;
                    }
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
                var message = string.Format(
                    "{0} must be closed before continuing installation.\n" +
                        "When the application is closed, select OK to continue updating, or Cancel to quit updating.",
                    hostProcess.ProcessName);

                if (MessageBox.Show(
                    message,
                    "Dynamo Update",
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
            Console.WriteLine("Please reinstall Dynamo manually.");
            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
        }
    }

    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        public WindowWrapper(IntPtr handle)
        {
            _hwnd = handle;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }

        private IntPtr _hwnd;
    }
}
