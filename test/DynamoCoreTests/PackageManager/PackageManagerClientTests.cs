using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.PackageManager;
using Greg;
using Greg.Requests;
using Greg.Responses;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    public static class MakeMock
    {
        public static T Empty<T>() where T : class
        {
            return (new Mock<T>()).Object;
        }
    }

    class PackageManagerClientTests
    {
        // No Authentication Required

            // ListAll
            // DownloadPackageHeader
            // DownloadPackage

        // Requires Authentication

            // Upvote
            // Downvote
            // DownloadPackage
            // GetTermsOfUseAcceptanceStatus
            // SetTermsOfUseAcceptanceStatus
            // Publish
            // Deprecate
            // Undeprecate
            // Logout
            // Login


        #region ListAll

        [Test]
        public void ListAll_ReturnsNonEmptyListWhenPackagesAreAvailable()
        {
            // Returned content
            var mpl = new ResponseWithContentBody<List<PackageHeader>>
            {
                content = new List<PackageHeader>()
                {
                    new PackageHeader()
                },
                success = true
            };

            // Returns mpl for any arguments
            var c = new Mock<IGregClient>();
            c.Setup(x =>
                x.ExecuteAndDeserializeWithContent<List<PackageHeader>>(It.IsAny<HeaderCollectionDownload>()))
                .Returns(mpl);

            var m = new PackageManagerClient(c.Object, MakeMock.Empty<IPackageUploadBuilder>());

            var pl = m.ListAll();
            Assert.AreNotEqual(0, pl.Count());
        }

        [Test]
        public void ListAll_ReturnsEmptyListWhenQueryThrowsException()
        {
            // Returns mpl for any arguments
            var c = new Mock<IGregClient>();
            c.Setup(x =>
                x.ExecuteAndDeserializeWithContent<List<PackageHeader>>(It.IsAny<HeaderCollectionDownload>()))
                .Throws(new Exception("Fail!"));

            var m = new PackageManagerClient(c.Object, MakeMock.Empty<IPackageUploadBuilder>());

            var pl = m.ListAll();
            Assert.AreEqual(0, pl.Count());
        }

        #endregion

        #region DownloadPackageHeader

        [Test]
        public void DownloadPackageHeader_SucceedsForValidPackage()
        {
            var id = "1";

            // Returned content
            var mp = new ResponseWithContentBody<PackageHeader>
            {
                content = new PackageHeader()
                {
                    _id = id
                },
                success = true
            };

            // Returns mock for any arguments
            var c = new Mock<IGregClient>();
            c.Setup(x =>
                x.ExecuteAndDeserializeWithContent<PackageHeader>(It.IsAny<HeaderDownload>()))
                .Returns(mp);

            var pc = new PackageManagerClient(c.Object, MakeMock.Empty<IPackageUploadBuilder>());

            PackageHeader header;
            var res = pc.DownloadPackageHeader(id, out header);

            Assert.AreEqual(id, header._id);
            Assert.IsTrue(res.Success);
        }

        [Test]
        public void DownloadPackageHeader_ReturnsFailureObjectWhenDownloadThrowsAnException()
        {
            // Returns mock for any arguments
            var c = new Mock<IGregClient>();
            c.Setup(x =>
                x.ExecuteAndDeserializeWithContent<PackageHeader>(It.IsAny<HeaderDownload>()))
                .Throws<Exception>();

            var pc = new PackageManagerClient(c.Object, MakeMock.Empty<IPackageUploadBuilder>());

            PackageHeader header;
            var res = pc.DownloadPackageHeader("1", out header);

            Assert.IsFalse(res.Success);
        }

        #endregion

        #region DownloadPackage

        [Test]
        public void DownloadPackage_GivesValidFileForValidPackage()
        {
            Assert.Inconclusive("Requires additional changes to Greg to mock.  PackageDownload.GetFileFromResponse is not mockable");
            
            //var fs = new Mock<IFileSystem>();
            //fs.Setup(x => x.FileExists("I:/do/not/know.zip"));

            //var gc = new Mock<IGregClient>();
            //gc.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Returns( Mockable

            //var pc = new PackageManagerClient(gc.Object, MakeMock.Empty<IPackageUploadBuilder>());
        }

        [Test]
        public void DownloadPackage_ReturnsFailureObjectWhenDownloadThrowsAnException()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Throws(new Exception("Failed to get your package!"));

            var pc = new PackageManagerClient(gc.Object, MakeMock.Empty<IPackageUploadBuilder>());

            string downloadPath;
            var res = pc.DownloadPackage("1", "0.1", out downloadPath);

            Assert.IsNull(downloadPath);
            Assert.IsFalse(res.Success);
        }

        #endregion

        #region Upvote



        #endregion

        #region Downvote
        #endregion

        #region DownloadPackage
        #endregion

        #region GetTermsOfUseAcceptanceStatus
        #endregion

        #region SetTermsOfUseAcceptanceStatus
        #endregion

        #region Publish
        #endregion

        #region Deprecate
        #endregion

        #region Undeprecate
        #endregion

        #region Logout
        #endregion

        #region Login
        #endregion

    }
}