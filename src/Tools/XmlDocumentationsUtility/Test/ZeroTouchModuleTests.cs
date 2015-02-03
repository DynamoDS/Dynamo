using NodeDocumentationUtility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentationsUtility.Test
{
    [TestFixture]
    class ZeroTouchModuleTests
    {
        private ZeroTouchModule module = null;

        [SetUp]
        public void SetUp()
        {
            module = new ZeroTouchModule("ProtoGeometry.dll");
        }

        [Test]
        public void TypeExists()
        {
            Assert.IsFalse(module.TypeExists("Analysis.IAnalysisData"));
            Assert.IsTrue(module.TypeExists("Autodesk.DesignScript.Geometry.Point"));
        }

        [Test]
        public void PropertyExists()
        {
            Assert.IsTrue(module.PropertyExists("Autodesk.DesignScript.Geometry.Vector", "X"));
            Assert.IsTrue(module.PropertyExists("Autodesk.DesignScript.Geometry.Surface", "ClosedInU"));
        }

        [Test]
        public void FieldExists()
        {
            Assert.IsFalse(module.PropertyExists("Autodesk.DesignScript.Geometry.Geometry", "mGeometryContructors"));
        }

        [Test]
        public void MethodExists()
        {
            Assert.IsFalse(module.MethodExists("Autodesk.DesignScript.Geometry.Geometry","RegisterHostType"));
            Assert.IsTrue(module.MethodExists("Autodesk.DesignScript.Geometry.Geometry","Translate"));
        }
    }
}
