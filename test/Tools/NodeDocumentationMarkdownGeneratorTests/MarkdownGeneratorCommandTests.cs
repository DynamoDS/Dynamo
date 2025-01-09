using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NodeDocumentationMarkdownGenerator;
using NodeDocumentationMarkdownGenerator.Commands;
using NodeDocumentationMarkdownGenerator.Verbs;
using NUnit.Framework;

namespace NodeDocumentationMarkdownGeneratorTests
{
    [TestFixture]
    public class MarkdownGeneratorCommandTests
    {
        private const string CORENODEMODELS_DLL_NAME = "CoreNodeModels.dll";
        private const string LibraryViewExtension_DLL_NAME = "LibraryViewExtensionWebView2.dll";
        private static readonly string DynamoCoreDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static readonly string DynamoCoreNodesDir = Path.Combine(DynamoCoreDir, "Nodes");
        private static string DynamoRepoRoot = new DirectoryInfo(DynamoCoreDir).Parent.Parent.Parent.FullName;
        private static readonly string NodeGeneratorToolBuildPath = Path.Combine(DynamoRepoRoot, "src","tools", "NodeDocumentationMarkdownGenerator","bin",
    "AnyCPU"
            );
        private static readonly string toolsTestFilesDirectory = Path.GetFullPath(Path.Combine(DynamoRepoRoot, "test","Tools", "docGeneratorTestFiles"));
        private static readonly string testLayoutSpecPath = Path.Combine(toolsTestFilesDirectory, "testlayoutspec.json");
        private static readonly string mockedDictionaryRoot = Path.Combine(toolsTestFilesDirectory, "sampledictionarycontent");
        private static readonly string mockedDictionaryJson = Path.Combine(mockedDictionaryRoot, "Dynamo_Nodes_Documentation.json");

        private static readonly List<string> preloadedLibraryPaths = new List<string>
            {
                "VMDataBridge.dll",
                "ProtoGeometry.dll",
                "DesignScriptBuiltin.dll",
                "DSCoreNodes.dll",
                "DSOffice.dll",
                "DSCPython.dll",
                "FunctionObject.ds",
                "BuiltIn.ds",
                "DynamoConversions.dll",
                "DynamoUnits.dll",
                "Tessellation.dll",
                "Analysis.dll",
                "GeometryColor.dll"
            };

        private DirectoryInfo tempDirectory = null;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                var libviewExtensionAssem = Assembly.LoadFrom(Path.Combine(DynamoCoreDir, LibraryViewExtension_DLL_NAME));
                SaveCoreLayoutSpecToPath(libviewExtensionAssem, testLayoutSpecPath);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            if (File.Exists(testLayoutSpecPath))
            {
                File.Delete(testLayoutSpecPath);
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //resovle assemblies from the tool's bin folder - nmgt is not copied to dynamo bin.
            var requestedAssembly = new AssemblyName(args.Name);
            var masks = new[] { "*.dll", "*.exe" };
            var files = masks.SelectMany(x=> new DirectoryInfo(NodeGeneratorToolBuildPath).EnumerateFiles(x, SearchOption.AllDirectories));
            var found = files.Where(f => Path.GetFileNameWithoutExtension(f.FullName) == requestedAssembly.Name).FirstOrDefault();
            if (found != null)
            {
                return Assembly.LoadFrom(found.FullName);
            }
            return null;
        }

        [SetUp]
        public void SetUp()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Program.CurrentDomain_AssemblyResolve;
        }

        [TearDown]
        public void CleanUp()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= Program.CurrentDomain_AssemblyResolve;

            if (tempDirectory == null || !tempDirectory.Exists) return;

            tempDirectory.Delete(true);
            tempDirectory = null;
        }

        [Test]
        public void ProducesCorrectOutputFromDirectory()
        {
            // Test output is generated with the following args:
            // 
            // NodeDocumentationMarkdownGenerator.exe
            // fromdirectory
            // -i "..\Dynamo\bin\nodes"
            // -o "..\Dynamo\test\Tools\docGeneratorTestFiles\TestMdOutput_CoreNodeModels"
            // -f "CoreNodeModels.dll"

            // Arrange
            var testOutputDirName = "TestMdOutput_CoreNodeModels";
            var expectedOutputDirectory = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, testOutputDirName));
            Assert.That(expectedOutputDirectory.Exists);

            var coreNodeModelsDll = Path.Combine(DynamoCoreNodesDir, CORENODEMODELS_DLL_NAME);
            Assert.That(File.Exists(coreNodeModelsDll));

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreNodesDir,
                OutputFolderPath = tempDirectory.FullName,
                Filter = new List<string> { CORENODEMODELS_DLL_NAME },
                ReferencePaths = new List<string>()
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            var generatedFileNames = tempDirectory.GetFiles().Select(x => x.Name);

            // Assert file names are correct.
            CollectionAssert.AreEquivalent(expectedOutputDirectory.GetFiles().Select(x => x.Name), generatedFileNames);
        }

        [Test]
        public void ProducesCorrectOutputFromCoreDirectory_preloadedbinaries()
        {
            // Arrange
            var testOutputDirName = "TestMdOutput_CoreNodeModels";
            var expectedMDList = new List<string>() {"Analysis.Label.ByPointAndString.md", "Autodesk.DesignScript.Geometry.Arc.ByBestFitThroughPoints.md", "Autodesk.DesignScript.Geometry.Arc.ByCenterPointRadiusAngle.md", "Autodesk.DesignScript.Geometry.Arc.ByCenterPointStartPointEndPoint.md", "Autodesk.DesignScript.Geometry.Arc.ByCenterPointStartPointSweepAngle.md", "Autodesk.DesignScript.Geometry.Arc.ByFillet.md", "Autodesk.DesignScript.Geometry.Arc.ByFilletTangentToCurve.md", "Autodesk.DesignScript.Geometry.Arc.ByStartEndAndTangencies.md", "Autodesk.DesignScript.Geometry.Arc.ByStartPointEndPointStartTangent.md", "Autodesk.DesignScript.Geometry.Arc.ByThreePoints.md", "Autodesk.DesignScript.Geometry.Arc.CenterPoint.md", "Autodesk.DesignScript.Geometry.Arc.Radius.md", "Autodesk.DesignScript.Geometry.Arc.StartAngle.md", "Autodesk.DesignScript.Geometry.Arc.SweepAngle.md", "Autodesk.DesignScript.Geometry.BoundingBox.ByCorners.md", "Autodesk.DesignScript.Geometry.BoundingBox.ByGeometry.md", "Autodesk.DesignScript.Geometry.BoundingBox.ByMinimumVolume.md", "Autodesk.DesignScript.Geometry.BoundingBox.Contains.md", "Autodesk.DesignScript.Geometry.BoundingBox.ContextCoordinateSystem.md", "Autodesk.DesignScript.Geometry.BoundingBox.Intersection.md", "Autodesk.DesignScript.Geometry.BoundingBox.Intersects.md", "Autodesk.DesignScript.Geometry.BoundingBox.IsEmpty.md", "Autodesk.DesignScript.Geometry.BoundingBox.MaxPoint.md", "Autodesk.DesignScript.Geometry.BoundingBox.MinPoint.md", "Autodesk.DesignScript.Geometry.BoundingBox.ToCuboid.md", "Autodesk.DesignScript.Geometry.BoundingBox.ToPolySurface.md", "Autodesk.DesignScript.Geometry.Circle.ByBestFitThroughPoints.md", "Autodesk.DesignScript.Geometry.Circle.ByCenterPointRadius.md", "Autodesk.DesignScript.Geometry.Circle.ByCenterPointRadiusNormal.md", "Autodesk.DesignScript.Geometry.Circle.ByPlaneRadius.md", "Autodesk.DesignScript.Geometry.Circle.ByThreePoints.md", "Autodesk.DesignScript.Geometry.Circle.CenterPoint.md", "Autodesk.DesignScript.Geometry.Circle.Radius.md", "Autodesk.DesignScript.Geometry.Cone.ByCoordinateSystemHeightRadii.md", "Autodesk.DesignScript.Geometry.Cone.ByCoordinateSystemHeightRadius.md", "Autodesk.DesignScript.Geometry.Cone.ByPointsRadii.md", "Autodesk.DesignScript.Geometry.Cone.ByPointsRadius.md", "Autodesk.DesignScript.Geometry.Cone.EndPoint.md", "Autodesk.DesignScript.Geometry.Cone.EndRadius.md", "Autodesk.DesignScript.Geometry.Cone.Height.md", "Autodesk.DesignScript.Geometry.Cone.RadiusRatio.md", "Autodesk.DesignScript.Geometry.Cone.StartPoint.md", "Autodesk.DesignScript.Geometry.Cone.StartRadius.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.ByCylindricalCoordinates.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.ByOrigin(origin).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.ByOrigin(x, y).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.ByOrigin(x, y, z).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.ByOriginVectors(origin, xAxis, yAxis).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.ByOriginVectors(origin, xAxis, yAxis, zAxis).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.ByPlane.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.BySphericalCoordinates.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Determinant.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Identity.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Inverse.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.IsEqualTo.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.IsScaledOrtho.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.IsSingular.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.IsUniscaledOrtho.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Mirror.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Origin.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.PostMultiplyBy.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.PreMultiplyBy.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Rotate(origin, axis, degrees).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Rotate(plane, degrees).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Scale(amount).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Scale(basePoint, from, to).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Scale(plane, xamount, yamount, zamount).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Scale(xamount, yamount, zamount).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Scale1D.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Scale2D.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.ScaleFactor.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Transform(coordinateSystem).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Transform(fromCoordinateSystem, contextCoordinateSystem).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Translate(direction).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Translate(direction, distance).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.Translate(xTranslation, yTranslation, zTranslation).md", "Autodesk.DesignScript.Geometry.CoordinateSystem.XAxis.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.XScaleFactor.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.XYPlane.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.YAxis.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.YScaleFactor.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.YZPlane.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.ZAxis.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.ZScaleFactor.md", "Autodesk.DesignScript.Geometry.CoordinateSystem.ZXPlane.md", "Autodesk.DesignScript.Geometry.Cuboid.ByCorners.md", "Autodesk.DesignScript.Geometry.Cuboid.ByLengths(coordinateSystem, width, length, height).md", "Autodesk.DesignScript.Geometry.Cuboid.ByLengths(origin, width, length, height).md", "Autodesk.DesignScript.Geometry.Cuboid.ByLengths(width, length, height).md", "Autodesk.DesignScript.Geometry.Cuboid.Height.md", "Autodesk.DesignScript.Geometry.Cuboid.Length.md", "Autodesk.DesignScript.Geometry.Cuboid.Width.md", "Autodesk.DesignScript.Geometry.Curve.ApproximateWithArcAndLineSegments.md", "Autodesk.DesignScript.Geometry.Curve.ByBlendBetweenCurves.md", "Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface.md", "Autodesk.DesignScript.Geometry.Curve.ByParameterLineOnSurface.md", "Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtParameter.md", "Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtSegmentLength.md", "Autodesk.DesignScript.Geometry.Curve.EndParameter.md", "Autodesk.DesignScript.Geometry.Curve.EndPoint.md", "Autodesk.DesignScript.Geometry.Curve.Extend.md", "Autodesk.DesignScript.Geometry.Curve.ExtendEnd.md", "Autodesk.DesignScript.Geometry.Curve.ExtendStart.md", "Autodesk.DesignScript.Geometry.Curve.Extrude(direction).md", "Autodesk.DesignScript.Geometry.Curve.Extrude(direction, distance).md", "Autodesk.DesignScript.Geometry.Curve.Extrude(distance).md", "Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(direction).md", "Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(direction, distance).md", "Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(distance).md", "Autodesk.DesignScript.Geometry.Curve.HorizontalFrameAtParameter.md", "Autodesk.DesignScript.Geometry.Curve.IsClosed.md", "Autodesk.DesignScript.Geometry.Curve.IsPlanar.md", "Autodesk.DesignScript.Geometry.Curve.Join.md", "Autodesk.DesignScript.Geometry.Curve.Length.md", "Autodesk.DesignScript.Geometry.Curve.Normal.md", "Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(param).md", "Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(param, side).md", "Autodesk.DesignScript.Geometry.Curve.OffsetMany.md", "Autodesk.DesignScript.Geometry.Curve.ParameterAtChordLength.md", "Autodesk.DesignScript.Geometry.Curve.ParameterAtPoint.md", "Autodesk.DesignScript.Geometry.Curve.ParameterAtSegmentLength.md", "Autodesk.DesignScript.Geometry.Curve.Patch.md", "Autodesk.DesignScript.Geometry.Curve.PlaneAtParameter.md", "Autodesk.DesignScript.Geometry.Curve.PlaneAtSegmentLength.md", "Autodesk.DesignScript.Geometry.Curve.PointAtChordLength.md", "Autodesk.DesignScript.Geometry.Curve.PointAtParameter.md", "Autodesk.DesignScript.Geometry.Curve.PointAtSegmentLength.md", "Autodesk.DesignScript.Geometry.Curve.PointsAtChordLengthFromPoint.md", "Autodesk.DesignScript.Geometry.Curve.PointsAtEqualChordLength.md", "Autodesk.DesignScript.Geometry.Curve.PointsAtEqualSegmentLength.md", "Autodesk.DesignScript.Geometry.Curve.PointsAtSegmentLengthFromPoint.md", "Autodesk.DesignScript.Geometry.Curve.Project.md", "Autodesk.DesignScript.Geometry.Curve.PullOntoPlane.md", "Autodesk.DesignScript.Geometry.Curve.PullOntoSurface.md", "Autodesk.DesignScript.Geometry.Curve.Reverse.md", "Autodesk.DesignScript.Geometry.Curve.SegmentLengthAtParameter.md", "Autodesk.DesignScript.Geometry.Curve.SegmentLengthBetweenParameters.md", "Autodesk.DesignScript.Geometry.Curve.Simplify.md", "Autodesk.DesignScript.Geometry.Curve.SplitByParameter.md", "Autodesk.DesignScript.Geometry.Curve.SplitByPoints.md", "Autodesk.DesignScript.Geometry.Curve.StartParameter.md", "Autodesk.DesignScript.Geometry.Curve.StartPoint.md", "Autodesk.DesignScript.Geometry.Curve.SweepAsSolid.md", "Autodesk.DesignScript.Geometry.Curve.SweepAsSurface.md", "Autodesk.DesignScript.Geometry.Curve.TangentAtParameter.md", "Autodesk.DesignScript.Geometry.Curve.ToNurbsCurve.md", "Autodesk.DesignScript.Geometry.Curve.TrimByEndParameter.md", "Autodesk.DesignScript.Geometry.Curve.TrimByParameter.md", "Autodesk.DesignScript.Geometry.Curve.TrimByStartParameter.md", "Autodesk.DesignScript.Geometry.Curve.TrimInteriorByParameter.md", "Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter.md", "Autodesk.DesignScript.Geometry.Cylinder.Axis.md", "Autodesk.DesignScript.Geometry.Cylinder.ByPointsRadius.md", "Autodesk.DesignScript.Geometry.Cylinder.ByRadiusHeight.md", "Autodesk.DesignScript.Geometry.Cylinder.Height.md", "Autodesk.DesignScript.Geometry.Cylinder.Radius.md", "Autodesk.DesignScript.Geometry.DebugTools.DisableAsmJournaling.md", "Autodesk.DesignScript.Geometry.DebugTools.EnableAsmJournaling.md", "Autodesk.DesignScript.Geometry.Edge.AdjacentFaces.md", "Autodesk.DesignScript.Geometry.Edge.CurveGeometry.md", "Autodesk.DesignScript.Geometry.Edge.EndVertex.md", "Autodesk.DesignScript.Geometry.Edge.StartVertex.md", "Autodesk.DesignScript.Geometry.Ellipse.ByCoordinateSystemRadii.md", "Autodesk.DesignScript.Geometry.Ellipse.ByOriginRadii.md", "Autodesk.DesignScript.Geometry.Ellipse.ByOriginVectors.md", "Autodesk.DesignScript.Geometry.Ellipse.ByPlaneRadii.md", "Autodesk.DesignScript.Geometry.Ellipse.CenterPoint.md", "Autodesk.DesignScript.Geometry.Ellipse.MajorAxis.md", "Autodesk.DesignScript.Geometry.Ellipse.MinorAxis.md", "Autodesk.DesignScript.Geometry.EllipseArc.ByPlaneRadiiAngles.md", "Autodesk.DesignScript.Geometry.EllipseArc.CenterPoint.md", "Autodesk.DesignScript.Geometry.EllipseArc.MajorAxis.md", "Autodesk.DesignScript.Geometry.EllipseArc.MinorAxis.md", "Autodesk.DesignScript.Geometry.EllipseArc.Plane.md", "Autodesk.DesignScript.Geometry.EllipseArc.StartAngle.md", "Autodesk.DesignScript.Geometry.EllipseArc.SweepAngle.md", "Autodesk.DesignScript.Geometry.Face.Edges.md", "Autodesk.DesignScript.Geometry.Face.SurfaceGeometry.md", "Autodesk.DesignScript.Geometry.Face.Vertices.md", "Autodesk.DesignScript.Geometry.Geometry.BoundingBox.md", "Autodesk.DesignScript.Geometry.Geometry.ClosestPointTo.md", "Autodesk.DesignScript.Geometry.Geometry.ContextCoordinateSystem.md", "Autodesk.DesignScript.Geometry.Geometry.DistanceTo.md", "Autodesk.DesignScript.Geometry.Geometry.DoesIntersect.md", "Autodesk.DesignScript.Geometry.Geometry.Explode.md", "Autodesk.DesignScript.Geometry.Geometry.ExportToSAT.md", "Autodesk.DesignScript.Geometry.Geometry.FromSolidDef.md", "Autodesk.DesignScript.Geometry.Geometry.Intersect.md", "Autodesk.DesignScript.Geometry.Geometry.IntersectAll.md", "Autodesk.DesignScript.Geometry.Geometry.IsAlmostEqualTo.md", "Autodesk.DesignScript.Geometry.Geometry.Mirror.md", "Autodesk.DesignScript.Geometry.Geometry.OrientedBoundingBox.md", "Autodesk.DesignScript.Geometry.Geometry.Rotate(basePlane, degrees).md", "Autodesk.DesignScript.Geometry.Geometry.Rotate(origin, axis, degrees).md", "Autodesk.DesignScript.Geometry.Geometry.Scale(amount).md", "Autodesk.DesignScript.Geometry.Geometry.Scale(basePoint, from, to).md", "Autodesk.DesignScript.Geometry.Geometry.Scale(plane, xamount, yamount, zamount).md", "Autodesk.DesignScript.Geometry.Geometry.Scale(xamount, yamount, zamount).md", "Autodesk.DesignScript.Geometry.Geometry.Scale1D.md", "Autodesk.DesignScript.Geometry.Geometry.Scale2D.md", "Autodesk.DesignScript.Geometry.Geometry.SerializeAsSAB.md", "Autodesk.DesignScript.Geometry.Geometry.Split.md", "Autodesk.DesignScript.Geometry.Geometry.ToSolidDef.md", "Autodesk.DesignScript.Geometry.Geometry.Transform(cs).md", "Autodesk.DesignScript.Geometry.Geometry.Transform(fromCoordinateSystem, contextCoordinateSystem).md", "Autodesk.DesignScript.Geometry.Geometry.Translate(direction).md", "Autodesk.DesignScript.Geometry.Geometry.Translate(direction, distance).md", "Autodesk.DesignScript.Geometry.Geometry.Translate(xTranslation, yTranslation, zTranslation).md", "Autodesk.DesignScript.Geometry.Geometry.Trim.md", "Autodesk.DesignScript.Geometry.Helix.Angle.md", "Autodesk.DesignScript.Geometry.Helix.AxisDirection.md", "Autodesk.DesignScript.Geometry.Helix.AxisPoint.md", "Autodesk.DesignScript.Geometry.Helix.ByAxis.md", "Autodesk.DesignScript.Geometry.Helix.Pitch.md", "Autodesk.DesignScript.Geometry.Helix.Radius.md", "Autodesk.DesignScript.Geometry.IndexGroup.A.md", "Autodesk.DesignScript.Geometry.IndexGroup.B.md", "Autodesk.DesignScript.Geometry.IndexGroup.ByIndices(a, b, c).md", "Autodesk.DesignScript.Geometry.IndexGroup.ByIndices(a, b, c, d).md", "Autodesk.DesignScript.Geometry.IndexGroup.C.md", "Autodesk.DesignScript.Geometry.IndexGroup.Count.md", "Autodesk.DesignScript.Geometry.IndexGroup.D.md", "Autodesk.DesignScript.Geometry.Line.ByBestFitThroughPoints.md", "Autodesk.DesignScript.Geometry.Line.ByStartPointDirectionLength.md", "Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint.md", "Autodesk.DesignScript.Geometry.Line.ByTangency.md", "Autodesk.DesignScript.Geometry.Line.Direction.md", "Autodesk.DesignScript.Geometry.Mesh.Area.md", "Autodesk.DesignScript.Geometry.Mesh.BooleanDifference.md", "Autodesk.DesignScript.Geometry.Mesh.BooleanIntersection.md", "Autodesk.DesignScript.Geometry.Mesh.BooleanUnion.md", "Autodesk.DesignScript.Geometry.Mesh.BoundingBox.md", "Autodesk.DesignScript.Geometry.Mesh.ByGeometry.md", "Autodesk.DesignScript.Geometry.Mesh.ByPointsFaceIndices.md", "Autodesk.DesignScript.Geometry.Mesh.ByVerticesAndIndices.md", "Autodesk.DesignScript.Geometry.Mesh.CloseCracks.md", "Autodesk.DesignScript.Geometry.Mesh.Cone.md", "Autodesk.DesignScript.Geometry.Mesh.Cuboid.md", "Autodesk.DesignScript.Geometry.Mesh.EdgeCount.md", "Autodesk.DesignScript.Geometry.Mesh.Edges.md", "Autodesk.DesignScript.Geometry.Mesh.EdgesAsSixNumbers.md", "Autodesk.DesignScript.Geometry.Mesh.Explode.md", "Autodesk.DesignScript.Geometry.Mesh.ExportMeshes.md", "Autodesk.DesignScript.Geometry.Mesh.ExtrudePolyCurve.md", "Autodesk.DesignScript.Geometry.Mesh.FaceIndices.md", "Autodesk.DesignScript.Geometry.Mesh.GenerateSupport.md", "Autodesk.DesignScript.Geometry.Mesh.ImportFile.md", "Autodesk.DesignScript.Geometry.Mesh.Intersect.md", "Autodesk.DesignScript.Geometry.Mesh.MakeHollow.md", "Autodesk.DesignScript.Geometry.Mesh.MakeWatertight.md", "Autodesk.DesignScript.Geometry.Mesh.Mirror.md", "Autodesk.DesignScript.Geometry.Mesh.Nearest.md", "Autodesk.DesignScript.Geometry.Mesh.Plane.md", "Autodesk.DesignScript.Geometry.Mesh.PlaneCut.md", "Autodesk.DesignScript.Geometry.Mesh.Project.md", "Autodesk.DesignScript.Geometry.Mesh.Reduce.md", "Autodesk.DesignScript.Geometry.Mesh.Remesh.md", "Autodesk.DesignScript.Geometry.Mesh.Repair.md", "Autodesk.DesignScript.Geometry.Mesh.Rotate.md", "Autodesk.DesignScript.Geometry.Mesh.Scale(scaleFactor).md", "Autodesk.DesignScript.Geometry.Mesh.Scale(x, y, z).md", "Autodesk.DesignScript.Geometry.Mesh.Smooth.md", "Autodesk.DesignScript.Geometry.Mesh.Sphere.md", "Autodesk.DesignScript.Geometry.Mesh.Translate(vector).md", "Autodesk.DesignScript.Geometry.Mesh.Translate(vector, distance).md", "Autodesk.DesignScript.Geometry.Mesh.Translate(x, y, z).md", "Autodesk.DesignScript.Geometry.Mesh.TriangleCentroids.md", "Autodesk.DesignScript.Geometry.Mesh.TriangleCount.md", "Autodesk.DesignScript.Geometry.Mesh.TriangleNormals.md", "Autodesk.DesignScript.Geometry.Mesh.Triangles.md", "Autodesk.DesignScript.Geometry.Mesh.TrianglesAsNineNumbers.md", "Autodesk.DesignScript.Geometry.Mesh.VertexCount.md", "Autodesk.DesignScript.Geometry.Mesh.VertexIndicesByTri.md", "Autodesk.DesignScript.Geometry.Mesh.VertexNormals.md", "Autodesk.DesignScript.Geometry.Mesh.VertexPositions.md", "Autodesk.DesignScript.Geometry.Mesh.VerticesAsThreeNumbers.md", "Autodesk.DesignScript.Geometry.Mesh.Volume.md", "Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPoints(points).md", "Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPoints(points, degree).md", "Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPoints(points, degree, closeCurve).md", "Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPointsWeightsKnots.md", "Autodesk.DesignScript.Geometry.NurbsCurve.ByPoints(points).md", "Autodesk.DesignScript.Geometry.NurbsCurve.ByPoints(points, closeCurve).md", "Autodesk.DesignScript.Geometry.NurbsCurve.ByPoints(points, degree).md", "Autodesk.DesignScript.Geometry.NurbsCurve.ByPointsTangents.md", "Autodesk.DesignScript.Geometry.NurbsCurve.ControlPoints.md", "Autodesk.DesignScript.Geometry.NurbsCurve.Degree.md", "Autodesk.DesignScript.Geometry.NurbsCurve.IsPeriodic.md", "Autodesk.DesignScript.Geometry.NurbsCurve.IsRational.md", "Autodesk.DesignScript.Geometry.NurbsCurve.Knots.md", "Autodesk.DesignScript.Geometry.NurbsCurve.Weights.md", "Autodesk.DesignScript.Geometry.NurbsSurface.ByControlPoints.md", "Autodesk.DesignScript.Geometry.NurbsSurface.ByControlPointsWeightsKnots.md", "Autodesk.DesignScript.Geometry.NurbsSurface.ByPoints.md", "Autodesk.DesignScript.Geometry.NurbsSurface.ByPointsTangents.md", "Autodesk.DesignScript.Geometry.NurbsSurface.ByPointsTangentsKnotsDerivatives.md", "Autodesk.DesignScript.Geometry.NurbsSurface.ControlPoints.md", "Autodesk.DesignScript.Geometry.NurbsSurface.DegreeU.md", "Autodesk.DesignScript.Geometry.NurbsSurface.DegreeV.md", "Autodesk.DesignScript.Geometry.NurbsSurface.IsPeriodicInU.md", "Autodesk.DesignScript.Geometry.NurbsSurface.IsPeriodicInV.md", "Autodesk.DesignScript.Geometry.NurbsSurface.IsRational.md", "Autodesk.DesignScript.Geometry.NurbsSurface.NumControlPointsU.md", "Autodesk.DesignScript.Geometry.NurbsSurface.NumControlPointsV.md", "Autodesk.DesignScript.Geometry.NurbsSurface.UKnots.md", "Autodesk.DesignScript.Geometry.NurbsSurface.VKnots.md", "Autodesk.DesignScript.Geometry.NurbsSurface.Weights.md", "Autodesk.DesignScript.Geometry.PanelSurface.ByCrossSplitSquares.md", "Autodesk.DesignScript.Geometry.PanelSurface.ByCustomOrthogonalLattice.md", "Autodesk.DesignScript.Geometry.PanelSurface.ByDiagonallySplitSquares.md", "Autodesk.DesignScript.Geometry.PanelSurface.ByDiamonds.md", "Autodesk.DesignScript.Geometry.PanelSurface.ByHexagons.md", "Autodesk.DesignScript.Geometry.PanelSurface.ByParallelograms.md", "Autodesk.DesignScript.Geometry.PanelSurface.ByQuads.md", "Autodesk.DesignScript.Geometry.PanelSurface.ByRhombiTriHexagonals.md", "Autodesk.DesignScript.Geometry.PanelSurface.BySplitDiamonds.md", "Autodesk.DesignScript.Geometry.PanelSurface.ByStaggeredQuads.md", "Autodesk.DesignScript.Geometry.PanelSurface.GetNumPanelVertices.md", "Autodesk.DesignScript.Geometry.PanelSurface.GetPanelPoints.md", "Autodesk.DesignScript.Geometry.PanelSurface.GetPanelPolygon.md", "Autodesk.DesignScript.Geometry.PanelSurface.GetPanelVertices.md", "Autodesk.DesignScript.Geometry.PanelSurface.GetPoint.md", "Autodesk.DesignScript.Geometry.PanelSurface.GetVertex.md", "Autodesk.DesignScript.Geometry.PanelSurface.GetVertexIndex.md", "Autodesk.DesignScript.Geometry.PanelSurface.NumPanels.md", "Autodesk.DesignScript.Geometry.PanelSurface.NumVertices.md", "Autodesk.DesignScript.Geometry.PanelSurface.SetTransform.md", "Autodesk.DesignScript.Geometry.Plane.ByBestFitThroughPoints.md", "Autodesk.DesignScript.Geometry.Plane.ByLineAndPoint.md", "Autodesk.DesignScript.Geometry.Plane.ByOriginNormal.md", "Autodesk.DesignScript.Geometry.Plane.ByOriginNormalXAxis.md", "Autodesk.DesignScript.Geometry.Plane.ByOriginXAxisYAxis.md", "Autodesk.DesignScript.Geometry.Plane.ByThreePoints.md", "Autodesk.DesignScript.Geometry.Plane.Normal.md", "Autodesk.DesignScript.Geometry.Plane.Offset.md", "Autodesk.DesignScript.Geometry.Plane.Origin.md", "Autodesk.DesignScript.Geometry.Plane.ToCoordinateSystem.md", "Autodesk.DesignScript.Geometry.Plane.XAxis.md", "Autodesk.DesignScript.Geometry.Plane.XY.md", "Autodesk.DesignScript.Geometry.Plane.XZ.md", "Autodesk.DesignScript.Geometry.Plane.YAxis.md", "Autodesk.DesignScript.Geometry.Plane.YZ.md", "Autodesk.DesignScript.Geometry.Point.Add.md", "Autodesk.DesignScript.Geometry.Point.AsVector.md", "Autodesk.DesignScript.Geometry.Point.ByCartesianCoordinates.md", "Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, y).md", "Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, y, z).md", "Autodesk.DesignScript.Geometry.Point.ByCylindricalCoordinates.md", "Autodesk.DesignScript.Geometry.Point.BySphericalCoordinates.md", "Autodesk.DesignScript.Geometry.Point.Origin.md", "Autodesk.DesignScript.Geometry.Point.Project.md", "Autodesk.DesignScript.Geometry.Point.PruneDuplicates.md", "Autodesk.DesignScript.Geometry.Point.Subtract.md", "Autodesk.DesignScript.Geometry.Point.X.md", "Autodesk.DesignScript.Geometry.Point.Y.md", "Autodesk.DesignScript.Geometry.Point.Z.md", "Autodesk.DesignScript.Geometry.PolyCurve.BasePlane.md", "Autodesk.DesignScript.Geometry.PolyCurve.ByGroupedCurves.md", "Autodesk.DesignScript.Geometry.PolyCurve.ByJoinedCurves.md", "Autodesk.DesignScript.Geometry.PolyCurve.ByPoints.md", "Autodesk.DesignScript.Geometry.PolyCurve.ByThickeningCurveNormal.md", "Autodesk.DesignScript.Geometry.PolyCurve.CloseWithLine.md", "Autodesk.DesignScript.Geometry.PolyCurve.CloseWithLineAndTangentArcs.md", "Autodesk.DesignScript.Geometry.PolyCurve.CurveAtIndex.md", "Autodesk.DesignScript.Geometry.PolyCurve.Curves.md", "Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc.md", "Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithEllipse.md", "Autodesk.DesignScript.Geometry.PolyCurve.Fillet.md", "Autodesk.DesignScript.Geometry.PolyCurve.Heal.md", "Autodesk.DesignScript.Geometry.PolyCurve.NumberOfCurves.md", "Autodesk.DesignScript.Geometry.PolyCurve.OffsetMany.md", "Autodesk.DesignScript.Geometry.PolyCurve.Points.md", "Autodesk.DesignScript.Geometry.Polygon.ByPoints.md", "Autodesk.DesignScript.Geometry.Polygon.Center.md", "Autodesk.DesignScript.Geometry.Polygon.ContainmentTest.md", "Autodesk.DesignScript.Geometry.Polygon.Corners.md", "Autodesk.DesignScript.Geometry.Polygon.PlaneDeviation.md", "Autodesk.DesignScript.Geometry.Polygon.RegularPolygon.md", "Autodesk.DesignScript.Geometry.Polygon.SelfIntersections.md", "Autodesk.DesignScript.Geometry.PolySurface.ByJoinedSurfaces.md", "Autodesk.DesignScript.Geometry.PolySurface.ByLoft(crossSections).md", "Autodesk.DesignScript.Geometry.PolySurface.ByLoft(crossSections, guideCurve).md", "Autodesk.DesignScript.Geometry.PolySurface.ByLoftGuides.md", "Autodesk.DesignScript.Geometry.PolySurface.BySolid.md", "Autodesk.DesignScript.Geometry.PolySurface.BySweep.md", "Autodesk.DesignScript.Geometry.PolySurface.Chamfer.md", "Autodesk.DesignScript.Geometry.PolySurface.EdgeCount.md", "Autodesk.DesignScript.Geometry.PolySurface.ExtractSolids.md", "Autodesk.DesignScript.Geometry.PolySurface.Fillet.md", "Autodesk.DesignScript.Geometry.PolySurface.LocateSurfacesByLine.md", "Autodesk.DesignScript.Geometry.PolySurface.LocateSurfacesByPoint.md", "Autodesk.DesignScript.Geometry.PolySurface.SurfaceCount.md", "Autodesk.DesignScript.Geometry.PolySurface.Surfaces.md", "Autodesk.DesignScript.Geometry.PolySurface.UnconnectedBoundaries.md", "Autodesk.DesignScript.Geometry.PolySurface.VertexCount.md", "Autodesk.DesignScript.Geometry.Rectangle.ByCornerPoints(p1, p2, p3, p4).md", "Autodesk.DesignScript.Geometry.Rectangle.ByCornerPoints(points).md", "Autodesk.DesignScript.Geometry.Rectangle.ByWidthLength(coordinateSystem, width, length).md", "Autodesk.DesignScript.Geometry.Rectangle.ByWidthLength(plane, width, length).md", "Autodesk.DesignScript.Geometry.Rectangle.ByWidthLength(width, length).md", "Autodesk.DesignScript.Geometry.Rectangle.Height.md", "Autodesk.DesignScript.Geometry.Rectangle.Width.md", "Autodesk.DesignScript.Geometry.Solid.Area.md", "Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces.md", "Autodesk.DesignScript.Geometry.Solid.ByLoft(crossSections).md", "Autodesk.DesignScript.Geometry.Solid.ByLoft(crossSections, guideCurves).md", "Autodesk.DesignScript.Geometry.Solid.ByRevolve.md", "Autodesk.DesignScript.Geometry.Solid.ByRuledLoft.md", "Autodesk.DesignScript.Geometry.Solid.BySweep.md", "Autodesk.DesignScript.Geometry.Solid.BySweep2Rails.md", "Autodesk.DesignScript.Geometry.Solid.ByUnion.md", "Autodesk.DesignScript.Geometry.Solid.Centroid.md", "Autodesk.DesignScript.Geometry.Solid.Chamfer.md", "Autodesk.DesignScript.Geometry.Solid.Difference.md", "Autodesk.DesignScript.Geometry.Solid.DifferenceAll.md", "Autodesk.DesignScript.Geometry.Solid.Fillet.md", "Autodesk.DesignScript.Geometry.Solid.ProjectInputOnto.md", "Autodesk.DesignScript.Geometry.Solid.Repair.md", "Autodesk.DesignScript.Geometry.Solid.Separate.md", "Autodesk.DesignScript.Geometry.Solid.ThinShell.md", "Autodesk.DesignScript.Geometry.Solid.Union.md", "Autodesk.DesignScript.Geometry.Solid.Volume.md", "Autodesk.DesignScript.Geometry.Sphere.ByBestFit.md", "Autodesk.DesignScript.Geometry.Sphere.ByCenterPointRadius.md", "Autodesk.DesignScript.Geometry.Sphere.ByFourPoints.md", "Autodesk.DesignScript.Geometry.Sphere.CenterPoint.md", "Autodesk.DesignScript.Geometry.Sphere.Radius.md", "Autodesk.DesignScript.Geometry.Surface.ApproximateWithTolerance.md", "Autodesk.DesignScript.Geometry.Surface.Area.md", "Autodesk.DesignScript.Geometry.Surface.ByLoft(crossSections).md", "Autodesk.DesignScript.Geometry.Surface.ByLoft(crossSections, guideCurves).md", "Autodesk.DesignScript.Geometry.Surface.ByPatch.md", "Autodesk.DesignScript.Geometry.Surface.ByPerimeterPoints.md", "Autodesk.DesignScript.Geometry.Surface.ByRevolve.md", "Autodesk.DesignScript.Geometry.Surface.ByRuledLoft.md", "Autodesk.DesignScript.Geometry.Surface.BySweep.md", "Autodesk.DesignScript.Geometry.Surface.BySweep2Rails.md", "Autodesk.DesignScript.Geometry.Surface.ByUnion.md", "Autodesk.DesignScript.Geometry.Surface.Closed.md", "Autodesk.DesignScript.Geometry.Surface.ClosedInU.md", "Autodesk.DesignScript.Geometry.Surface.ClosedInV.md", "Autodesk.DesignScript.Geometry.Surface.CoordinateSystemAtParameter.md", "Autodesk.DesignScript.Geometry.Surface.CurvatureAtParameter.md", "Autodesk.DesignScript.Geometry.Surface.DerivativesAtParameter.md", "Autodesk.DesignScript.Geometry.Surface.Difference.md", "Autodesk.DesignScript.Geometry.Surface.FlipNormalDirection.md", "Autodesk.DesignScript.Geometry.Surface.GaussianCurvatureAtParameter.md", "Autodesk.DesignScript.Geometry.Surface.GetIsoline.md", "Autodesk.DesignScript.Geometry.Surface.NormalAtParameter.md", "Autodesk.DesignScript.Geometry.Surface.NormalAtPoint.md", "Autodesk.DesignScript.Geometry.Surface.Offset.md", "Autodesk.DesignScript.Geometry.Surface.Perimeter.md", "Autodesk.DesignScript.Geometry.Surface.PerimeterCurves.md", "Autodesk.DesignScript.Geometry.Surface.PointAtParameter.md", "Autodesk.DesignScript.Geometry.Surface.PrincipalCurvaturesAtParameter.md", "Autodesk.DesignScript.Geometry.Surface.PrincipalDirectionsAtParameter.md", "Autodesk.DesignScript.Geometry.Surface.ProjectInputOnto.md", "Autodesk.DesignScript.Geometry.Surface.Repair.md", "Autodesk.DesignScript.Geometry.Surface.SubtractFrom.md", "Autodesk.DesignScript.Geometry.Surface.TangentAtUParameter.md", "Autodesk.DesignScript.Geometry.Surface.TangentAtVParameter.md", "Autodesk.DesignScript.Geometry.Surface.Thicken(thickness).md", "Autodesk.DesignScript.Geometry.Surface.Thicken(thickness, both_sides).md", "Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface.md", "Autodesk.DesignScript.Geometry.Surface.TrimWithEdgeLoops.md", "Autodesk.DesignScript.Geometry.Surface.UVParameterAtPoint.md", "Autodesk.DesignScript.Geometry.Topology.Edges.md", "Autodesk.DesignScript.Geometry.Topology.Faces.md", "Autodesk.DesignScript.Geometry.Topology.Vertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Info.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsBorder.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsManifold.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.UVNFrame.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Index.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Info.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Sides.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineFace.UVNFrame.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Valence.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByRadial.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.IsRadial.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.RadialSymmetryFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.XAxis.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.YAxis.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ZAxis.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.Axis.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.IsRadial.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.Plane.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentAngle.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentsCount.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.AddReflections.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BevelEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildPipes.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByBoxCorners.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByBoxLengths(cs, width, length, height, xSpans, ySpans, zSpans, symmetry, inSmoothMode).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByBoxLengths(origin, width, length, height, xSpans, ySpans, zSpans, symmetry, inSmoothMode).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByBoxLengths(width, length, height, xSpans, ySpans, zSpans, symmetry, inSmoothMode).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByCombinedTSplineSurfaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByConeCoordinateSystemHeightRadii.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByConeCoordinateSystemHeightRadius.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByConePointsRadii.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByConePointsRadius.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByCylinderPointsRadius.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByCylinderRadiusHeight.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByNurbsSurfaceCurvature.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByNurbsSurfaceUniform.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneLineAndPoint.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormal.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormalXAxis.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginXAxisYAxis.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneThreePoints.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByQuadballCenterRadius.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByQuadballCoordinateSystemRadius.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByRevolve.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BySphereBestFit.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BySphereCenterPointRadius.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BySphereFourPoints.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BySweep.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByTorusCenterRadii.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByTorusCoordinateSystemRadii.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreateMatch(tsEdges, brepEdges, continuity, useArclength, useRefinement, numRefinementSteps, refinementTolerance, usePropagation, widthOfPropagation, scale, flipSourceTargetAlignment).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreateMatch(tsEdges, curves, continuity, useArclength, useRefinement, numRefinementSteps, refinementTolerance, usePropagation, widthOfPropagation, scale, flipSourceTargetAlignment).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DeleteEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DeleteFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DeleteVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DeserializeFromTSM.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.EnableSmoothMode.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExportToTSM.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExportToTSS.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExtrudeEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExtrudeEdgesAlongCurve.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExtrudeFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExtrudeFacesAlongCurve.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FlattenVertices(vertices).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FlattenVertices(vertices, parallelPlane).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FlipNormals.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ImportFromTSM(file, inSmoothMode).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ImportFromTSM(filePath, inSmoothMode).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ImportFromTSS(file, inSmoothMode).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ImportFromTSS(filePath, inSmoothMode).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Interpolate.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsExtractable.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsInBoxMode.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsWaterTight.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MakeUniform.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MergeEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MoveVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.PullVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Reflections.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Repair.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SerializeAsTSM.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Thicken(distance, softEdges).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Thicken(vector, softEdges).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToBRep.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToMesh.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldVertices(firstGroup, secondGroup, keepSubdCreases).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldVertices(vertices, newPosition, keepSubdCreases).md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgesCount.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.FaceByIndex.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.FacesCount.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.InnerEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.InnerFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.InnerVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.NGonFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.NonManifoldEdges.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.NonManifoldVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.RegularFaces.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.RegularVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.StarPointVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.TPointVertices.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.VertexByIndex.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.VerticesCount.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineUVNFrame.Normal.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineUVNFrame.Position.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineUVNFrame.U.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineUVNFrame.V.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.FunctionalValence.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Info.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsManifold.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.UVNFrame.md", "Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Valence.md", "Autodesk.DesignScript.Geometry.UV.ByCoordinates.md", "Autodesk.DesignScript.Geometry.UV.U.md", "Autodesk.DesignScript.Geometry.UV.V.md", "Autodesk.DesignScript.Geometry.Vector.Add.md", "Autodesk.DesignScript.Geometry.Vector.AngleAboutAxis.md", "Autodesk.DesignScript.Geometry.Vector.AngleWithVector.md", "Autodesk.DesignScript.Geometry.Vector.AsPoint.md", "Autodesk.DesignScript.Geometry.Vector.ByCoordinates(x, y, z).md", "Autodesk.DesignScript.Geometry.Vector.ByCoordinates(x, y, z, normalized).md", "Autodesk.DesignScript.Geometry.Vector.ByTwoPoints.md", "Autodesk.DesignScript.Geometry.Vector.Cross.md", "Autodesk.DesignScript.Geometry.Vector.Dot.md", "Autodesk.DesignScript.Geometry.Vector.IsAlmostEqualTo.md", "Autodesk.DesignScript.Geometry.Vector.IsParallel.md", "Autodesk.DesignScript.Geometry.Vector.Length.md", "Autodesk.DesignScript.Geometry.Vector.Normalized.md", "Autodesk.DesignScript.Geometry.Vector.Reverse.md", "Autodesk.DesignScript.Geometry.Vector.Rotate(axis, degrees).md", "Autodesk.DesignScript.Geometry.Vector.Rotate(plane, degrees).md", "Autodesk.DesignScript.Geometry.Vector.Scale(scale_factor).md", "Autodesk.DesignScript.Geometry.Vector.Scale(xScaleFactor, yScaleFactor, zScaleFactor).md", "Autodesk.DesignScript.Geometry.Vector.Subtract.md", "Autodesk.DesignScript.Geometry.Vector.Transform.md", "Autodesk.DesignScript.Geometry.Vector.X.md", "Autodesk.DesignScript.Geometry.Vector.XAxis.md", "Autodesk.DesignScript.Geometry.Vector.Y.md", "Autodesk.DesignScript.Geometry.Vector.YAxis.md", "Autodesk.DesignScript.Geometry.Vector.Z.md", "Autodesk.DesignScript.Geometry.Vector.ZAxis.md", "Autodesk.DesignScript.Geometry.Vertex.AdjacentEdges.md", "Autodesk.DesignScript.Geometry.Vertex.AdjacentFaces.md", "Autodesk.DesignScript.Geometry.Vertex.PointGeometry.md", "CoreNodeModels.ColorRange.md", "CoreNodeModels.CreateList.md", "CoreNodeModels.DefineData.md", "CoreNodeModels.Equals.md", "CoreNodeModels.FormattedStringFromArray.md", "CoreNodeModels.FormattedStringFromObject.md", "CoreNodeModels.FromArray.md", "CoreNodeModels.FromObject.md", "CoreNodeModels.HigherOrder.ApplyFunction.md", "CoreNodeModels.HigherOrder.CartesianProduct.md", "CoreNodeModels.HigherOrder.Combine.md", "CoreNodeModels.HigherOrder.ComposeFunctions.md", "CoreNodeModels.HigherOrder.Filter.md", "CoreNodeModels.HigherOrder.LaceLongest.md", "CoreNodeModels.HigherOrder.LaceShortest.md", "CoreNodeModels.HigherOrder.Map.md", "CoreNodeModels.HigherOrder.Reduce.md", "CoreNodeModels.HigherOrder.Replace.md", "CoreNodeModels.HigherOrder.ScanList.md", "CoreNodeModels.Input.BoolSelector.md", "CoreNodeModels.Input.ColorPalette.md", "CoreNodeModels.Input.CustomSelection.md", "CoreNodeModels.Input.DateTime.md", "CoreNodeModels.Input.Directory.md", "CoreNodeModels.Input.DirectoryObject.md", "CoreNodeModels.Input.DoubleInput.md", "CoreNodeModels.Input.DoubleSlider.md", "CoreNodeModels.Input.Filename.md", "CoreNodeModels.Input.FileObject.md", "CoreNodeModels.Input.IntegerSlider64Bit.md", "CoreNodeModels.Input.StringInput.md", "CoreNodeModels.Logic.And.md", "CoreNodeModels.Logic.Gate.md", "CoreNodeModels.Logic.Or.md", "CoreNodeModels.Logic.RefactoredIf.md", "CoreNodeModels.Logic.ScopedIf.md", "CoreNodeModels.Range.md", "CoreNodeModels.Remember.md", "CoreNodeModels.Sequence.md", "CoreNodeModels.Watch.md", "CoreNodeModels.WatchImageCore.md", "CoreNodeModels.WebRequest.md", "DesignScript.Builtin.Dictionary.ByKeysValues.md", "DesignScript.Builtin.Dictionary.Components.md", "DesignScript.Builtin.Dictionary.Count.md", "DesignScript.Builtin.Dictionary.Keys.md", "DesignScript.Builtin.Dictionary.RemoveKeys.md", "DesignScript.Builtin.Dictionary.SetValueAtKeys.md", "DesignScript.Builtin.Dictionary.ValueAtKey.md", "DesignScript.Builtin.Dictionary.Values.md", "DSOffice.Data.ExportCSV.md", "DSOffice.Data.ImportCSV.md", "DSOffice.Data.OpenXMLExportExcel.md", "DSOffice.Data.OpenXMLImportExcel.md", "DynamoUnits.Location.ByLatitudeAndLongitude.md", "DynamoUnits.Location.Latitude.md", "DynamoUnits.Location.Longitude.md", "DynamoUnits.Location.Name.md", "DynamoUnits.Quantity.ByTypeID.md", "DynamoUnits.Quantity.Name.md", "DynamoUnits.Quantity.TypeId.md", "DynamoUnits.Quantity.Units.md", "DynamoUnits.Symbol.ByTypeID.md", "DynamoUnits.Symbol.Space.md", "DynamoUnits.Symbol.StringifyDecimal.md", "DynamoUnits.Symbol.StringifyFraction.md", "DynamoUnits.Symbol.SymbolsByUnit.md", "DynamoUnits.Symbol.Text.md", "DynamoUnits.Symbol.TypeId.md", "DynamoUnits.Symbol.Unit.md", "DynamoUnits.Unit.AreUnitsConvertible.md", "DynamoUnits.Unit.ByTypeID.md", "DynamoUnits.Unit.ConvertibleUnits.md", "DynamoUnits.Unit.Name.md", "DynamoUnits.Unit.QuantitiesContainingUnit.md", "DynamoUnits.Unit.TypeId.md", "DynamoUnits.Utilities.ConvertByUnits.md", "DynamoUnits.Utilities.ParseExpression.md", "DynamoUnits.Utilities.ParseExpressionByUnit.md", "GeometryUI.DeserializeFromSABWithUnits.md", "GeometryUI.ExportWithUnits.md", "GeometryUI.ImportFromSATWithUnits.md", "GeometryUI.PanelSurfaceBoundaryConditionDropDown.md", "List.Equals.md", "List.GroupByFunction.md", "List.MaximumItemByKey.md", "List.MinimumItemByKey.md", "List.Rank.md", "List.RemoveIfNot.md", "List.SortByFunction.md", "List.TrueForAll.md", "List.TrueForAny.md", "LoopWhile.md", "Modifiers.GeometryColor.ByGeometryColor.md", "Modifiers.GeometryColor.ByMeshColors.md", "Modifiers.GeometryColor.BySurfaceColors.md", "PythonNodeModels.PythonNode.md", "PythonNodeModels.PythonStringNode.md", "Tessellation.ConvexHull.ByPoints.md", "Tessellation.Delaunay.ByParametersOnSurface.md", "Tessellation.Delaunay.ByPoints.md", "Tessellation.Voronoi.ByParametersOnSurface.md", "UnitsUI.DynamoUnitConvert.md", "UnitsUI.Quantities.md", "UnitsUI.Symbols.md", "UnitsUI.UnitInput.md", "UnitsUI.Units.md" };
         
            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreDir,
                RecursiveScan = true,
                OutputFolderPath = tempDirectory.FullName,
                Filter = preloadedLibraryPaths.Concat(new string[] 
                {CORENODEMODELS_DLL_NAME,"GeometryUI.dll","PythonNodeModels.dll","Watch3dNodeModels.dll","UnitsNodeModels.dll","" }),
                ReferencePaths = new List<string>()
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            var generatedFileNames = tempDirectory.GetFiles().Select(x => x.Name);
            var missingMDFiles = expectedMDList.Except(generatedFileNames).ToList();
            Assert.AreEqual(756, generatedFileNames.Count(), string.Format("Missing MD files: {0}", string.Join(", ",missingMDFiles)));
        }

        [Test]
        public void ProducesCorrectOutputFromCoreDirectory_dsFiles()
        {
            // Arrange
            var testOutputDirName = "TestMdOutput_CoreNodeModels";

            var expectedFileNames = new List<string>
            {
                "LoopWhile.md", 
                "List.Equals.md", 
                "List.GroupByFunction.md",
                "List.MaximumItemByKey.md",
                "List.MinimumItemByKey.md", 
                "List.Rank.md", 
                "List.RemoveIfNot.md", 
                "List.SortByFunction.md", 
                "List.TrueForAll.md", 
                "List.TrueForAny.md"
            };

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreDir,
                OutputFolderPath = tempDirectory.FullName,
                Filter = new List<string> { "FunctionObject.ds",
                "BuiltIn.ds", },
                ReferencePaths = new List<string>()
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            var generatedFileNames = tempDirectory.GetFiles().Select(x => x.Name);
            //assert count is correct.
            Assert.AreEqual(10, generatedFileNames.Count());
            CollectionAssert.AreEquivalent(expectedFileNames, generatedFileNames);

        }


        [Test]
        public void DictionaryContentIsFoundCorrectlyForCoreNodes()
        {
            // Test output is generated with the following args:
            // 
            // NodeDocumentationMarkdownGenerator.exe
            // fromdirectory
            // -i "..\Dynamo\bin\nodes"
            // -o "..\Dynamo\test\Tools\docGeneratorTestFiles\TestMdOutput_CoreNodeModels"
            // -d "..\Dynamo\test\Tools\docGeneratorTestFiles\sampledictionarycontent\Dynamo_Nodes_Documentation.json"
            // -x  "..\Dynamo\test\Tools\docGeneratorTestFiles\testlayoutspec.json"
            // -f "CoreNodeModels.dll"

            // Arrange
            var testOutputDirName = "TestMdOutput_CoreNodeModels";
            //these are new files/nodes so there is no dictionary content fo them.
            var filesToSkip = new string[] { "CoreNodeModels.FormattedStringFromObject", "CoreNodeModels.FormattedStringFromArray" };
                
            var coreNodeModelsDll = Path.Combine(DynamoCoreNodesDir, CORENODEMODELS_DLL_NAME);
            Assert.That(File.Exists(coreNodeModelsDll));

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreNodesDir,
                OutputFolderPath = tempDirectory.FullName,
                DictionaryDirectory = mockedDictionaryJson,
                LayoutSpecPath = testLayoutSpecPath,
                Filter = new List<string> { CORENODEMODELS_DLL_NAME },
                ReferencePaths = new List<string>(),
                Overwrite = true
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            var generatedFileNames = tempDirectory.GetFiles().Select(x => x.FullName);

            //assert that the generated markdown files all contain an "indepth section" from the dictionary entry, which means
            //they were all found.
            var generatedFileNamesSubset = generatedFileNames.Where(x => !filesToSkip.Contains(Path.GetFileNameWithoutExtension(x)));
            Assert.True(generatedFileNamesSubset.Where(x=>Path.GetExtension(x).Contains("md")).All(x => File.ReadAllText(x).ToLower().Contains("in depth")));
          
        }

        [Test]
        public void DictionaryImagesAreCompressed()
        {
           
            // Arrange
            var testOutputDirName = "TestMdOutput_CoreNodeModels";
            var sizesBeforeCompression = new DirectoryInfo(mockedDictionaryRoot).GetFiles("*.*", SearchOption.AllDirectories).Where(
                x => x.Extension.ToLower().Contains("gif") || x.Extension.ToLower().Contains("jpg")).OrderBy(x=>x.FullName).Select(f => File.ReadAllBytes(f.FullName).Length).ToArray();

            var coreNodeModelsDll = Path.Combine(DynamoCoreNodesDir, CORENODEMODELS_DLL_NAME);
            Assert.That(File.Exists(coreNodeModelsDll));

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreNodesDir,
                OutputFolderPath = tempDirectory.FullName,
                DictionaryDirectory = mockedDictionaryJson,
                LayoutSpecPath = testLayoutSpecPath,
                Filter = new List<string> { CORENODEMODELS_DLL_NAME },
                ReferencePaths = new List<string>(),
                Overwrite = true,
                CompressGifs = true,
                CompressImages = true,
                Verbose = true
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            var generatedFileImages = tempDirectory.GetFiles().Where(
                x => x.Extension.ToLower().Contains("gif") || x.Extension.ToLower().Contains("jpg")).OrderBy(x => x.FullName);
            var generatedFileSizes = generatedFileImages.Select(x=> File.ReadAllBytes(x.FullName).Length);
            //check all files were larger before in bytes.
            Assert.IsTrue(generatedFileSizes.Select((x, i) => sizesBeforeCompression[i] >= x).All(x=>x));

        }


        [Test]
        public void ProducesCorrectOutputFromPackage()
        {
            // Arrange
            var packageName = "Dynamo Samples";
            var packageDirectory = Path.GetFullPath(Path.Combine(toolsTestFilesDirectory, @"..\..\pkgs", packageName));
            var testOutputDirName = "doc";
            tempDirectory = new DirectoryInfo(Path.Combine(packageDirectory, testOutputDirName));
            Assert.IsFalse(tempDirectory.Exists);

            var expectedFileNames = new List<string>
            {
                "Examples.BasicExample.Awesome.md",
                "Examples.BasicExample.Create(point).md",
                "Examples.BasicExample.Create(x, y, z).md",
                "Examples.BasicExample.MultiReturnExample.md",
                "Examples.BasicExample.MultiReturnExample2.md",
                "Examples.BasicExample.Point.md",
                "Examples.CustomRenderExample.Create.md",
                "Examples.PeriodicIncrement.Increment.md",
                "Examples.PeriodicUpdateExample.PointField.md",
                "Examples.TransformableExample.ByGeometry.md",
                "Examples.TransformableExample.Geometry.md",
                "Examples.TransformableExample.TransformObject.md",
                "SampleLibraryUI.Examples.ButtonCustomNodeModel.md",
                "SampleLibraryUI.Examples.DropDownExample.md",
                "SampleLibraryUI.Examples.LocalizedCustomNodeModel.md",
                "SampleLibraryUI.Examples.SliderCustomNodeModel.md"
            };
            
            // Act
            var opts = new FromPackageOptions
            {
                InputFolderPath = packageDirectory,
                ReferencePaths = new List<string> { toolsTestFilesDirectory }
            };

            FromPackageFolderCommand.HandlePackageDocumentation(opts);

            tempDirectory.Refresh();

            // Assert
            Assert.IsTrue(tempDirectory.Exists);
            CollectionAssert.AreEquivalent(expectedFileNames, tempDirectory.GetFiles().Select(x => x.Name));
        }

        [Test]
        public void ProducesCorrectOutputFromPackageIncludingDYF()
        {
            // Arrange
            var packageName = "EvenOdd";
            var packageDirectory = Path.GetFullPath(Path.Combine(toolsTestFilesDirectory, @"..\..\pkgs", packageName));
            var testOutputDirName = "doc";
            tempDirectory = new DirectoryInfo(Path.Combine(packageDirectory, testOutputDirName));
            Assert.IsFalse(tempDirectory.Exists);

            var expectedFileNames = new List<string>
            {

                "Test.EvenOdd.md"
            };

            // Act
            var opts = new FromPackageOptions
            {
                InputFolderPath = packageDirectory,
                Overwrite = true
            };

            FromPackageFolderCommand.HandlePackageDocumentation(opts);

            tempDirectory.Refresh();

            // Assert
            Assert.IsTrue(tempDirectory.Exists);
            CollectionAssert.AreEquivalent(expectedFileNames, tempDirectory.GetFiles().Select(x => x.Name));
        }


        [Test]
        public void CanOverWriteExistingFiles()
        {
            // Arrange
            var originalOutDirName = "TestMdOutput_CoreNodeModels";
            var originalOutDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, originalOutDirName));

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            CopyFilesRecursively(originalOutDir, tempDirectory);

            var lastWriteTimeBefore = tempDirectory
                .GetFiles()
                .Select(x => x.LastWriteTime)
                .ToList();

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreNodesDir,
                OutputFolderPath = tempDirectory.FullName,
                Filter = new List<string> { CORENODEMODELS_DLL_NAME },
                ReferencePaths = new List<string>(),
                Overwrite = false
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            tempDirectory.Refresh();
            var lastWriteTimeAfterCommandWithoutOverwrite = tempDirectory
                .GetFiles()
                .Select(x => x.LastWriteTime)
                .ToList();

            opts.Overwrite = true;
            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            tempDirectory.Refresh();
            var lastWriteTimeAfterCommandWithOverwrite = tempDirectory
                .GetFiles()
                .Select(x => x.LastWriteTime)
                .ToList();

            // Assert
            Assert.IsTrue(lastWriteTimeAfterCommandWithOverwrite.Count() == lastWriteTimeBefore.Count());

            // Compare last write times on original files
            // and new files without overwrite (-w)
            for (int i = 0; i < lastWriteTimeBefore.Count(); i++)
            {
                // as overwrite has not been specified here
                // it is expected that last write time will
                // be the same for all the files
                Assert.That(DateTime.Compare(lastWriteTimeBefore[i], lastWriteTimeAfterCommandWithoutOverwrite[i]) == 0);
            }

            // Compare last write times on original files
            // and new files with overwrite (-w)
            for (int i = 0; i < lastWriteTimeBefore.Count(); i++)
            {
                // if Compare returns less than 0 first element
                // is earlier then 2nd
                Assert.That(DateTime.Compare(lastWriteTimeBefore[i], lastWriteTimeAfterCommandWithOverwrite[i]) < 0);
            }

            CollectionAssert.AreEquivalent(
                originalOutDir.GetFiles().Select(x => x.Name), 
                tempDirectory.GetFiles().Select(x => x.Name));
        }

        [Test]
        public void CanScanAssemblyFromPath()
        {
            // Arrange
            var assemblyPath = Path.Combine(DynamoCoreNodesDir, CORENODEMODELS_DLL_NAME);
            var coreNodeModelMdFilesDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, "TestMdOutput_CoreNodeModels"));
            var coreNodeModelMdFiles = coreNodeModelMdFilesDir.GetFiles();

            // Act
            var mdFileInfos = AssemblyHandler.ScanAssemblies(new List<string> { assemblyPath });


            // Assert
            Assert.IsTrue(coreNodeModelMdFiles.Count() == mdFileInfos.Count);
            AssertMdFileInfos(mdFileInfos, coreNodeModelMdFiles);
        }
        [Test]
        public void ReferencesFlagAddsReferencePaths()
        {   
            // Arrange
            //using the dynamosamples package as a reference because it's not in the default bin paths.
            var packageName = "Dynamo Samples";
            var packageDirectory = Path.GetFullPath(Path.Combine(toolsTestFilesDirectory, @"..\..\pkgs", packageName)); ;
            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreNodesDir,
                Filter = new string[] { "doesnotexist.dll" },
                ReferencePaths = new List<string> { packageDirectory }
            };


            // Act
            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);


            // Assert
            Assert.IsTrue(Program.ReferenceAssemblyPaths.Select(x => new FileInfo(x).Name).Contains("SampleLibraryUI.dll"));
        }

        [Test]
        public void CanRenameFile()
        {
            // Arrange
            var originalOutDirName = "fallback_docs";
            var originalOutDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, originalOutDirName));

            var targetMdFile = "CoreNodeModels.HigherOrder.Map.md";
            var renamedTargetMdFile = "SVLKFMPW6YIPCHS5TA2H3KJQQTSPUZOGUBWJG3VEPVFVB7DMGFDQ.md";

            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            CopyFilesRecursively(originalOutDir, tempDirectory);
            var mdFile = Path.Combine(tempDirectory.FullName, targetMdFile);
            var renamedMdFile = Path.Combine(tempDirectory.FullName, renamedTargetMdFile);

            // Act
            var opts = new RenameOptions
            {
                InputMdFile = mdFile
            };

            RenameCommand.HandleRename(opts);

            // Assert
            var mdFiles = tempDirectory.GetFiles("*.md", SearchOption.TopDirectoryOnly)
                .Select(x => x.Name);

            var content = File.ReadAllText(renamedMdFile);

            Assert.IsTrue(mdFiles.Contains(renamedTargetMdFile));
            Assert.IsTrue(content.Contains("CoreNodeModels.HigherOrder.Map"));
        }

        [Test]
        public void CanRenameFileLongName()
        {
            // Arrange
            var originalOutDirName = "fallback_docs";
            var filesDirectory = "LongNameFiles";
            var emptySpaceChar = "%20";
            var originalOutDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, originalOutDirName, filesDirectory));

            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            CopyFilesRecursively(originalOutDir, tempDirectory);

            var originalMdFile = tempDirectory.GetFiles("*.md", SearchOption.TopDirectoryOnly)
                .Select(x => x.Name).FirstOrDefault();
            Assert.IsNotNull(originalMdFile);

            //Check that the original MD file contains space characters URL encoded
            var originalMdFileContent = Path.Combine(tempDirectory.FullName, originalMdFile);
            Assert.IsTrue(File.ReadAllText(originalMdFileContent).Contains(emptySpaceChar));

            // Act
            var opts = new RenameOptions
            {
                InputMdDirectory = tempDirectory.FullName,
                MaxLength = 90
            };

            //Rename all the files in the temp directory
            RenameCommand.HandleRename(opts);

            // Assert
            var finalMdFile = tempDirectory.GetFiles("*.md", SearchOption.TopDirectoryOnly)
                .Select(x => x.Name).FirstOrDefault();
            Assert.IsNotNull(finalMdFile);

            var hashedName = Path.GetFileNameWithoutExtension(finalMdFile); 

            //Validates that all the renamed files start with the hashed name
            var allFiles = tempDirectory.GetFiles("*.*", SearchOption.TopDirectoryOnly).Select(x => x.Name);
            foreach(var file in allFiles)
            {
                Assert.IsTrue(file.StartsWith(hashedName));
            }

            //Get the image file name renamed
            var imageFile = tempDirectory.GetFiles("*.jpg", SearchOption.TopDirectoryOnly)
                .Select(x => x.Name).FirstOrDefault();
            Assert.IsNotNull(imageFile);

            //Validates that the image file name is present inside the md file content.
            var finalMdFileContent = Path.Combine(tempDirectory.FullName, finalMdFile);
            Assert.IsTrue(File.ReadAllText(finalMdFileContent).Contains(imageFile));
        }

        [Test]
        public void CanRenameFilesInADirectory()
        {
            // Arrange
            var originalOutDirName = "fallback_docs";
            var originalOutDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, originalOutDirName));

            var expectedFileNames = new List<string>
            {
                "FGRJU5ZIMM4EKNHFEXZGHJTKI73262KTH4CSUBI2IEXVH46TACRA.md",
                "HEG35EENB6LZZUAB4OKNCYCDHDTBEF7IR2YWCH7I4EOIQPFOJGFQ.md",
                "SVLKFMPW6YIPCHS5TA2H3KJQQTSPUZOGUBWJG3VEPVFVB7DMGFDQ.md",
                "list.rank.md",
                "loopwhile.md"
            };

            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            CopyFilesRecursively(originalOutDir, tempDirectory);

            // Act
            var opts = new RenameOptions
            {
                InputMdDirectory = tempDirectory.FullName,
                MaxLength = 15
            };

            RenameCommand.HandleRename(opts);

            // Assert
            CollectionAssert.AreEquivalent(expectedFileNames, tempDirectory.GetFiles().Select(x => x.Name));
        }

        #region Helpers
        internal void AssertMdFileInfos(List<MdFileInfo> mdFileInfos, FileInfo[] coreNodeModelMdFiles)
        {
            var expectedFileNames = coreNodeModelMdFiles.Select(x => Path.GetFileNameWithoutExtension(x.FullName));
            var expectedMdFileInfoNamespace = "CoreNodeModels";
            foreach (var info in mdFileInfos)
            {
                Assert.That(expectedFileNames.Contains(info.FileName));
                Assert.IsTrue(info.NodeNamespace.StartsWith(expectedMdFileInfoNamespace));
            }
        }

        protected DirectoryInfo CreateTempOutputDirectory()
        {
            string tempDirectoryPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_markdownGeneratorTestOutput");
            var tempDir = Directory.CreateDirectory(tempDirectoryPath);
            return tempDir;
        }

        protected static void CopyFilesRecursively(DirectoryInfo originalDir, DirectoryInfo targetDir)
        {
            foreach (var file in originalDir.GetFiles())
            {
                file.CopyTo(Path.Combine(targetDir.FullName, file.Name));
            }
        }

        protected static void SaveCoreLayoutSpecToPath(Assembly assembly, string savePath)
        {
            var resource = "Dynamo.LibraryViewExtensionWebView2.Packages.LibrarieJS.layoutSpecs.json";
            assembly = assembly == null ? Assembly.GetExecutingAssembly() : assembly;
            var stream = assembly.GetManifestResourceStream(resource);
            var fs = File.Create(savePath);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fs);
            fs.Close();
            stream.Close();
        }
        #endregion
    }
}
