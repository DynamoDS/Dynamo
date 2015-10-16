using System.Linq;
using Dynamo.Wpf.Rendering;
using Dynamo.Wpf.ViewModels.Watch3D;
using HelixToolkit.Wpf.SharpDX;
using NUnit.Framework;
using SharpDX;

namespace WpfVisualizationTests
{
    [TestFixture]
    public class HelixWatch3DViewModelUnitTests
    {
        private PointGeometryModel3D closePoint;
        private PointGeometryModel3D farPoint;
        private MeshGeometryModel3D cube;

        [SetUp]
        public void Setup()
        {
            closePoint = InitializePointWithId("closePoint");
            farPoint = InitializePointWithId("farPoint", new Vector3(10000, 10000, 10000));
            var builder = new MeshBuilder();
            builder.AddBox(new Vector3(-10000, -10000, -5000), 5, 5, 5, BoxFaces.All);
            var boxGeom = builder.ToMeshGeometry3D();
            cube = new MeshGeometryModel3D()
            {
                Geometry = boxGeom,
                Name = "cube"
            };
        }

        #region clip plane tests

        [Test]
        public void ComputeClipPlaneDistances_AllObjectsInFront_ClipDistance_IsCorrect()
        {
            double near, far;
            var planeOrigin = new Vector3(-50000,0,0);
            var planeNormal = new Vector3(1,0,0);
            var geometry = new GeometryModel3D[] {closePoint, farPoint, cube};
            HelixWatch3DViewModel.ComputeClipPlaneDistances(planeOrigin, planeNormal, geometry, 0.001, out near, out far, 
                HelixWatch3DViewModel.DefaultNearClipDistance, HelixWatch3DViewModel.DefaultFarClipDistance);
            Assert.Less(near, closePoint.Bounds().GetCorners().Min(c=>c.DistanceToPlane(planeOrigin, planeNormal)) );
            Assert.AreNotEqual(near, HelixWatch3DViewModel.DefaultNearClipDistance);
            Assert.Greater(far, farPoint.Bounds().GetCorners().Max(c=>c.DistanceToPlane(planeOrigin, planeNormal)));
        }

        [Test]
        public void ComputeClipPlaneDistances_AllObjectsBehind__ClipDistances_AreDefault()
        {
            double near, far;
            var planeOrigin = new Vector3(50000, 0, 0);
            var planeNormal = new Vector3(1, 0, 0);
            var geometry = new GeometryModel3D[] { closePoint, farPoint, cube };
            HelixWatch3DViewModel.ComputeClipPlaneDistances(planeOrigin, planeNormal, geometry, 0.001, out near, out far,
                HelixWatch3DViewModel.DefaultNearClipDistance, HelixWatch3DViewModel.DefaultFarClipDistance);
            Assert.AreEqual(near, HelixWatch3DViewModel.DefaultNearClipDistance);
            Assert.AreEqual(far, HelixWatch3DViewModel.DefaultFarClipDistance);
        }

        [Test]
        public void ComputeClipPlaneDistances_ObjectsInFrontAndBehind_ClipDistance_IsCorrect()
        {
            double near, far;
            var planeOrigin = new Vector3(-10, 0, 0);
            var planeNormal = new Vector3(1, 0, 0);
            var geometry = new GeometryModel3D[] { closePoint, farPoint, cube };
            HelixWatch3DViewModel.ComputeClipPlaneDistances(planeOrigin, planeNormal, geometry, 0.001, out near, out far,
                HelixWatch3DViewModel.DefaultNearClipDistance, HelixWatch3DViewModel.DefaultFarClipDistance);
            Assert.Less(near, closePoint.Bounds().GetCorners().Min(c => c.DistanceToPlane(planeOrigin, planeNormal)));
            Assert.Greater(far, farPoint.Bounds().GetCorners().Max(c => c.DistanceToPlane(planeOrigin, planeNormal)));
        }

        [Test]
        public void ComputeClipPlaneDistances_ClosestObjectCoincidentWithCamera_NearClipDistance_IsDefault()
        {
            double near, far;
            var planeOrigin = farPoint.Bounds().Minimum;
            var planeNormal = new Vector3(1, 0, 0);
            var geometry = new GeometryModel3D[] { closePoint, farPoint, cube };
            HelixWatch3DViewModel.ComputeClipPlaneDistances(planeOrigin, planeNormal, geometry, 0.001, out near, out far,
                HelixWatch3DViewModel.DefaultNearClipDistance, HelixWatch3DViewModel.DefaultFarClipDistance);
            Assert.AreEqual(near, HelixWatch3DViewModel.DefaultNearClipDistance);
        }

        #endregion

        #region zoom to fit tests

        [Test]
        public void ComputeBoundsForSelectedNodeGeometry_NothingSelected_BoundingBoxAroundAllGeometry()
        {
            var bounds = HelixWatch3DViewModel.ComputeBoundsForGeometry(new GeometryModel3D[] { });
            Assert.AreEqual(bounds, HelixWatch3DViewModel.DefaultBounds);
        }

        [Test]
        public void ComputeBoundsForSelectedNodeGeometry_PointNodeSelected_BoundingBoxIsAroundPoint()
        {
            var pointA = InitializePointWithId("A");
            var bounds = HelixWatch3DViewModel.ComputeBoundsForGeometry(new GeometryModel3D[] {pointA});
            Assert.AreEqual(bounds, pointA.Bounds());
        }

        [Test]
        public void ComputeBoundsForSelectedNodeGeometry_MultiplePointNodesSelected_BoundingBoxIsCorrect()
        {
            var pointA = InitializePointWithId("A");
            var pointB = InitializePointWithId("B", new Vector3(1000000, 1000000, 1000000));
            var bounds = HelixWatch3DViewModel.ComputeBoundsForGeometry(new GeometryModel3D[] { pointA, pointB });
            Assert.AreEqual(bounds.Minimum, pointA.Bounds().Minimum);
            Assert.AreEqual(bounds.Maximum, pointB.Bounds().Maximum);
        }

        [Test]
        public void ComputeBoundsForSelectedNodeGeometry_OnePointOneCubeSelected_BoundingBoxIsCorrect()
        {
            var pointA = InitializePointWithId("A");

            var builder = new MeshBuilder();
            builder.AddBox(new Vector3(1000000, 1000000, 1000000), 5, 5, 5, BoxFaces.All);
            var boxGeom = builder.ToMeshGeometry3D();
            var boxB = new MeshGeometryModel3D()
            {
                Geometry = boxGeom,
                Name = "B"
            };

            var bounds = HelixWatch3DViewModel.ComputeBoundsForGeometry(new GeometryModel3D[] { pointA, boxB });
            Assert.AreEqual(bounds.Minimum, pointA.Bounds().Minimum);
            Assert.AreEqual(bounds.Maximum, boxB.Bounds().Maximum);
        }

        private PointGeometryModel3D InitializePointWithId(string id, Vector3 location = new Vector3())
        {
            var pointGeom = HelixRenderPackage.InitPointGeometry();
            pointGeom.Positions.Add(location);
            pointGeom.Colors.Add(new Color4());
            pointGeom.Indices.Add(0);
            var point = new PointGeometryModel3D
            {
                Geometry = pointGeom,
                Name = id
            };
            return point;
        }

        private void AddGeometryToExistingPoint(PointGeometryModel3D point, Vector3 location)
        {
            if (point == null || point.Geometry == null) return;

            point.Geometry.Positions.Add(location);
            point.Geometry.Colors.Add(new Color4());
            point.Geometry.Indices.Add(point.Geometry.Indices.Any()? point.Geometry.Indices.Last() + 1: 0);
        }

        #endregion
    }
}
