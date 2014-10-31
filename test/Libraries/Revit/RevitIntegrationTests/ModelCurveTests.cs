using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    public class ModelCurveTests : SystemTest
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void ModelCurve()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(workingDirectory, @".\ModelCurve\ModelCurve.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //ViewModel.Model.RunExpression();

            //var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            //fec.OfClass(typeof(CurveElement));

            ////verify five model curves created
            //int count = fec.ToElements().Count;
            //Assert.IsInstanceOf(typeof(ModelCurve), fec.ToElements().First());
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
