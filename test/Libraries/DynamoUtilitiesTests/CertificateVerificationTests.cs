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
        public void CheckAssemblyForValidCertificateTest_ValidCertificate()
        {
            var executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var directory = new DirectoryInfo(executingDirectory);
            var testFilePath = Path.Combine(directory.Parent.Parent.Parent.FullName, "test", "pkgs_signed", "Signed Package", "bin", "SignedPackage.dll");

            Assert.IsTrue(File.Exists(testFilePath));

            Assert.IsTrue(DynamoUtilities.CertificateVerification.CheckAssemblyForValidCertificate(testFilePath));
        }

        [Test]
        [Category("UnitTests")]
        public void CheckAssemblyForValidCertificateTest_NoCertificate()
        {
            var executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var directory = new DirectoryInfo(executingDirectory);
            var testFilePath = Path.Combine(directory.Parent.Parent.Parent.FullName, "test", "pkgs_signed", "Unsigned Package", "bin", "Package.dll");

            Assert.IsTrue(File.Exists(testFilePath));

            Assert.Throws(
                typeof(CertificateVerification.UnTrustedAssemblyException), 
                () =>
                {
                    DynamoUtilities.CertificateVerification.CheckAssemblyForValidCertificate(testFilePath);
                }
            ); 
        }

        [Test]
        [Category("UnitTests")]
        public void CheckAssemblyForValidCertificateTest_TransferredCertificate()
        {
            var executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var directory = new DirectoryInfo(executingDirectory);
            var testFilePath = Path.Combine(directory.Parent.Parent.Parent.FullName, "test", "pkgs_signed", "Modfied Signed Package", "bin", "LegitAssemblyTransplanted.dll");

            Assert.IsTrue(File.Exists(testFilePath));

            Assert.Throws(
                typeof(CertificateVerification.UnTrustedAssemblyException),
                () =>
                {
                    DynamoUtilities.CertificateVerification.CheckAssemblyForValidCertificate(testFilePath);
                }
            );
        }
    }
}
