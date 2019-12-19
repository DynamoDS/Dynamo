using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DynamoUtilities;
using NUnit.Framework;

namespace DynamoUtilitiesTests
{
    class CertificateVerificationTests 
    {
        [Test]
        [Category("UnitTests")]
        public void CheckAssemblyForValidCertificateTest()
        {
            var executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var directory = new DirectoryInfo(executingDirectory);
            var testFilePath = Path.Combine(directory.Parent.Parent.Parent.FullName, "test", "pkgs_signed", "Signed Package", "bin", "SignedPackage.dll");

            Assert.IsTrue(File.Exists(testFilePath));

            Assert.IsTrue(DynamoUtilities.CertificateVerification.CheckAssemblyForValidCertificate(testFilePath));
        }
    }
}
