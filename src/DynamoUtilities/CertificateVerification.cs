using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamoUtilities
{
    public class CertificateVerification
    {
        /// <summary>
        /// Check if a .NET assembly can be loaded and has a valid certificate
        /// </summary>
        /// <param name="assemblyPath">Path of the assembly file</param>
        /// <returns></returns>
        public static bool CheckAssemblyForValidCertificate(string assemblyPath)
        {
            //Verify the assembly exists
            if (!File.Exists(assemblyPath))
            {
                throw new Exception(String.Format(
                    "A dll file was not found at {0}. No certificate was able to be verified.", assemblyPath));
            }

            //Verify that you can load the assembly into a Reflection only context
            Assembly asm;
            try
            {
                asm = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            }
            catch
            {
                throw new Exception(String.Format(
                    "A dll file found at {0} could not be loaded.", assemblyPath));
            }

            //Verify the node library has a verified signed certificate
            var cert = asm.Modules.FirstOrDefault()?.GetSignerCertificate();
            if (cert != null)
            {
                var cert2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(cert);
                if (cert2.Verify())
                {
                    return true;
                }
            }

            throw new Exception(String.Format(
                "A dll file found at {0} did not have a signed certificate.", assemblyPath));
        }
    }
}
