using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Ionic.Zip;

namespace ACGClientForCEF.Utility
{
    public class FileUtilities
    {

        /// <summary>
        /// Signs a hash with the RSA algorithm
        /// </summary>
        /// <param name="input"></param>
        public static void Sign(byte[] input)
        {
            using (var rsa = new RSACryptoServiceProvider(4096) )
            {

                var bytesToDecrypt = Convert.FromBase64String("la0Cz.....D43g=="); // string to decrypt, base64 encoded

                try
                {
                    var ps = new RSAParameters();
                    // ps.Modulus = my private key
                    // ps.Exponent = public key

                    rsa.ImportParameters(ps);

                    byte[] output = rsa.Encrypt(input, false);

                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        /// <summary>
        /// Compute MD5 checksum of file download
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static byte[] GetMD5Checksum(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }

        /// <summary>
        /// Create a SHA-256 Secure Hash from a file
        /// </summary>
        /// <param name="fInfo"></param>
        /// <returns></returns>
        public static byte[] GetFileHash(FileInfo fInfo)
        {
            var mySHA256 = SHA256Managed.Create();

            byte[] hashValue;

            // Create a fileStream for the file.
            var fileStream = fInfo.Open(FileMode.Open);
            // Be sure it's positioned to the beginning of the stream.
            fileStream.Position = 0;
            // Compute the hash of the fileStream.
            hashValue = mySHA256.ComputeHash(fileStream);

            // Close the file.
            fileStream.Close();

            return hashValue;
        }

        public static string GetTempFolder()
        {
            return Path.GetDirectoryName(Path.GetTempPath());
        }

        public static string GetTempZipPath()
        {
            var tempFolder = GetTempFolder();
            return Path.Combine(tempFolder, "gregPkg" + DateTime.Now.Millisecond.ToString() + ".zip");
        }

        public static string GetTempZipOutputPath()
        {
            var tempFolder = GetTempFolder();
            return Path.Combine(tempFolder, "gregPkgOutput" + DateTime.Now.Millisecond.ToString());
        }

        /// <summary>
        /// Make a zip file from a collection of files
        /// </summary>
        /// <param name="paths">A list of filepaths</param>
        /// <returns>False if the process fails, true otherwise</returns>
        /// <throws>FileNotFoundException, ZipException</throws>
        public static string Zip(IEnumerable<string> filePaths)
        {
            var zipPath = GetTempZipPath();

            using (var zip = new ZipFile())
            {
                foreach (var filename in filePaths)
                {
                    ZipEntry e = zip.AddFile(filename, "/");
                    e.Comment = "Added by GregClient.";
                }

                zip.Comment = String.Format("This zip archive was created by GregClient.");
                zip.Save(zipPath);
            }

            return zipPath;
        }

        /// <summary>
        /// Make a zip file from a collection of files
        /// </summary>
        /// <param name="paths">A list of filepaths</param>
        /// <returns>False if the process fails, true otherwise</returns>
        /// <throws>FileNotFoundException, ZipException</throws>
        public static string Zip(string directory)
        {
            var zipPath = GetTempZipPath();

            using (var zip = new ZipFile())
            {
                ZipEntry e = zip.AddDirectory(directory, "/");
                e.Comment = "Added by GregClient.";
                zip.Comment = String.Format("This zip archive was created by GregClient.");
                zip.Save(zipPath);
            }

            return zipPath;
        }


        /// <summary>
        /// Given a path to a zip, extracts it and returns the 
        /// temp directory where it is located.
        /// </summary>
        /// <param name="zipFilePath"></param>
        /// <returns></returns>
        public static string UnZip(string zipFilePath)
        {
            var outputPath = GetTempZipOutputPath();
            var count = 0;
            while (Directory.Exists(outputPath + count))
            {
                count++;
            }
            outputPath = outputPath + count;

            UnZip(zipFilePath, outputPath);
            return outputPath;
        }

        /// <summary>
        /// Given a path to a zip and a destination directory.
        /// </summary>
        /// <param name="zipFilePath"></param>
        /// <returns></returns>
        public static void UnZip(string zipFilePath, string unzipDirectory)
        {
            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.ExtractAll(unzipDirectory);
            }
        }

    }
}
