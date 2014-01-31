using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoTest.DebugTests;
using ProtoTestFx;
namespace ProtoTest
{
    [TestFixture]
    class GeometryTest
    {
        [SetUp]
        public void SetUp()
        {
            GeometryTestFrame.ScriptDir = @"..\..\..\Scripts\Geometry\";
            GeometryTestFrame.BaseDir = @"..\..\..\Scripts\Geometry\Base\";
        }

        [Test]
        public void PointArrayCreation()
        {
            String filePath = @"..\..\..\Scripts\Geometry\point_creation_array.ds";
            GeometryTestFrame.RunAndCompare(filePath);
        }

        [Test]
        public void LineCreation()
        {
            String filePath = @"..\..\..\Scripts\Geometry\line_creation.ds";
            GeometryTestFrame.RunAndCompare(filePath);
        }
    }
}
