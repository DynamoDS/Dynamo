using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;
using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class LevelTests:DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\Level\Level.rvt")]
        public void Level()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(_testPath, @".\Level\Level.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            ////ensure that the level count is the same
            //var levelColl = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            //levelColl.OfClass(typeof(Autodesk.Revit.DB.Level));
            //Assert.AreEqual(levelColl.ToElements().Count(), 6);

            ////change the number and run again
            //var numNode = (DoubleInput)ViewModel.Model.DynamoModel.Nodes.First(x => x is DoubleInput);
            //numNode.Value = "0..20..2";
            //Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            ////ensure that the level count is the same
            //levelColl = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            //levelColl.OfClass(typeof(Autodesk.Revit.DB.Level));
            //Assert.AreEqual(levelColl.ToElements().Count(), 11);

            Assert.Inconclusive("Porting : DoubleInput");
        }
    }
}
