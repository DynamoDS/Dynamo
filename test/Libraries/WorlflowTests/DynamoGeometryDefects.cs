using System.IO;
using NUnit.Framework;
using Dynamo.Utilities;
using Dynamo.Models;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;

namespace Dynamo.Tests
{
    [TestFixture]
    class DynamoGeometryDefects : DSEvaluationUnitTest
    {
        [Test]
        public void Defect_MAGN_3714()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3714

            DynamoModel model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), @"core\DynamoDefects\Defect_MAGN_3714.dyn");

            RunModel(openPath);

            var curveByDivide = "a78486e2-66aa-43e4-a261-34ae3b079307";
            AssertPreviewCount(curveByDivide, 5);

            // output will be 5 Curves, so putting verification for all Curves
            for (int i = 0; i <= 4; i++)
            {
                var curve = GetPreviewValueAtIndex(curveByDivide, i) as Curve;
                Assert.IsNotNull(curve);
            }
        }
    }
}
