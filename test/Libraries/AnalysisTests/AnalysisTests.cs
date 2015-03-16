using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Analysis.DataTypes;

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
        public void SurfaceAnalysisDataBySurfaceAndPoints_ValidArgs()
        {
            var sad = SurfaceAnalysisData.BySurfaceAndPoints(TestSurface(), TestUvs());
            Assert.NotNull(sad);
            Assert.NotNull(sad.Surface);
            Assert.NotNull(sad.CalculationLocations);
        }

        [Test, Category("UnitTests")]
        public void SurfaceAnalysisDataBySurfaceAndPoints_BadArgs()
        {
            Assert.Throws<ArgumentNullException>(()=>SurfaceAnalysisData.BySurfaceAndPoints(null, TestUvs()));
            Assert.Throws<ArgumentNullException>(() => SurfaceAnalysisData.BySurfaceAndPoints(TestSurface(), null));
        }

        [Test, Category("UnitTests")]
        public void SurfaceAnalysisDataBySurfacePointsAndResults_ValidArgs()
        {
            var sad = SurfaceAnalysisData.BySurfacePointsAndResults(
                TestSurface(),
                TestUvs(),
                TestResultNames(),
                TestResults());

            Assert.NotNull(sad);
            Assert.NotNull(sad.Surface);
            Assert.AreEqual(sad.Results.Count, 3);
        }

        [Test, Category("UnitTests")]
        public void SurfaceAnalysisDataBySurfacePointAndResults_BadArgs()
        {
            Assert.Throws<ArgumentNullException>(() => SurfaceAnalysisData.BySurfacePointsAndResults(
                null,
                TestUvs(),
                TestResultNames(),
                TestResults()));

            Assert.Throws<ArgumentNullException>(() => SurfaceAnalysisData.BySurfacePointsAndResults(
                TestSurface(),
                null,
                TestResultNames(),
                TestResults()));

            Assert.Throws<ArgumentNullException>(() => SurfaceAnalysisData.BySurfacePointsAndResults(
                TestSurface(),
                TestUvs(),
                null,
                TestResults()));

            Assert.Throws<ArgumentNullException>(() => SurfaceAnalysisData.BySurfacePointsAndResults(
                TestSurface(),
                TestUvs(),
                TestResultNames(),
                null));

            // Test empty calculation set
            Assert.Throws<ArgumentException>(() => SurfaceAnalysisData.BySurfacePointsAndResults(
                TestSurface(),
                new List<UV>(),
                TestResultNames(),
                TestResults()));

            // Test non matching results sets
            Assert.Throws<ArgumentException>(() => SurfaceAnalysisData.BySurfacePointsAndResults(
                TestSurface(),
                TestUvs(),
                new []{"cat","foo"},
                TestResults()));
        }

        [Test, Category("UnitTests")]
        public void PointAnalysisDataByPoints_ValidArgs()
        {
            var pad = PointAnalysisData.ByPoints(TestPoints());
            Assert.NotNull(pad);
            Assert.NotNull(pad.CalculationLocations);
        }

        [Test, Category("UnitTests")]
        public void PointAnalysisDataByPoints_BadArgs()
        {
            Assert.Throws<ArgumentNullException>(() => PointAnalysisData.ByPoints(null));
        }

        [Test, Category("UnitTests")]
        public void PointAnalysisDataByPointsAndResults_ValidArgs()
        {
            var pad = PointAnalysisData.ByPointsAndResults(
                TestPoints(),
                TestResultNames(),
                TestResults());

            Assert.NotNull(pad);
            Assert.NotNull(pad.CalculationLocations);
            Assert.NotNull(pad.Results);
            Assert.AreEqual(pad.Results.Count, 3);
        }

        [Test, Category("UnitTests")]
        public void PointAnalysisDataByPointsAndResults_BadArgs()
        {
            Assert.Throws<ArgumentNullException>(
                () => PointAnalysisData.ByPointsAndResults(null, TestResultNames(), TestResults()));

            Assert.Throws<ArgumentNullException>(
                () => PointAnalysisData.ByPointsAndResults(TestPoints(), null, TestResults()));

            Assert.Throws<ArgumentNullException>(
                () => PointAnalysisData.ByPointsAndResults(TestPoints(), TestResultNames(), null));

            Assert.Throws<ArgumentException>(
                () => PointAnalysisData.ByPointsAndResults(TestPoints(), new []{"cat","foo"}, TestResults()));
        }

        [Test, Category("UnitTests")]
        public void VectorAnalysisDataByPoints_ValidArgs()
        {
            var vad = VectorAnalysisData.ByPoints(TestPoints());
            Assert.NotNull(vad);
            Assert.NotNull(vad.CalculationLocations);
            Assert.AreEqual(vad.CalculationLocations.Count(), 3);
        }

        [Test, Category("UnitTests")]
        public void VectorAnalysisdataByPoint_BadArgs()
        {
            Assert.Throws<ArgumentNullException>(() => VectorAnalysisData.ByPoints(null));
        }

        [Test, Category("UnitTests")]
        public void VectorAnalysisDataByPointsAndResults_ValidArgs()
        {
            var vad = VectorAnalysisData.ByPointsAndResults(TestPoints(), TestResultNames(), TestVectorResults());
            Assert.NotNull(vad);
            Assert.NotNull(vad.CalculationLocations);
            Assert.NotNull(vad.Results);
            Assert.AreEqual(vad.Results.Count, 3);
        }

        [Test, Category("UnitTests")]
        public void VectorAnalysisDataByPointsAndResults_BadArgs()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                    VectorAnalysisData.ByPointsAndResults(
                        null,
                        TestResultNames(),
                        TestVectorResults()));

            Assert.Throws<ArgumentNullException>(
                () =>
                    VectorAnalysisData.ByPointsAndResults(
                        TestPoints(),
                        null,
                        TestVectorResults()));

            Assert.Throws<ArgumentNullException>(
                () =>
                    VectorAnalysisData.ByPointsAndResults(
                        TestPoints(),
                        TestResultNames(),
                        null));

            Assert.Throws<ArgumentException>(
                () =>
                    VectorAnalysisData.ByPointsAndResults(
                        TestPoints(),
                        new []{"cat","foo"},
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

        private static IList<string> TestResultNames()
        {
            return new[] { "cat", "foo", "bar" };
        } 

        private static IList<IList<double>> TestResults()
        {
            var a = new List<double>{ 1.0, 2.0, 3.0 };
            var b = new List<double>{ 4.0, 5.0, 6.0 };
            var c = new List<double> { 7.0, 8.0, 9.0 };

            return new List<IList<double>> { a, b, c };
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

        private static IList<IList<Vector>> TestVectorResults()
        {
            var x = Vector.ByCoordinates(1, 0, 0);
            var y = Vector.ByCoordinates(0, 1, 0);
            var z = Vector.ByCoordinates(0, 0, 1);

            var a = new List<Vector> {x,y,z};
            var b = new List<Vector> {x,y,z};
            var c = new List<Vector> {x,y,z};

            return new List<IList<Vector>> { a, b, c };
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
