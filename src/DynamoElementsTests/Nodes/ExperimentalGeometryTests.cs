using System.IO;
using Dynamo.FSchemeInterop;
using NUnit.Framework;
using Dynamo.Utilities;
using Dynamo.Nodes;

namespace Dynamo.Tests
{
    [TestFixture]
    class ExperimentalGeometryTests : DynamoUnitTest
    {
        [Test]
        public void LineTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\LineTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for X coordinate of Point node.
            Autodesk.LibG.Geometry geometry = null;
            var getGeometry = model.CurrentWorkspace.NodeFromWorkspace<Point3DNode>("18dce051-d02c-4e24-b852-8626af14b2ea");
            Assert.IsTrue(Utils.Convert(getGeometry.GetValue(0), ref geometry));

            Autodesk.LibG.Point point = geometry as Autodesk.LibG.Point;
            Assert.AreNotEqual(null, point);
            Assert.AreEqual(0, point.x());

            // Verification for Y coordinate of Point node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<Point3DNode>("c231bff3-36d7-4171-bd28-21d5730ab230");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Point point1 = geometry1 as Autodesk.LibG.Point;
            Assert.AreNotEqual(null, point1);
            Assert.AreEqual(10, point1.x());

            Autodesk.LibG.Geometry line = null;
            var getGeometry2 = model.CurrentWorkspace.NodeFromWorkspace<LineNode>("6e53d1b5-d85a-47d7-a299-7bd031281363");
            Assert.IsTrue(Utils.Convert(getGeometry2.GetValue(0), ref line));

            // Verification for "Length" , "StartPoint" and "EndPoint" of Line.
            Autodesk.LibG.Line line1 = line as Autodesk.LibG.Line;
            Assert.AreEqual(0, line1.start_point().x());
            Assert.AreEqual(10, line1.end_point().x());
            Assert.AreEqual(10, line1.length());
        }

        [Test]
        public void CircleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\CircleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for X coordinate of CenterPoint node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<Point3DNode>("753356e5-4025-46b9-a688-8a2e20e482fc");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Point point1 = geometry1 as Autodesk.LibG.Point;
            Assert.AreNotEqual(null, point1);
            Assert.AreEqual(0, point1.x());

            // Verification for Circle node.
            Autodesk.LibG.Geometry geometry = null;
            var getGeometry = model.CurrentWorkspace.NodeFromWorkspace<CircleNode>("3382f192-30e7-49bf-92f0-c24504d498b5");
            Assert.IsTrue(Utils.Convert(getGeometry.GetValue(0), ref geometry));

            Autodesk.LibG.Circle circle = geometry as Autodesk.LibG.Circle;
            Assert.AreNotEqual(null, circle);
            //Assert.AreEqual(point1, circle.center_point());
            Assert.AreEqual(0, circle.center_point().x());

        }

    }
}
