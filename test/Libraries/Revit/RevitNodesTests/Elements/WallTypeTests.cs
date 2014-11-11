using System;
using Revit.Elements;
using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{

    [TestFixture]
    public class WallTypeTests : RevitNodeTestBase
    {

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByName_ValidArgs()
        {
            var wallTypeName = "Curtain Wall 1";
            var wallType = WallType.ByName(wallTypeName);
            Assert.NotNull(wallType);
            Assert.AreEqual(wallTypeName, wallType.Name);
        }

        [Test]
        public void ByName_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => WallType.ByName(null));
        }

    }

}

