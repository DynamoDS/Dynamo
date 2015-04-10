using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Geometry;

using Dynamo.Tests;

using DynamoConversions;

using NUnit.Framework;

namespace GeometryUITests
{

    public class GeometryUITests : DSEvaluationViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestExportWithUnits()
        {
            string openPath = Path.Combine(
                TestDirectory,
                @"core\geometryui\export_units_one_cuboid.dyn");

            RunModel(openPath);

            var exportPreview = GetPreviewValue("71e5eea4-63ea-4c97-9d8d-aa9c8a2c420a") as string;

            Assert.IsNotNull(exportPreview);

            Assert.IsTrue(exportPreview.Contains("exported.sat"));
        }
    }
}
