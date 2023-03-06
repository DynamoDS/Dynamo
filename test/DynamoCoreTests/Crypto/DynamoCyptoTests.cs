using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

//TODO Get rid of these tests, DynamoSign and DynamoCrypto.
#if NETFRAMEWORK

namespace DynamoCrypto
{
    [TestFixture]
    class DynamoCyptoTests
    {
        [Test]
        [Category("Failure")]
        public void VerificationFailsForAlteredFile()
        {
            var asm = Assembly.GetExecutingAssembly().Location;
            var asmDir = Path.GetDirectoryName(asm);
            var testFile =
                Path.GetFullPath(
                    Path.Combine(asmDir, @"..\..\..\test\DynamoCoreTests\Crypto\AnAlteredFile.txt"));
            var sigFile =
                Path.GetFullPath(
                    Path.Combine(asmDir, @"..\..\..\test\DynamoCoreTests\Crypto\AnImportantFile.sig"));

            var pubKey = AssertCertAndPublicKey();

            var verify = Utils.VerifyFile(testFile, sigFile, pubKey);
            Assert.False(verify, "The verification passed, but the file had been altered.");
        }

        [Test]
        [Category("Failure")]
        public void VerificationFailsForAlteredSignature()
        {
            var asm = Assembly.GetExecutingAssembly().Location;
            var asmDir = Path.GetDirectoryName(asm);
            var testFile =
                Path.GetFullPath(
                    Path.Combine(asmDir, @"..\..\..\test\DynamoCoreTests\Crypto\AnImportantFile.txt"));
            var sigFile =
                Path.GetFullPath(
                    Path.Combine(asmDir, @"..\..\..\test\DynamoCoreTests\Crypto\AnAlteredSignature.sig"));

            var pubKey = AssertCertAndPublicKey();

            var verify = Utils.VerifyFile(testFile, sigFile, pubKey);
            Assert.False(verify, "The verification passed, but the signature file had been altered.");
        }

        [Test]
        [Category("Failure")]
        public void VertificationSucceedsIfAllIsGood()
        {
            var asm = Assembly.GetExecutingAssembly().Location;
            var asmDir = Path.GetDirectoryName(asm);
            var testFile =
                Path.GetFullPath(
                    Path.Combine(asmDir, @"..\..\..\test\DynamoCoreTests\Crypto\AnImportantFile.txt"));
            var sigFile =
                Path.GetFullPath(
                    Path.Combine(asmDir, @"..\..\..\test\DynamoCoreTests\Crypto\AnImportantFile.sig"));

            var pubKey = AssertCertAndPublicKey();

            var verify = Utils.VerifyFile(testFile, sigFile, pubKey);
            Assert.True(verify, "The file could not be verified against the signature.");
        }

        private static byte[] AssertCertAndPublicKey()
        {
            var cert = Utils.FindCertificateForCurrentUser("Dynamo", StoreLocation.CurrentUser);
            Assert.NotNull(cert, "Dynamo certificate could not be found. Perhaps you need to install the certificate on the testing machine?");

            var pubKey = Utils.GetPublicKeyFromCertificate(cert);
            Assert.NotNull(pubKey, "A public key could not be returned from the certificate.");

            return pubKey;
        }
    }
}

#endif
