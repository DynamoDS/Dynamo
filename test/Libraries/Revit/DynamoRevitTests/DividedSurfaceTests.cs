using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;
using DividedSurface = Autodesk.Revit.DB.DividedSurface;

namespace Dynamo.Tests
{
    [TestFixture]
    class DividedSurfaceTests:DynamoRevitUnitTestBase
    {
        [Test]
        public void DividedSurface()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\DividedSurface\DividedSurface.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //var shellPath = Path.Combine(_testPath, "shell.rfa");
            //SwapCurrentModel(shellPath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentUIDocument.Document);
            fec.OfClass(typeof(DividedSurface));

            //did it create a divided surface?
            Assert.AreEqual(1, fec.ToElements().Count());

            var ds = (DividedSurface)fec.ToElements()[0];
            Assert.AreEqual(5, ds.USpacingRule.Number);
            Assert.AreEqual(5, ds.VSpacingRule.Number);

            //can we change the number of divisions
            var numNode = dynSettings.Controller.DynamoModel.Nodes.OfType<DoubleInput>().First();
            numNode.Value = "10";
            dynSettings.Controller.RunExpression(true);

            //did it create a divided surface?
            Assert.AreEqual(10, ds.USpacingRule.Number);
            Assert.AreEqual(10, ds.VSpacingRule.Number);

            //does it throw an error when we try to set a negative number of divisions
            numNode.Value = "-5";
            Assert.Throws(typeof(AssertionException),
                          () => dynSettings.Controller.RunExpression(true));
        }
    }
}
