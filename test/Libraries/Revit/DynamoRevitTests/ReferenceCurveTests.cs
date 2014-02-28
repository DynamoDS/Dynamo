using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;

namespace Dynamo.Tests
{
    [TestFixture]
    public class ReferenceCurveTests : DynamoRevitUnitTestBase
    {
        [Test]
        public void ReferenceCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\ReferenceCurve\ReferenceCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentUIDocument.Document);
            fec.OfClass(typeof(CurveElement));

            //verify five model curves created
            int count = fec.ToElements().Count;
            Assert.IsInstanceOf(typeof(ModelCurve), fec.ToElements().First());
            Assert.IsTrue(((ModelCurve)fec.ToElements().First()).IsReferenceLine);
            Assert.AreEqual(5, count);

            ElementId id = fec.ToElements().First().Id;

            //update any number node and verify 
            //that the element gets updated not recreated
            var doubleNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is DoubleInput);
            var node = doubleNodes.First() as DoubleInput;

            Assert.IsNotNull(node);

            node.Value = node.Value + .1;
            dynSettings.Controller.RunExpression(true);
            Assert.AreEqual(5, fec.ToElements().Count);
        }
    }
}
