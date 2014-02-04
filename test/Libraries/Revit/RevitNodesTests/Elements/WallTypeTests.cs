using System;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{

    [TestFixture]
    public class WallTypeTests
    {

        [Test]
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

