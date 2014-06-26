using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DynamoCrypto
{
    public class Utils
    {
        /// <summary>
        /// Find a certificate in the key store and return the 
        /// private key, if one is available.
        /// </summary>
        /// <param name="keyContainerName">The key container name.</param>
        /// <returns>A byte array of the private key.</returns>
        public static byte[] FindCertificateAndGetPrivateKey(string keyContainerName)
        {
            byte[] privateBlob;

            X509Certificate2 cer = FindCertificate(keyContainerName);
            if (cer == null)
            {
                return null;
            }

            if (cer.HasPrivateKey)
            {
                var dsa = cer.PrivateKey as DSACryptoServiceProvider;

                if (dsa == null)
                {
                    Console.WriteLine("There was an error getting the private key from the certificate.");
                    return null;
                }

                //publicBlob = dsa.ExportCspBlob(false);
                privateBlob = dsa.ExportCspBlob(true);
                dsa.Dispose();
            }
            else
            {
                Console.WriteLine("The certificate does not contain a private key.");
                return null;
            }

            return privateBlob;
        }

        /// <summary>
        /// Find a certificate in the key store and return the
        /// public key, if one is available.
        /// </summary>
        /// <param name="keyContainerName">The key container name.</param>
        /// <returns>A byte array of the the public key.</returns>
        public static byte[] FindCertificateAndGetPublicKey(string keyContainerName)
        {
            X509Certificate2 cer = FindCertificate(keyContainerName);
            if (cer == null)
            {
                return null;
            }

            var dsa = cer.PublicKey.Key as DSACryptoServiceProvider;

            if (dsa == null)
            {
                Console.WriteLine("There was an error getting the public key from the certificate.");
                return null;
            }

            byte[] publicBlob = dsa.ExportCspBlob(false);
            dsa.Dispose();

            return publicBlob;
        }

        /// <summary>
        /// Generate a signature file using a private key.
        /// </summary>
        /// <param name="filePath">The file whose contents will be hashed.</param>
        /// <param name="signatureFilePath">The path of the generated signature file.</param>
        /// <param name="privateBlob">The private key.</param>
        public static void SignFile(string filePath, string signatureFilePath, byte[] privateBlob)
        {
            try
            {
                byte[] hash = null;

                using (Stream fileStream = File.Open(filePath, FileMode.Open))
                {
                    SHA1 sha1 = new SHA1CryptoServiceProvider();
                    hash = sha1.ComputeHash(fileStream);
                }

                // Import the private key
                var dsa = new DSACryptoServiceProvider();
                dsa.ImportCspBlob(privateBlob);
                var rsaFormatter = new DSASignatureFormatter(dsa);
                rsaFormatter.SetHashAlgorithm("SHA1");

                // Create a signature based on the private key
                byte[] signature = rsaFormatter.CreateSignature(hash);

                // Write the signature file
                File.WriteAllBytes(signatureFilePath, signature);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// Verify a file using a signature file and a public key.
        /// </summary>
        /// <param name="filePath">The file whose contents will be hashed.</param>
        /// <param name="signatureFilePath">The path of the signature file.</param>
        /// <param name="publicBlob">The public key.</param>
        /// <returns> True if the file is verified, otherwise false.</returns>
        public static bool VerifyFile(string filePath, string signatureFilePath, byte[] publicBlob)
        {
            bool verified = false;
            byte[] hash = null;

            try
            {
                // Compute a hash of the installer
                using (Stream fileStream = File.Open(filePath, FileMode.Open))
                {
                    SHA1 sha1 = new SHA1CryptoServiceProvider();
                    hash = sha1.ComputeHash(fileStream);
                }

                // Import the public key
                var dsa = new DSACryptoServiceProvider();
                dsa.ImportCspBlob(publicBlob);

                var dsaDeformatter = new DSASignatureDeformatter(dsa);
                dsaDeformatter.SetHashAlgorithm("SHA1");

                // Read the signature file
                byte[] signature = File.ReadAllBytes(signatureFilePath);

                // Verify the signature against the hash of the installer
                verified = dsaDeformatter.VerifySignature(hash, signature);

                Console.WriteLine(verified);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                return false;
            }

            return verified;
        }

        /// <summary>
        /// Find a certificate in the key store.
        /// </summary>
        /// <param name="keyContainerName">The key container name.</param>
        /// <returns>An X509Certificate2 or null if no certificate can be found.</returns>
        private static X509Certificate2 FindCertificate(string keyContainerName)
        {
            // Look for the Dynamo certificate in the certificate store. 
            // http://stackoverflow.com/questions/6304773/how-to-get-x509certificate-from-certificate-store-and-generate-xml-signature-dat
            var store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var cers = store.Certificates.Find(X509FindType.FindBySubjectName, keyContainerName, false);

            X509Certificate2 cer = null;
            if (cers.Count == 0)
            {
                Console.WriteLine("The certificate could not be found in the certificate store.");
                return null;
            }

            cer = cers[0];
            return cer;
        }

        /// <summary>
        /// Install a certificate in the local machine certificate store.
        /// </summary>
        /// <param name="certPath"></param>
        private static void InstallCertificate(string certPath)
        {
            var store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            var cert = new X509Certificate2(certPath);
            store.Add(cert);
        }
    }
}
