using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using Autodesk.RevitAddIns;
using Moq;
using NUnit.Framework;

namespace DynamoAddinGenerator
{
    [TestFixture]
    class Tests
    {
        [Test]
        public void GetValidProducts_FindsValidProducts()
        {
            var existingProds = SomeValidProducts();
            var prods = new RevitProductCollection(existingProds);
            Assert.AreEqual(3, prods.Products.Count());
        }

        [Test]
        public void GetValidProducts_FindsNoValidProducts()
        {
            var existingProds = NoValidProducts();
            var prods = new RevitProductCollection(existingProds);
            Assert.AreEqual(0, prods.Products.Count());
        }

        [Test]
        public void DynamoInstallCollection_OldInstall_Latest()
        {
            var dynamo1 = new DynamoInstall(DynamoVersions.dynamo_063);

            var installColl = new DynamoInstallCollection(new List<DynamoInstall> { dynamo1 });

            Assert.AreEqual(installColl.GetLatest().Folder, DynamoVersions.dynamo_063);
        }

        [Test]
        public void DynamoInstallCollection_OldAndNewInstalls_Latest()
        {
            var dynamo1 = new DynamoInstall(DynamoVersions.dynamo_063);
            var dynamo2 = new DynamoInstall(DynamoVersions.dynamo_071_x86);

            var installColl = new DynamoInstallCollection(new List<DynamoInstall>{dynamo1,dynamo2});

            Assert.AreEqual(installColl.GetLatest().Folder, DynamoVersions.dynamo_071_x86);
        }

        [Test]
        public void DynamoInstallCollection_NewInstalls_Latest()
        {
            var dynamo1 = new DynamoInstall(DynamoVersions.dynamo_071_x86);
            var dynamo2 = new DynamoInstall(DynamoVersions.dynamo_07x);

            var installColl = new DynamoInstallCollection(new List<DynamoInstall> { dynamo1, dynamo2 });

            Assert.AreEqual(installColl.GetLatest().Folder, DynamoVersions.dynamo_07x);
        }

        [Test]
        public void AddinData_071a()
        {
            var prod = CreateRevit2014();
            var latest = MockDynamoInstall(DynamoVersions.dynamo_071_x86);

            var addinData = new DynamoAddinData(prod, latest);

            Assert.AreEqual(addinData.AddinPath, Path.Combine(prod.AddinsFolder, "Dynamo.addin"));
            Assert.AreEqual(addinData.AssemblyPath, Path.Combine(DynamoVersions.dynamo_071_x86, "Revit_2014\\DynamoRevitVersionSelector.dll"));
            Assert.AreEqual(addinData.ClassName, "Dynamo.Applications.VersionLoader");
        }

        [Test]
        public void AddinData_071b_2014()
        {
            var prod = CreateRevit2014();
            var latest = MockDynamoInstall(DynamoVersions.dynamo_071_x64);

            var addinData = new DynamoAddinData(prod, latest);

            Assert.AreEqual(addinData.AddinPath, Path.Combine(prod.AddinsFolder, "Dynamo.addin"));
            Assert.AreEqual(addinData.AssemblyPath, Path.Combine(DynamoVersions.dynamo_071_x64, "Revit_2014\\DynamoRevitVersionSelector.dll"));
            Assert.AreEqual(addinData.ClassName, "Dynamo.Applications.VersionLoader");
        }

        [Test]
        public void AddinData_07x_2015()
        {
            var prod = CreateRevit2015();
            var latest = MockDynamoInstall(DynamoVersions.dynamo_07x);

            var addinData = new DynamoAddinData(prod, latest);

            Assert.AreEqual(addinData.AddinPath, Path.Combine(prod.AddinsFolder, "Dynamo.addin"));
            Assert.AreEqual(addinData.AssemblyPath, Path.Combine(DynamoVersions.dynamo_07x, "Revit_2015\\DynamoRevitVersionSelector.dll"));
            Assert.AreEqual(addinData.ClassName, "Dynamo.Applications.VersionLoader");
        }

        [Test]
        public void AddinDataIsCorrectInstallingOnMultipleRevits()
        {
            var existingProds = SomeValidProducts();
            var prods = new RevitProductCollection(existingProds);

            var dynamo1 = new DynamoInstall(DynamoVersions.dynamo_071_x86);
            var dynamo2 = new DynamoInstall(DynamoVersions.dynamo_07x);
            var dynamos = new DynamoInstallCollection(new List<DynamoInstall> { dynamo1, dynamo2 });

            var addinData = new DynamoAddinData(prods.Products.First(), dynamos.GetLatest());

            Assert.AreEqual(addinData.AddinPath, Path.Combine(prods.Products.First().AddinsFolder, "Dynamo.addin"));
            Assert.AreEqual(addinData.AssemblyPath, Path.Combine(DynamoVersions.dynamo_07x, "Revit_2014\\DynamoRevitVersionSelector.dll"));
            Assert.AreEqual(addinData.ClassName, "Dynamo.Applications.VersionLoader");
        }

        [Test]
        public void CanGenerateAddinIfFolderDoesNotExist()
        {
            var mockAddinData = new Mock<IDynamoAddinData>();
            var tmpDirectory = Path.Combine(Path.GetTempPath(), "testAddins");
            var tmpAddinPath = Path.Combine(tmpDirectory, "Dynamo.addin");
            mockAddinData.Setup(x => x.AddinPath).Returns(tmpAddinPath);

            Assert.DoesNotThrow(() => Program.GenerateDynamoAddin(mockAddinData.Object));

            if (File.Exists(tmpAddinPath))
            {
                File.Delete(tmpAddinPath);
            }

            if (Directory.Exists(tmpDirectory))
            {
                Directory.Delete(tmpDirectory);
            }
        }

        private IDynamoInstall MockDynamoInstall(string path)
        {
            var install = new Mock<IDynamoInstall>();
            install.Setup(x => x.Folder).Returns(path);
            return install.Object;
        }

        private IRevitProduct CreateRevit2013()
        {
            var prod = new Mock<IRevitProduct>();
            prod.Setup(x => x.InstallLocation).Returns(@"C:\Program Files\Autodesk\Revit");
            prod.Setup(x => x.AddinsFolder).Returns(@"C:\ProgramData\Autodesk\Revit\Addins\2013");
            prod.Setup(x => x.ProductName).Returns(@"Autodesk Revit 2013");
            prod.Setup(x => x.VersionString).Returns(RevitVersion.Revit2013.ToString());
            return prod.Object;
        }

        private IRevitProduct CreateRevit2014()
        {
            var prod = new Mock<IRevitProduct>();
            prod.Setup(x => x.InstallLocation).Returns(@"C:\Program Files\Autodesk\Revit");
            prod.Setup(x => x.AddinsFolder).Returns(@"C:\ProgramData\Autodesk\Revit\Addins\2014");
            prod.Setup(x => x.ProductName).Returns(@"Autodesk Revit 2014");
            prod.Setup(x => x.VersionString).Returns(RevitVersion.Revit2014.ToString());
            return prod.Object;
        }

        private IRevitProduct CreateRevit2015()
        {
            var prod = new Mock<IRevitProduct>();
            prod.Setup(x => x.InstallLocation).Returns(@"C:\Program Files\Autodesk\Revit");
            prod.Setup(x => x.AddinsFolder).Returns(@"C:\ProgramData\Autodesk\Revit\Addins\2015");
            prod.Setup(x => x.ProductName).Returns(@"Autodesk Revit 2015");
            prod.Setup(x => x.VersionString).Returns(RevitVersion.Revit2015.ToString());
            return prod.Object;
        }

        private IEnumerable<IRevitProduct> NoValidProducts()
        {
            // Two Revit 2014 products we should handle
            var p1 = new Mock<IRevitProduct>();
            p1.Setup(p => p.ProductName).Returns("Revit1");
            p1.Setup(p => p.InstallLocation).Returns(@"C:\Revit1\");
            p1.Setup(p => p.AddinsFolder).Returns(@"C:\addins1\");
            p1.Setup(p => p.VersionString).Returns(RevitVersion.Revit2011.ToString);

            var p2 = new Mock<IRevitProduct>();
            p2.Setup(p => p.ProductName).Returns("Revit2");
            p2.Setup(p => p.InstallLocation).Returns(@"C:\Revit2\");
            p2.Setup(p => p.AddinsFolder).Returns(@"C:\addins2\");
            p2.Setup(p => p.VersionString).Returns(RevitVersion.Revit2012.ToString);

            return new List<IRevitProduct>() { p1.Object, p2.Object };
        }

        private IEnumerable<IRevitProduct> SomeValidProducts()
        {
            // Two Revit 2014 products we should handle
            var p1 = new Mock<IRevitProduct>();
            p1.Setup(p => p.ProductName).Returns("Revit1");
            p1.Setup(p => p.InstallLocation).Returns(@"C:\Revit1\");
            p1.Setup(p => p.AddinsFolder).Returns(@"C:\addins1\");
            p1.Setup(p => p.VersionString).Returns(RevitVersion.Revit2014.ToString);

            var p2 = new Mock<IRevitProduct>();
            p2.Setup(p => p.ProductName).Returns("Revit2");
            p2.Setup(p => p.InstallLocation).Returns(@"C:\Revit2\");
            p2.Setup(p => p.AddinsFolder).Returns(@"C:\addins2\");
            p2.Setup(p => p.VersionString).Returns(RevitVersion.Revit2014.ToString);

            // A Revit 2015 product we should  handle
            var p3 = new Mock<IRevitProduct>();
            p3.Setup(p => p.ProductName).Returns("Revit3");
            p3.Setup(p => p.InstallLocation).Returns(@"C:\Revit3\");
            p3.Setup(p => p.AddinsFolder).Returns(@"C:\addins3\");
            p3.Setup(p => p.VersionString).Returns(RevitVersion.Revit2015.ToString);

            // A Revit 2011 product we should not handle
            var p4 = new Mock<IRevitProduct>();
            p4.Setup(p => p.ProductName).Returns("Revit4");
            p4.Setup(p => p.InstallLocation).Returns(@"C:\Revit4\");
            p4.Setup(p => p.AddinsFolder).Returns(@"C:\addins4\");
            p4.Setup(p => p.VersionString).Returns(RevitVersion.Revit2011.ToString);

            return new List<IRevitProduct>() { p1.Object, p2.Object, p3.Object, p4.Object };
        }
    }
}
