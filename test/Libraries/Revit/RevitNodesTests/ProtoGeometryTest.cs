using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    class ProtoGeometryTest
    {
        [SetUp]
        public void Setup()
        {
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public void TearDown()
        {
            HostFactory.Instance.ShutDown();
        }

    }
}
