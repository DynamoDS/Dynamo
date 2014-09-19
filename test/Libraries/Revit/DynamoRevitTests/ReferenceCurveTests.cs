using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;
using RTF.Framework;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;

namespace Dynamo.Tests
{
    [TestFixture]
    public class ReferenceCurveTests : DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void ReferenceCurve()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(_testPath, @".\ReferenceCurve\ReferenceCurve.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //ViewModel.Model.RunExpression();

            //FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            //fec.OfClass(typeof(CurveElement));

            ////verify five model curves created
            //int count = fec.ToElements().Count;
            //Assert.IsInstanceOf(typeof(ModelCurve), fec.ToElements().First());
            //Assert.IsTrue(((ModelCurve)fec.ToElements().First()).IsReferenceLine);
            //Assert.AreEqual(5, count);

            //ElementId id = fec.ToElements().First().Id;

            ////update any number node and verify 
            ////that the element gets updated not recreated
            //var doubleNodes = ViewModel.Model.Nodes.Where(x => x is DoubleInput);
            //var node = doubleNodes.First() as DoubleInput;

            //Assert.IsNotNull(node);

            //node.Value = node.Value + .1;
            //ViewModel.Model.RunExpression();
            //Assert.AreEqual(5, fec.ToElements().Count);

            Assert.Inconclusive("Porting : DoubleInput");
        }
    }
}
