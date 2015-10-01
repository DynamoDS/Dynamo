using System;
using System.Collections.Generic;
using System.Linq;

using DynamoInstallDetective;

using Moq;

using NUnit.Framework;

namespace DynamoUtilitiesTests
{
    public class ProductLookUpTests
    {
        [Test, Category("ProductLookUp"), Category("UnitTests")]
        public void GetInstalledProducts()
        {
            var products = new Dictionary<string, Tuple<int, int, int, int>>
            {
                { "A", Tuple.Create(0, 1, 2, 3) },
                { "B", Tuple.Create(0, 1, 3, 4) },
                { "C", Tuple.Create(1, 2, 3, 4) },
                { "D", Tuple.Create(1, 0, 3, 4) },
                { "E", null }
            };
            var lookUp = SetUpProductLookUp(products, null);

            var p = new InstalledProducts();
            p.LookUpAndInitProducts(lookUp);

            Assert.AreEqual("C", p.GetLatestProduct().ProductName);
            Assert.AreEqual("1.2.3.4", p.GetLatestProduct().VersionString);
            Assert.AreEqual(4, p.Products.Count());
        }

        [Test, Category("ProductLookUp"), Category("UnitTests")]
        public void GetDistinctInstalledProducts()
        {
            var products = new Dictionary<string, Tuple<int, int, int, int>>
            {
                { "A", Tuple.Create(0, 1, 2, 3) },
                { "B", Tuple.Create(0, 1, 3, 4) },
                { "C", Tuple.Create(1, 2, 3, 4) },
                { "D", Tuple.Create(1, 0, 3, 4) },
                { "E", null },
                { "F", Tuple.Create(1, 2, 3, 4) },
                { "G", Tuple.Create(1, 0, 3, 4) }
            };
            var lookUp = SetUpProductLookUp(products, null);

            var p = new InstalledProducts();
            p.LookUpAndInitProducts(lookUp);

            Assert.AreEqual("1.2.3.4", p.GetLatestProduct().VersionString);
            Assert.AreEqual(4, p.Products.Count());
        }

        [Test, Category("ProductLookUp"), Category("UnitTests")]
        public void GetInstalledDynamoProducts()
        {
            const string myPath = @"C:\MyXYZ\";
            const string dyn063 = @"C:\Autodesk\Dynamo\Core";
            var products = new Dictionary<string, Tuple<int, int, int, int>>
            {
                { "A", Tuple.Create(0, 1, 2, 3) },
                { "B", Tuple.Create(0, 1, 3, 4) },
                { "C", Tuple.Create(1, 2, 3, 4) },
                { "D", Tuple.Create(1, 0, 3, 4) },
                { "E", null }
            };
            var locations = new Dictionary<string, Tuple<int, int, int, int>>
            {
                { myPath, Tuple.Create(0, 8, 2, 3) },
                { dyn063, Tuple.Create(0, 6, 3, 4242) },
            };
            var lookUp = SetUpProductLookUp(products, locations);

            var p = DynamoProducts.FindDynamoInstallations(myPath, lookUp);
            
            Assert.AreEqual("C", p.GetLatestProduct().ProductName);
            Assert.AreEqual("1.2.3.4", p.GetLatestProduct().VersionString);
            Assert.AreEqual(6, p.Products.Count());

            //Find product name starting with "XYZ", name given for lookup
            var prods = p.Products.Where(x => x.ProductName.StartsWith("XYZ"));
            var prod063 = prods.First();
            var prod082 = prods.Last();
            Assert.AreEqual(2, prods.Count());

            Assert.AreEqual(dyn063, prod063.InstallLocation);
            Assert.AreEqual("XYZ 0.6", prod063.ProductName);
            Assert.AreEqual("0.6.3.4242", prod063.VersionString);

            Assert.AreEqual(myPath, prod082.InstallLocation);
            Assert.AreEqual("XYZ 0.8", prod082.ProductName);
            Assert.AreEqual("0.8.2.3", prod082.VersionString);
        }

        [Test, Category("ProductLookUp"), Category("UnitTests")]
        public void DynamoProductsOverrideWithDebugPath()
        {
            const string myPath = @"C:\MyXYZ\";
            const string dyn063 = @"C:\Autodesk\Dynamo\Core";
            var products = new Dictionary<string, Tuple<int, int, int, int>>
            {
                { "A", Tuple.Create(0, 1, 2, 3) },
                { "B", Tuple.Create(0, 1, 3, 4) },
                { "C", Tuple.Create(0, 8, 2, 4) },
                { "D", Tuple.Create(1, 0, 3, 4) },
                { "E", null }
            };
            var locations = new Dictionary<string, Tuple<int, int, int, int>>
            {
                { myPath, Tuple.Create(0, 8, 2, 3) },
                { dyn063, Tuple.Create(0, 6, 3, 4242) },
            };
            var lookUp = SetUpProductLookUp(products, locations);

            var p = DynamoProducts.FindDynamoInstallations(myPath, lookUp);

            Assert.AreEqual("D", p.GetLatestProduct().ProductName);
            Assert.AreEqual("1.0.3.4", p.GetLatestProduct().VersionString);
            Assert.AreEqual(5, p.Products.Count());

            //Product C is dropped because myPath with same version is provided.
            Assert.IsFalse(p.Products.Any(x => x.ProductName == "C"));

            //Find product name starting with "XYZ", name given for lookup
            var prods = p.Products.Where(x => x.ProductName.StartsWith("XYZ"));
            var prod063 = prods.First();
            var prod082 = prods.Last();
            Assert.AreEqual(2, prods.Count());

            Assert.AreEqual(dyn063, prod063.InstallLocation);
            Assert.AreEqual("XYZ 0.6", prod063.ProductName);
            Assert.AreEqual("0.6.3.4242", prod063.VersionString);

            Assert.AreEqual(myPath, prod082.InstallLocation);
            Assert.AreEqual("XYZ 0.8", prod082.ProductName);
            Assert.AreEqual("0.8.2.3", prod082.VersionString);
        }

        [Test, Category("ProductLookUp"), Category("UnitTests")]
        public void GetProductFromInstallPath()
        {
            var lookUp = new Mock<InstalledProductLookUp>(new object[]{"XYZ", "some.dll"});
            lookUp.Setup(l => l.ExistsAtPath(It.IsAny<string>())).Returns(true);
            lookUp.Setup(l => l.GetCoreFilePathFromInstallation(It.IsAny<string>()))
                .Returns<string>(s => s);
            lookUp.Setup(l => l.GetVersionInfoFromFile(It.IsAny<string>()))
                .Returns(Tuple.Create(0, 6, 3, 5322));

            const string path = @"C:\MyDrive\Dynamo\Core";
            var p = lookUp.Object.GetProductFromInstallPath(path);
            Assert.AreEqual("0.6.3.5322", p.VersionString);
            Assert.AreEqual(path, p.InstallLocation);
            Assert.AreEqual("XYZ 0.6", p.ProductName);
        }

        [Test, Category("ProductLookUp"), Category("UnitTests")]
        public void NoProductFromInstallPath()
        {
            var lookUp = new Mock<InstalledProductLookUp>(new object[] { "Dynamo", "some.dll" });
            lookUp.Setup(l => l.ExistsAtPath(It.IsAny<string>())).Returns(false);
            
            const string path = @"C:\MyDrive\Dynamo\Core";
            var p = lookUp.Object.GetProductFromInstallPath(path);
            Assert.IsNull(p);
        }

        [Test, Category("ProductLookUp"), Category("UnitTests")]
        public void GetProductFromProductName()
        {
            const string path = @"C:\MyDrive\Dynamo\Core";
            const string name = @"My New Dynamo";
            var lookUp = new Mock<InstalledProductLookUp>(new object[] { "Dynamo", "some.dll" });
            lookUp.Setup(l => l.ExistsAtPath(It.IsAny<string>())).Returns(true);
            lookUp.Setup(l => l.GetCoreFilePathFromInstallation(It.IsAny<string>()))
                .Returns<string>(s => s);
            lookUp.Setup(l => l.GetInstallLocationFromProductName(It.IsAny<string>())).Returns(path);
            lookUp.Setup(l => l.GetVersionInfoFromFile(It.IsAny<string>()))
                .Returns(Tuple.Create(0, 6, 3, 5322));

            var p = lookUp.Object.GetProductFromProductName(name);
            Assert.AreEqual("0.6.3.5322", p.VersionString);
            Assert.AreEqual(path, p.InstallLocation);
            Assert.AreEqual(name, p.ProductName);
        }

        [Test, Category("ProductLookUp"), Category("UnitTests")]
        public void NoProductFromProductName()
        {
            var lookUp = new Mock<InstalledProductLookUp>(new object[] { "Dynamo", "some.dll" });
            lookUp.Setup(l => l.GetInstallLocationFromProductName(It.IsAny<string>())).Returns(string.Empty);

            var p = lookUp.Object.GetProductFromProductName("ABC");
            Assert.IsNull(p);
        }

        [Test, Category("ProductLookUp"), Category("UnitTests")]
        public void GetProductFromProductCode()
        {
            const string path = @"C:\MyDrive\Dynamo\Core";
            const string code = @"{1123-2312}";
            string name = @"My New Dynamo";
            var lookUp = new Mock<InstalledProductLookUp>(new object[] { "Dynamo", "some.dll" });
            lookUp.Setup(l => l.ExistsAtPath(It.IsAny<string>())).Returns(true);
            lookUp.Setup(l => l.GetCoreFilePathFromInstallation(It.IsAny<string>()))
                .Returns<string>(s => s);
            lookUp.Setup(
                l => l.GetInstallLocationFromProductCode(It.IsAny<string>(), out name))
                .Returns(path);
            lookUp.Setup(l => l.GetVersionInfoFromFile(It.IsAny<string>()))
                .Returns(Tuple.Create(0, 6, 3, 5322));

            var p = lookUp.Object.GetProductFromProductCode(code);
            Assert.AreEqual("0.6.3.5322", p.VersionString);
            Assert.AreEqual(path, p.InstallLocation);
            Assert.AreEqual(name, p.ProductName);
        }

        [Test, Category("ProductLookUp"), Category("UnitTests")]
        public void NoProductFromProductCode()
        {
            const string code = @"{1123-2312}";
            string name = @"My New Dynamo";
            var lookUp = new Mock<InstalledProductLookUp>(new object[] { "Dynamo", "some.dll" });
            lookUp.Setup(
                l => l.GetInstallLocationFromProductCode(It.IsAny<string>(), out name))
                .Returns<string>(null);
            
            var p = lookUp.Object.GetProductFromProductCode(code);
            Assert.IsNull(p);
        }

        [Test, Category("ProductLookUp"), Category("UnitTests")]
        public void CompareProducts()
        {
            var lookUp = new Mock<InstalledProductLookUp>(new object[] { "Dynamo", "some.dll" });
            lookUp.Setup(l => l.ExistsAtPath(It.IsAny<string>())).Returns(true);
            lookUp.Setup(l => l.GetCoreFilePathFromInstallation(It.IsAny<string>()))
                .Returns<string>(s => s);
            lookUp.Setup(l => l.GetVersionInfoFromFile(It.IsAny<string>()))
                .Returns(Tuple.Create(0, 62, 350, 5322));

            const string path = @"C:\MyDrive\Dynamo\Core";
            var p = lookUp.Object.GetProductFromInstallPath(path);
            Assert.AreEqual("Dynamo 0.62", p.ProductName);

            var p1 = new Mock<IInstalledProduct>();
            p1.Setup(x => x.VersionInfo).Returns(Tuple.Create(0, 62, 350, 4523));
            Assert.IsTrue(p.CompareTo(p1.Object) == 0); //Doesn't care the rev number.

            var p2 = new Mock<IInstalledProduct>();
            p2.Setup(x => x.VersionInfo).Returns(Tuple.Create(1, 62, 350, 4523));
            Assert.IsTrue(p.CompareTo(p2.Object) < 0);

            var p3 = new Mock<IInstalledProduct>();
            p3.Setup(x => x.VersionInfo).Returns(Tuple.Create(0, 61, 350, 4523));
            Assert.IsTrue(p.CompareTo(p3.Object) > 0);

            var p4 = new Mock<IInstalledProduct>();
            p4.Setup(x => x.VersionInfo).Returns(Tuple.Create(0, 62, 30, 4523));
            Assert.IsTrue(p.CompareTo(p4.Object) > 0);

            var p5 = new Mock<IInstalledProduct>();
            p5.Setup(x => x.VersionInfo).Returns(Tuple.Create(0, 62, 360, 4523));
            Assert.IsTrue(p.CompareTo(p5) < 0);
        }

        private static InstalledProductLookUp SetUpProductLookUp(Dictionary<string, Tuple<int,int,int,int>> products,
            Dictionary<string, Tuple<int, int, int, int>> locations)
        {
            string name;
            Tuple<int, int, int, int> version;
                        
            var lookUp = new Mock<InstalledProductLookUp>(new object[] { "XYZ", "some.dll" }) { CallBase = true };
            lookUp.Setup(l => l.ExistsAtPath(It.IsAny<string>()))
                .Returns<string>(
                    (s) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return false;

                        if (products.TryGetValue(s, out version))
                            return version != null;
                        if (locations.TryGetValue(s, out version))
                            return version != null;

                        return false;
                    });

            lookUp.Setup(l => l.GetInstallLocationFromProductName(It.IsAny<string>()))
                .Returns<string>(s => s);
            lookUp.Setup(l => l.GetProductNameList()).Returns(products.Keys);
            lookUp.Setup(l => l.GetCoreFilePathFromInstallation(It.IsAny<string>()))
                .Returns<string>(s => s);
            lookUp.Setup(l => l.GetVersionInfoFromFile(It.IsAny<string>()))
                .Returns<string>(s => products.TryGetValue(s, out version) ? version : locations[s]);
            lookUp.Setup(
                l => l.GetInstallLocationFromProductCode(It.IsAny<string>(), out name))
                .Returns<string>(null);

            return lookUp.Object;
        }
    }
}
