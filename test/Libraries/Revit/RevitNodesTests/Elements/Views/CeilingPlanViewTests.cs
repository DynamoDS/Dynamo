using System;
using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using Revit.Elements.Views;
using NUnit.Framework;
using RTF.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    class CeilingPlanViewTests : RevitNodeTestBase
    {
        [SetUp]
        public override void Setup()
        {
            HostFactory.Instance.StartUp();
            base.Setup();
        }

        [TearDown]
        public override void TearDown()
        {
            HostFactory.Instance.ShutDown();
            base.TearDown();
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByLevel_ValidArgs()
        {
            var elevation = 100;
            var level = Level.ByElevation(elevation);
            Assert.NotNull(level);

            var view = CeilingPlanView.ByLevel(level);

            Assert.NotNull(view);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByLevel_BadArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => CeilingPlanView.ByLevel(null));
        }
    }
}
