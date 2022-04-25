using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using NUnit.Framework;

namespace Dynamo.Tests.Visualization
{
    [TestFixture]
    class RenderPackageCacheTest
    {
        /// <summary>
        /// This test method will execute the next methods from RenderPackageCache
        /// public RenderPackageCache(IEnumerable<IRenderPackage> otherPackages)
        /// public RenderPackageCache GetPortPackages(Guid portId)
        /// public void Add(RenderPackageCache other)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void RenderPackageCache_Constructor()
        {
            //Arrange
            var renderingFactory = new DefaultRenderPackageFactory();
            List<IRenderPackage> packages = new List<IRenderPackage>();
            packages.Add(renderingFactory.CreateRenderPackage());
            Guid gPort1 = Guid.NewGuid();

            //Act
            var renderPackage = new RenderPackageCache(packages);
            var renderPackage2 = new RenderPackageCache();

            //Assert
            //This will return null because the portMap is null
            Assert.IsNull(renderPackage.GetPortPackages(Guid.NewGuid()));

            renderPackage.Add(renderingFactory.CreateRenderPackage(), gPort1);

            //In renderPackage2 we are adding again the RenderPackageCache with the same prot
            renderPackage2.Add(renderingFactory.CreateRenderPackage(), gPort1);

            //The Add method will raise an ArgumentException since we already added the port
            Assert.Throws<ArgumentException>( () => renderPackage2.Add(renderPackage));

            //This will return null because the guid was not found in the portMap dictionary
            Assert.IsNull(renderPackage.GetPortPackages(Guid.NewGuid()));
        }
    }
}
