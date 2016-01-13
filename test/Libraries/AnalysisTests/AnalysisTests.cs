using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Analysis;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo;
using Dynamo.Utilities;
using DynamoShapeManager;
using DynamoUtilities;

using NUnit.Framework;

namespace AnalysisTests
{
    [TestFixture]
    public class AnalysisTests
    {
        private TestExecutionSession executionSession;
        IExtensionApplication application = Application.Instance as IExtensionApplication;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            executionSession = new TestExecutionSession(assemblyDirectory);

            application.OnBeginExecution(executionSession);
            HostFactory.Instance.StartUp();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            executionSession = null;
        }

        [Test, Category("UnitTests")]
        public void SurfaceAnalysisDataBySurfacePointsAndResults_ValidArgs()
        {
            var sad = SurfaceData.BySurfacePointsAndValues(
                TestSurface(),
                TestUvs(),
                TestResults());

            Assert.NotNull(sad);
            Assert.NotNull(sad.Surface);
            Assert.AreEqual(sad.Values.Count, 3);
        }

        [Test, Category("UnitTests")]
        public void SurfaceAnalysisDataBySurfacePointAndResults_BadArgs()
        {
            Assert.Throws<ArgumentNullException>(() => SurfaceData.BySurfacePointsAndValues(
                null,
                TestUvs(),
                TestResults()));

            Assert.Throws<ArgumentNullException>(() => SurfaceData.BySurfacePointsAndValues(
                TestSurface(),
                null,
                TestResults()));

            Assert.Throws<ArgumentNullException>(() => SurfaceData.BySurfacePointsAndValues(
                TestSurface(),
                TestUvs(),
                null));

            // Test empty calculation set
            Assert.Throws<ArgumentException>(() => SurfaceData.BySurfacePointsAndValues(
                TestSurface(),
                new List<UV>(),
                TestResults()));

            // Test non matching results sets
            Assert.Throws<ArgumentException>(() => SurfaceData.BySurfacePointsAndValues(
                TestSurface(),
                new []{UV.ByCoordinates(0,0)},
                TestResults()));
        }

        [Test, Category("UnitTests")]
        public void PointAnalysisDataByPointsAndResults_ValidArgs()
        {
            var pad = PointData.ByPointsAndValues(
                TestPoints(),
                TestResults());

            Assert.NotNull(pad);
            Assert.NotNull(pad.ValueLocations);
            Assert.NotNull(pad.Values);
            Assert.AreEqual(pad.Values.Count, 3);
        }

        [Test, Category("UnitTests")]
        public void PointAnalysisDataByPointsAndResults_BadArgs()
        {
            Assert.Throws<ArgumentNullException>(
                () => PointData.ByPointsAndValues(null, TestResults()));

            Assert.Throws<ArgumentNullException>(
                () => PointData.ByPointsAndValues(TestPoints(), null));

            Assert.Throws<ArgumentException>(
                () => PointData.ByPointsAndValues(new []{Point.ByCoordinates(0,0)}, TestResults()));
        }

        [Test, Category("UnitTests")]
        public void VectorAnalysisDataByPointsAndResults_ValidArgs()
        {
            var vad = VectorData.ByPointsAndValues(TestPoints(), TestVectorResults());
            Assert.NotNull(vad);
            Assert.NotNull(vad.ValueLocations);
            Assert.NotNull(vad.Values);
            Assert.AreEqual(vad.Values.Count, 3);
        }

        [Test, Category("UnitTests")]
        public void VectorAnalysisDataByPointsAndResults_BadArgs()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                    VectorData.ByPointsAndValues(
                        null,
                        TestVectorResults()));

            Assert.Throws<ArgumentNullException>(
                () =>
                    VectorData.ByPointsAndValues(
                        TestPoints(),
                        null));

            Assert.Throws<ArgumentException>(
                () =>
                    VectorData.ByPointsAndValues(
                        new []{Point.ByCoordinates(0,0)},
                        TestVectorResults()));

        }

        private static Surface TestSurface()
        {
            var points = new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(0, 1, 0),
                Point.ByCoordinates(1, 1, 0),
                Point.ByCoordinates(1, 0, 0)
            };

            var srf = Surface.ByPerimeterPoints(points);
            return srf;
        }

        private static IList<double> TestResults()
        {
            var a = new List<double>{ 1.0, 2.0, 3.0 };
            return a;
        }

        private static IEnumerable<Point> TestPoints()
        {
            var pts = new[]
            {
                Point.ByCoordinates(0, 0),
                Point.ByCoordinates(0.5,0.5),
                Point.ByCoordinates(1, 1)
            };

            return pts;
        } 

        private static IEnumerable<UV> TestUvs()
        {
            var uvs = new[]
            {
                UV.ByCoordinates(0, 0),
                UV.ByCoordinates(0.5, 0.5),
                UV.ByCoordinates(1, 1)
            };

            return uvs;
        }

        private static IList<Vector> TestVectorResults()
        {
            var x = Vector.ByCoordinates(1, 0, 0);
            var y = Vector.ByCoordinates(0, 1, 0);
            var z = Vector.ByCoordinates(0, 0, 1);

            var a = new List<Vector> {x,y,z};
            return a;
        } 
    }

    /// <summary>
    /// This is a temporary session class which is only used for nodes that are using Geometries.
    /// When ProtoGeometry is loaded, the static instance GeometryFactory will be constructed which
    /// requires a session to be present.
    /// </summary>
    class TestExecutionSession : IExecutionSession, IConfiguration, IDisposable
    {
        private readonly string rootModulePath;
        private readonly Dictionary<string, object> configValues;
        private Preloader preloader;

        public TestExecutionSession(string rootModulePath)
        {
            configValues = new Dictionary<string, object>();
            this.rootModulePath = rootModulePath;
        }

        public IConfiguration Configuration
        {
            get { return this; }
        }

        public string SearchFile(string fileName)
        {
            var path = Path.Combine(RootModulePath, fileName);
            if (File.Exists(path))
                return path;
            return fileName;
        }

        public string RootModulePath
        {
            get { return rootModulePath; }
        }

        public string[] IncludeDirectories
        {
            get { return new string[] { RootModulePath }; }
        }

        public bool IsDebugMode
        {
            get { return false; }
        }

        public object GetConfigValue(string config)
        {
            if (string.Compare(ConfigurationKeys.GeometryFactory, config) == 0)
            {
                if (preloader == null)
                {
                    var exePath = Assembly.GetExecutingAssembly().Location;
                    preloader = new Preloader(Path.GetDirectoryName(exePath));
                    preloader.Preload();
                }

                return preloader.GeometryFactoryPath;
            }

            if (configValues.ContainsKey(config))
                return configValues[config];

            return null;
        }

        public void SetConfigValue(string config, object value)
        {
            configValues[config] = value;
        }

        public void Dispose()
        {
            configValues.Clear();
        }
    }
}
