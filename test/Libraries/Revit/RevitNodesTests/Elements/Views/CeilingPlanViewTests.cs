using System;
using Dynamo.Tests;
using Revit.Elements;
using Revit.Elements.Views;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    class CeilingPlanViewTests
    {
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
