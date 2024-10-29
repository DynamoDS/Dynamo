using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Workspaces;
using Dynamo.Tests;
using Greg;
using Greg.Requests;
using Greg.Responses;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.PackageManager.Tests
{
    class PackageManagerClientTests
    {
        #region ListAll

        [Test]
        public void ListAll_ReturnsNonEmptyListWhenPackagesAreAvailable()
        {
            // Returned content
            var mpl = new ResponseWithContentBody<List<PackageHeader>>
            {
                content = new List<PackageHeader>()
                {
                    new PackageHeader(){
                        versions = new List<PackageVersion>(){
                            new PackageVersion(){
                                url="test.zip"
                            }
                        }
                    }
                },
                success = true
            };

            // Returns mpl for any arguments
            var c = new Mock<IGregClient>();
            c.Setup(x =>
                x.ExecuteAndDeserializeWithContent<List<PackageHeader>>(It.IsAny<HeaderCollectionDownload>()))
                .Returns(mpl);

            var m = new PackageManagerClient(c.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

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

            var m = new PackageManagerClient(c.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var pl = m.ListAll();
            Assert.AreEqual(0, pl.Count());
        }

        #endregion

        #region GetPackageVersionHeader

        [Test]
        public void SuccessfullyGetPackageVersionHeaderByPackageId()
        {
            var version = "1.0.0";
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    content = new PackageVersion()
                    {
                        version = version
                    },
                    success = true
                });

            var client = new PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var result = client.GetPackageVersionHeader(new PackageInfo(string.Empty, new Version(version)));
            Assert.AreEqual(result.version, version);
        }

        [Test]
        public void FailureOnGetPackageVersionHeaderByPackageId()
        {
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    message = "The package does not exist",
                    success = false
                });

            var client = new PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var e = Assert.Throws<ApplicationException>(() =>
            {
                client.GetPackageVersionHeader(new PackageInfo(string.Empty, new Version()));
            });
            Assert.AreEqual("The package does not exist", e.Message);
        }

        [Test]
        public void SuccessfullyGetPackageVersionHeaderByPackageName()
        {
            var version = "1.0.0";
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    content = new PackageVersion()
                    {
                        version = version
                    },
                    success = true
                });

            var client = new PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var result = client.GetPackageVersionHeader(string.Empty, version);
            Assert.AreEqual(result.version, version);
        }

        [Test]
        public void FailureOnGetPackageVersionHeaderByPackageName()
        {
            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
                .Returns(new ResponseWithContentBody<PackageVersion>()
                {
                    message = "The package does not exist",
                    success = false
                });

            var client = new PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var e = Assert.Throws<ApplicationException>(() =>
            {
                client.GetPackageVersionHeader(string.Empty, string.Empty);
            });
            Assert.AreEqual("The package does not exist",e.Message);

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

            //var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");
        }

        [Test]
        public void DownloadPackage_ReturnsFailureObjectWhenDownloadThrowsAnException()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.Execute(It.IsAny<PackageDownload>())).Throws(new Exception("Failed to get your package!"));

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            string downloadPath;
            var res = pc.DownloadPackage("1", "0.1", out downloadPath);

            Assert.IsNull(downloadPath);
            Assert.IsFalse(res.Success);
        }

        #endregion

        #region Upvote

        [Test]
        public void Upvote_ReturnsTrueWhenRequestSucceeds()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<Upvote>())).Returns(new ResponseBody()
            {
                success = true
            });

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.Upvote("id");

            Assert.IsTrue(res);
        }

        [Test]
        public void Upvote_ReturnsFalseWhenRequestThrowsException()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<Upvote>())).Throws<Exception>();

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.Upvote("id");

            Assert.IsFalse(res);
        }

        #endregion

        #region GetTermsOfUseAcceptanceStatus

        [Test]
        public void GetTermsOfUseAcceptanceStatus_ReturnsTrueWhenAccepted()
        {
            var resp = new ResponseWithContentBody<TermsOfUseStatus>()
            {
                content = new TermsOfUseStatus()
                {
                    accepted = true
                }
            };

            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserializeWithContent<TermsOfUseStatus>(It.IsAny<TermsOfUse>())).Returns(resp);

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.GetTermsOfUseAcceptanceStatus();

            Assert.IsTrue(res);
        }

        [Test]
        public void GetTermsOfUseAcceptanceStatus_ReturnsFalseWhenNotAccepted()
        {
            var resp = new ResponseWithContentBody<TermsOfUseStatus>()
            {
                content = new TermsOfUseStatus()
                {
                    accepted = false
                }
            };

            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserializeWithContent<TermsOfUseStatus>(It.IsAny<TermsOfUse>())).Returns(resp);

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.GetTermsOfUseAcceptanceStatus();

            Assert.IsFalse(res);
        }

        [Test]
        public void GetTermsOfUseAcceptanceStatus_ReturnsFalseWhenRequestThrowsAnException()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserializeWithContent<TermsOfUseStatus>(It.IsAny<TermsOfUse>()))
                .Throws<Exception>();

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.GetTermsOfUseAcceptanceStatus();

            Assert.IsFalse(res);
        }

        #endregion

        #region SetTermsOfUseAcceptanceStatus

        [Test]
        public void SetTermsOfUseAcceptanceStatus_ReturnsTrueWhenRequestSucceeds()
        {
            var resp = new ResponseWithContentBody<TermsOfUseStatus>()
            {
                content = new TermsOfUseStatus()
                {
                    accepted = true
                }
            };

            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserializeWithContent<TermsOfUseStatus>(It.IsAny<TermsOfUse>())).Returns(resp);

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.SetTermsOfUseAcceptanceStatus();

            Assert.IsTrue(res);
        }

        [Test]
        public void SetTermsOfUseAcceptanceStatus_ReturnsTrueWhenRequestThrowsAnException()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserializeWithContent<TermsOfUseStatus>(It.IsAny<TermsOfUse>())).Throws<Exception>();

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.SetTermsOfUseAcceptanceStatus();

            Assert.IsFalse(res);
        }

        #endregion

        #region Publish

        [Test]
        public void Publish_SetsHandleToDoneWhenNewPackagePublishSucceeds()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<PackageUpload>()))
                .Returns(new ResponseBody()
                {
                    success = true
                });

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var pkg = new Package("", "Package", "0.1.0", "MIT");

            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));
            pc.Publish(pkg, Enumerable.Empty<string>(), Enumerable.Empty<string>(), false, handle, Enumerable.Empty<string>());

            Assert.AreEqual(PackageUploadHandle.State.Uploaded, handle.UploadState);
        }

        [Test]
        public void Publish_SetsHandleToDoneWhenNewPackageVersionPublishSucceeds()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<PackageVersionUpload>()))
                .Returns(new ResponseBody()
                {
                    success = true
                });

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var pkg = new Package("", "Package", "0.1.0", "MIT");

            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));
            pc.Publish(pkg, Enumerable.Empty<string>(), Enumerable.Empty<string>(), false, handle, Enumerable.Empty<string>());

            Assert.AreEqual(PackageUploadHandle.State.Uploaded, handle.UploadState);
        }

        [Test]
        public void Publish_SetsErrorStatusWhenRequestThrowsAnException()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<PackageUpload>())).Throws<Exception>();

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var pkg = new Package("", "Package", "0.1.0", "MIT");

            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));
            pc.Publish(pkg, Enumerable.Empty<string>(), Enumerable.Empty<string>(), false, handle, Enumerable.Empty<string>());

            Assert.AreEqual(PackageUploadHandle.State.Error, handle.UploadState);
        }

        [Test]
        public void Publish_SetsErrorStatusWhenResponseIsNull()
        {
            var gc = new Mock<IGregClient>();
            var rb = new ResponseBody();
            rb.success = false;
           
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<PackageUpload>())).Returns(rb);

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var pkg = new Package("", "Package", "0.1.0", "MIT");

            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));
            pc.Publish(pkg, Enumerable.Empty<string>(), Enumerable.Empty<string>(), true, handle, Enumerable.Empty<string>());

            Assert.AreEqual(PackageUploadHandle.State.Error, handle.UploadState);
        }

        #endregion

        #region PublishRetainingFolderStructure

        [Test]
        public void PublishRetain_SetsHandleToDoneWhenNewPackagePublishSucceeds()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<PackageUpload>()))
                .Returns(new ResponseBody()
                {
                    success = true
                });

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var pkg = new Package("", "Package", "0.1.0", "MIT");

            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));
            var listOfEmptyEnumerables = Enumerable.Range(1, 5)
                                                   .Select(_ => Enumerable.Empty<string>());

            pc.Publish(pkg, listOfEmptyEnumerables, Enumerable.Empty<string>(), false, handle, Enumerable.Empty<string>(), true);

            Assert.AreEqual(PackageUploadHandle.State.Uploaded, handle.UploadState);
        }

        [Test]
        public void PublishRetain_SetsHandleToDoneWhenNewPackageVersionPublishSucceeds()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<PackageVersionUpload>()))
                .Returns(new ResponseBody()
                {
                    success = true
                });

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var pkg = new Package("", "Package", "0.1.0", "MIT");

            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));
            var listOfEmptyEnumerables = Enumerable.Range(1, 5)
                                                   .Select(_ => Enumerable.Empty<string>());
            pc.Publish(pkg, listOfEmptyEnumerables, Enumerable.Empty<string>(), false, handle, Enumerable.Empty<string>(), true);

            Assert.AreEqual(PackageUploadHandle.State.Uploaded, handle.UploadState);
        }

        [Test]
        public void PublishRetain_SetsErrorStatusWhenRequestThrowsAnException()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<PackageUpload>())).Throws<Exception>();

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var pkg = new Package("", "Package", "0.1.0", "MIT");

            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));
            var listOfEmptyEnumerables = Enumerable.Range(1, 5)
                                                   .Select(_ => Enumerable.Empty<string>());
            pc.Publish(pkg, listOfEmptyEnumerables, Enumerable.Empty<string>(), false, handle, Enumerable.Empty<string>(), true);

            Assert.AreEqual(PackageUploadHandle.State.Error, handle.UploadState);
        }

        [Test]
        public void PublishRetain_SetsErrorStatusWhenResponseIsNull()
        {
            var gc = new Mock<IGregClient>();
            var rb = new ResponseBody();
            rb.success = false;

            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<PackageUpload>())).Returns(rb);

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var pkg = new Package("", "Package", "0.1.0", "MIT");

            var handle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(pkg));
            var listOfEmptyEnumerables = Enumerable.Range(1, 5)
                                                   .Select(_ => Enumerable.Empty<string>());
            pc.Publish(pkg, listOfEmptyEnumerables, Enumerable.Empty<string>(), true, handle, Enumerable.Empty<string>(), true);

            Assert.AreEqual(PackageUploadHandle.State.Error, handle.UploadState);
        }

        #endregion

        #region Deprecate

        [Test]
        public void Deprecate_ReturnsSuccessObjectWhenRequestSucceeds()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<Deprecate>())).Returns(new ResponseBody()
            {
                success = true
            });

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.Deprecate("id");

            Assert.IsTrue(res.Success);
        }

        [Test]
        public void Deprecate_ReturnsFailureObjectWhenRequestFails()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<Deprecate>())).Returns(new ResponseBody()
            {
                success = false // set to false
            });

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.Deprecate("id");

            Assert.IsFalse(res.Success);
        }

        [Test]
        public void Deprecate_ReturnsFailureObjectWhenRequestThrowsException()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<Deprecate>())).Throws(new Exception());

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.Deprecate("id");

            Assert.IsFalse(res.Success);
        }

        [Test]
        public void DoesCurrentUserOwnPackage_ReturnsFalseWhenCurrentUserIsNotTheOwner()
        {
            var id = "1";
            var username = "abcd";

            var mp = new ResponseWithContentBody<PackageHeader>
            {
                content = new PackageHeader()
                {
                    _id = id,
                    maintainers= new List<User>()
                },
                success = true
            };

            var c = new Mock<IGregClient>();
            c.Setup(x =>
                x.ExecuteAndDeserializeWithContent<PackageHeader>(It.IsAny<GetMaintainers>()))
                .Returns(mp);

            var pc = new PackageManagerClient(c.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");
            var res = pc.DoesCurrentUserOwnPackage(new Package("1","Package","2.0.4","1"), username);

            Assert.IsFalse(res);
        }
        [Test]
        public void DoesCurrentUserOwnPackage_ReturnsTrueWhenCurrentUserIsTheOwner()
        {
            var id = "1";
            var usrname = "abcd";
            User usr = new User {
                _id = id,
                username = usrname
            };

            var mp = new ResponseWithContentBody<PackageHeader>
            {
                content = new PackageHeader()
                {
                    _id = id,
                    maintainers = new List<User> { usr }
                },
                success = true
            };

            var c = new Mock<IGregClient>();
            c.Setup(x =>
                x.ExecuteAndDeserializeWithContent<PackageHeader>(It.IsAny<GetMaintainers>()))
                .Returns(mp);

            var pc = new PackageManagerClient(c.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");
            var res = pc.DoesCurrentUserOwnPackage(new Package("1", "Package", "2.0.4", "1"), usrname);

            Assert.IsTrue(res);
        }
        #endregion

        #region Undeprecate

        [Test]
        public void Undeprecate_ReturnsSuccessObjectWhenRequestSucceeds()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<Undeprecate>())).Returns(new ResponseBody()
            {
                success = true
            });

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.Undeprecate("id");

            Assert.IsTrue(res.Success);
        }

        [Test]
        public void Undeprecate_ReturnsFailureObjectWhenRequestFails()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<Undeprecate>())).Returns(new ResponseBody()
            {
                success = false // set to false
            });

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.Undeprecate("id");

            Assert.IsFalse(res.Success);
        }

        [Test]
        public void Undeprecate_ReturnsFailureObjectWhenRequestThrowsException()
        {
            var gc = new Mock<IGregClient>();
            gc.Setup(x => x.ExecuteAndDeserialize(It.IsAny<Undeprecate>())).Throws(new Exception());

            var pc = new PackageManagerClient(gc.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            var res = pc.Undeprecate("id");

            Assert.IsFalse(res.Success);
        }

        #endregion

        #region Compatibility Map

        [Test]
        public void CompatibilityMap_ReturnsValidMap()
        {
            // Mocking the response
            var responseContent = new List<JObject>
            {
                new JObject
                {
                    { "Revit", new JObject { { "2021", "2.5.2" }, { "2022", "2.10.1" } } }
                },
                new JObject
                {
                    { "Civil3D", new JObject { { "2021", "2.5.2" }, { "2022", "2.10.1" } } }
                }
            };

            var pkgResponse = new ResponseWithContentBody<object>
            {
                content = responseContent,
                success = true
            };

            // Mocking the IGregClient
            var mockClient = new Mock<IGregClient>();
            mockClient
                .Setup(c => c.ExecuteAndDeserializeWithContent<object>(It.IsAny<GetCompatibilityMap>()))
                .Returns(pkgResponse);

            // Instantiate the PackageManagerClient with the mocked IGregClient
            var packageManagerClient = new PackageManagerClient(mockClient.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            // Call CompatibilityMap
            var result = packageManagerClient.CompatibilityMap();

            // Assert that the result is not null and contains data
            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count); // We expect 2 entries in the compatibility map

            // Check that specific hosts are included
            Assert.IsTrue(result.Any(r => r.ContainsKey("Revit")));
            Assert.IsTrue(result.Any(r => r.ContainsKey("Civil3D")));
        }

        [Test]
        public void CompatibilityMap_ThrowsException_ReturnsNull()
        {
            // Mock IGregClient to throw an exception
            var mockClient = new Mock<IGregClient>();
            mockClient
                .Setup(c => c.ExecuteAndDeserializeWithContent<object>(It.IsAny<GetCompatibilityMap>()))
                .Throws(new Exception("Error fetching compatibility map"));

            // Instantiate the PackageManagerClient with the mocked IGregClient
            var packageManagerClient = new PackageManagerClient(mockClient.Object, MockMaker.Empty<IPackageUploadBuilder>(), "");

            // Call CompatibilityMap and ensure it catches the exception and returns null
            var result = packageManagerClient.CompatibilityMap();
            Assert.IsNull(result);
        }

        #endregion

    }
}
