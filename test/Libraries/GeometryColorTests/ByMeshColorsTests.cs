using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using DSCore;
using Dynamo.Visualization;
using NUnit.Framework;
using TestServices;

namespace DisplayTests
{
    [TestFixture]
    public class ByMeshColorsTests : GeometricTestBase
    {
        [Test]
        public void ByMeshColors_Contruction_OneColor_AllGood()
        {
            Assert.DoesNotThrow(() => Modifiers.GeometryColor.ByMeshColors(
                CreateOneMeshWithQuads(), CreateListOfColors(1)));

            Assert.DoesNotThrow(() => Modifiers.GeometryColor.ByMeshColors(
                CreateOneMeshWithNoQuads(), CreateListOfColors(1)));
        }

        [Test]
        public void ByMeshColors_Contruction_ColorByFace_AllGood()
        {

            Assert.DoesNotThrow(() => Modifiers.GeometryColor.ByMeshColors(
                CreateOneMeshWithQuads(), CreateListOfColors(3)));

            Assert.DoesNotThrow(() => Modifiers.GeometryColor.ByMeshColors(
                CreateOneMeshWithNoQuads(), CreateListOfColors(3)));
        }

        [Test]
        public void ByMeshColors_Contruction_ColorByVertex_AllGood()
        {
            //Test the color by vertex count constructor
            Assert.DoesNotThrow(() => Modifiers.GeometryColor.ByMeshColors(
                CreateOneMeshWithQuads(), CreateListOfColors(7)));

            Assert.DoesNotThrow(() => Modifiers.GeometryColor.ByMeshColors(
               CreateOneMeshWithNoQuads(), CreateListOfColors(5)));
        }

        [Test]
        public void ByMeshColors_Contruction_ColorByPerTriangleVertex_AllGood()
        {
            //Test the color by vertex count constructor
            Assert.DoesNotThrow(() => Modifiers.GeometryColor.ByMeshColors(
                CreateOneMeshWithQuads(), CreateListOfColors(15)));

            Assert.DoesNotThrow(() => Modifiers.GeometryColor.ByMeshColors(
               CreateOneMeshWithNoQuads(), CreateListOfColors(9)));
        }

        [Test]
        public void ByMeshColors_Construction_QuadMesh_OneColor_CreatesCorrectNumberTriangles()
        {
            var colors = CreateListOfColors(1);
            var displayMesh = Modifiers.GeometryColor.ByMeshColors(CreateOneMeshWithQuads(), colors);
            var factory = new DefaultRenderPackageFactory();
            var package = factory.CreateRenderPackage();
            displayMesh.Tessellate(package, new TessellationParameters());
            Assert.AreEqual(15, package.MeshVertexCount);
            var vertexColorList = package.MeshVertexColors.ToArray();
            var renderPackageColorList = new List<Color>();
            for(var i = 0; i < 60; i+=4)
            {
                var c = Color.ByARGB(vertexColorList[i + 3], vertexColorList[i], vertexColorList[i + 1], vertexColorList[i + 2]);
                renderPackageColorList.Add(c); 
            }

            var color = colors[0];
            foreach(var c in renderPackageColorList)
            {
                Assert.AreEqual(color, c);
            }

            Assert.AreEqual(0, package.LineVertexCount);
            Assert.AreEqual(0, package.PointVertexCount);
        }

        [Test]
        public void ByMeshColors_Construction_NonQaudMesh_OneColor_CreatesCorrectNumberTriangles()
        {
            var colors = CreateListOfColors(1);
            var displayMesh = Modifiers.GeometryColor.ByMeshColors(CreateOneMeshWithNoQuads(), colors);
            var factory = new DefaultRenderPackageFactory();
            var package = factory.CreateRenderPackage();
            displayMesh.Tessellate(package, new TessellationParameters());
            Assert.AreEqual(9, package.MeshVertexCount);
            var vertexColorList = package.MeshVertexColors.ToArray();
            var renderPackageColorList = new List<Color>();

            for (var i = 0; i < 36; i += 4)
            {
                var c = Color.ByARGB(vertexColorList[i + 3], vertexColorList[i], vertexColorList[i + 1], vertexColorList[i + 2]);
                renderPackageColorList.Add(c);
            }

            var color = colors[0];
            foreach (var c in renderPackageColorList)
            {
                Assert.AreEqual(color, c);
            }

            Assert.AreEqual(0, package.LineVertexCount);
            Assert.AreEqual(0, package.PointVertexCount);
        }

        [Test]
        public void ByMeshColors_Construction_QuadMesh_FaceColor_CreatesCorrectNumberTriangles()
        {
            var colors = CreateListOfColors(3);
            var displayMesh = Modifiers.GeometryColor.ByMeshColors(CreateOneMeshWithQuads(), colors);
            var factory = new DefaultRenderPackageFactory();
            var package = factory.CreateRenderPackage();
            displayMesh.Tessellate(package, new TessellationParameters());
            Assert.AreEqual(15, package.MeshVertexCount);

            var vertexColorList = package.MeshVertexColors.ToArray();
            var renderPackageColorList = new List<Color>();
            for (var i = 0; i < 60; i += 4)
            {
                var c = Color.ByARGB(vertexColorList[i + 3], vertexColorList[i], vertexColorList[i + 1], vertexColorList[i + 2]);
                renderPackageColorList.Add(c);
            }

            renderPackageColorList.Reverse();
            var renderPacakgeColorStack = new Stack<Color>(renderPackageColorList);

            var color = colors[0];
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(color, renderPacakgeColorStack.Pop());
            }

            color = colors[1];
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(color, renderPacakgeColorStack.Pop());
            }

            color = colors[2];
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(color, renderPacakgeColorStack.Pop());
            }

            Assert.AreEqual(0, package.LineVertexCount);
            Assert.AreEqual(0, package.PointVertexCount);
        }

        [Test]
        public void ByMeshColors_Construction_NonQuadMesh_FaceColor_CreatesCorrectNumberTriangles()
        {
            var colors = CreateListOfColors(3);
            var displayMesh = Modifiers.GeometryColor.ByMeshColors(CreateOneMeshWithNoQuads(), colors);
            var factory = new DefaultRenderPackageFactory();
            var package = factory.CreateRenderPackage();
            displayMesh.Tessellate(package, new TessellationParameters());
            Assert.AreEqual(9, package.MeshVertexCount);

            var vertexColorList = package.MeshVertexColors.ToArray();
            var renderPackageColorList = new List<Color>();
            for (var i = 0; i < 36; i += 4)
            {
                var c = Color.ByARGB(vertexColorList[i + 3], vertexColorList[i], vertexColorList[i + 1], vertexColorList[i + 2]);
                renderPackageColorList.Add(c);
            }

            renderPackageColorList.Reverse();
            var renderPacakgeColorStack = new Stack<Color>(renderPackageColorList);

            var color = colors[0];
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(color, renderPacakgeColorStack.Pop());
            }

            color = colors[1];
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(color, renderPacakgeColorStack.Pop());
            }

            color = colors[2];
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(color, renderPacakgeColorStack.Pop());
            }

            Assert.AreEqual(0, package.LineVertexCount);
            Assert.AreEqual(0, package.PointVertexCount);
        }

        [Test]
        public void ByMeshColors_Construction_QuadMesh_PerTriangleVertexColor_CreatesCorrectNumberTriangles()
        {
            var colors = CreateListOfColors(15);
            var displayMesh = Modifiers.GeometryColor.ByMeshColors(CreateOneMeshWithQuads(), colors);
            var factory = new DefaultRenderPackageFactory();
            var package = factory.CreateRenderPackage();
            displayMesh.Tessellate(package, new TessellationParameters());
            Assert.AreEqual(15, package.MeshVertexCount);

            var vertexColorList = package.MeshVertexColors.ToArray();
            var renderPackageColorList = new List<Color>();
            for (var i = 0; i < 60; i += 4)
            {
                var c = Color.ByARGB(vertexColorList[i + 3], vertexColorList[i], vertexColorList[i + 1], vertexColorList[i + 2]);
                renderPackageColorList.Add(c);
            }

            for (var i = 0; i < 15; i++)
            {
                Assert.AreEqual(colors[i], renderPackageColorList[i]);
            }

            Assert.AreEqual(0, package.LineVertexCount);
            Assert.AreEqual(0, package.PointVertexCount);
        }

        [Test]
        public void ByMeshColors_Construction_NonQuadMesh_PerTriangleVertexColor_CreatesCorrectNumberTriangles()
        {
            var colors = CreateListOfColors(9);
            var displayMesh = Modifiers.GeometryColor.ByMeshColors(CreateOneMeshWithNoQuads(), colors);
            var factory = new DefaultRenderPackageFactory();
            var package = factory.CreateRenderPackage();
            displayMesh.Tessellate(package, new TessellationParameters());
            Assert.AreEqual(9, package.MeshVertexCount);

            var vertexColorList = package.MeshVertexColors.ToArray();
            var renderPackageColorList = new List<Color>();
            for (var i = 0; i < 36; i += 4)
            {
                var c = Color.ByARGB(vertexColorList[i + 3], vertexColorList[i], vertexColorList[i + 1], vertexColorList[i + 2]);
                renderPackageColorList.Add(c);
            }

            for (var i = 0; i < 9; i++)
            {
                Assert.AreEqual(colors[i], renderPackageColorList[i]);
            }

            Assert.AreEqual(0, package.LineVertexCount);
            Assert.AreEqual(0, package.PointVertexCount);
        }

        [Test]
        public void ByMeshColors_Construction_NullMesh_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Modifiers.GeometryColor.ByMeshColors(
                null, CreateListOfColors(1)));
        }

        [Test]
        public void ByMeshColors_Construction_NullColors_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Modifiers.GeometryColor.ByMeshColors(
                CreateOneMeshWithQuads(), null));
        }

        [Test]
        public void ByMeshColors_Construction_NoColors_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Modifiers.GeometryColor.ByMeshColors(
                CreateOneMeshWithQuads(), new Color[]{}));
        }

        [Test]
        public void ByMeshColors_Construction_WrongNumberOfColors_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Modifiers.GeometryColor.ByMeshColors(
                CreateOneMeshWithQuads(), CreateListOfColors(2)));

            Assert.Throws<ArgumentException>(() => Modifiers.GeometryColor.ByMeshColors(
               CreateOneMeshWithNoQuads(), CreateListOfColors(2)));
        }

        private static Mesh CreateOneMeshWithNoQuads()
        {
            var pt1 = Point.ByCoordinates(3, 2, 0);
            var pt2 = Point.ByCoordinates(1, 3, 0);
            var pt3 = Point.ByCoordinates(1, 1, 0);
            var pt4 = Point.ByCoordinates(2, 0, 0);
            var pt5 = Point.ByCoordinates(4, 0, 0);
            var pt6 = Point.ByCoordinates(4, 2, 0);
            var pt7 = Point.ByCoordinates(6, 0, 0);
            var pts = new List<Point>() { pt1, pt2, pt3, pt4, pt5 };

            var ig1 = IndexGroup.ByIndices(0, 1, 2);
            var ig2 = IndexGroup.ByIndices(3, 0, 2);
            var ig3 = IndexGroup.ByIndices(3, 4, 0);

            var igs = new List<IndexGroup>() { ig1, ig2, ig3 };

            var mesh = Mesh.ByPointsFaceIndices(pts, igs);
            return mesh;
        }

        private static Mesh CreateOneMeshWithQuads()
        {
            var pt1 = Point.ByCoordinates(3, 2, 0);
            var pt2 = Point.ByCoordinates(1, 3, 0);
            var pt3 = Point.ByCoordinates(1, 1, 0);
            var pt4 = Point.ByCoordinates(2, 0, 0);
            var pt5 = Point.ByCoordinates(4, 0, 0);
            var pt6 = Point.ByCoordinates(4, 2, 0);
            var pt7 = Point.ByCoordinates(6, 0, 0);
            var pts = new List<Point>() { pt1, pt2, pt3, pt4, pt5, pt6, pt7 };

            var ig1 = IndexGroup.ByIndices(0, 1, 2, 3);
            var ig2 = IndexGroup.ByIndices(0, 5, 4, 3);
            var ig3 = IndexGroup.ByIndices(4, 5, 6);

            var igs = new List<IndexGroup>() { ig1, ig2, ig3 };

            var mesh = Mesh.ByPointsFaceIndices(pts, igs);
            return mesh;
        }

        private static Color[] CreateListOfColors(int number)
        {
            var colors = new List<Color>();

            for (int i = 0; i < number; i++)
            {
                colors.Add(Color.ByARGB(255, 255, i, 0));
            };

            return colors.ToArray();
        }

    }
}
