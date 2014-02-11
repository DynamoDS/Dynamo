using System.IO;
using Dynamo.Core;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class PythonTests : DynamoRevitUnitTestBase
    {
        [Test]
        public void CanAddTwoInputsWithPython()
        {
            var model = DynamoSettings.Controller.DynamoModel;

            string graph = Path.Combine(_testPath, @".\Python\Python_add.dyn");
            string testPath = Path.GetFullPath(graph);

            model.Open(testPath);
            Assert.DoesNotThrow(() => DynamoSettings.Controller.RunExpression(true));
        }

        [Test]
        public void ConnectTwoPointArraysWithoutPython()
        {
            //var model = DynamoSettings.Controller.DynamoModel;

            //string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\connect two point arrays without python.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //DynamoSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            //DynamoSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
            Assert.Inconclusive("Python examples do not play well with testing.");
        }

        [Test]
        public void ConnectTwoPointArrays()
        {
            //var model = DynamoSettings.Controller.DynamoModel;

            //string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\connect two point arrays.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //DynamoSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            //DynamoSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
            Assert.Inconclusive("Python examples do not play well with testing.");
        }

        [Test]
        public void CreateSineWaveFromSelectedCurve()
        {
            //var model = DynamoSettings.Controller.DynamoModel;

            //string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\create sine wave from selected curve.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //CurveByPoints cbp = null;
            //using (_trans = new Transaction(dynRevitSettings.Doc.Document))
            //{
            //    _trans.Start("Create reference points for testing Python node.");

            //    ReferencePoint p1 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());
            //    ReferencePoint p2 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(0,10,0));
            //    ReferencePoint p3 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(0,20,0));
            //    ReferencePointArray ptArr = new ReferencePointArray();
            //    ptArr.Append(p1);
            //    ptArr.Append(p2);
            //    ptArr.Append(p3);

            //    cbp = dynRevitSettings.Doc.Document.FamilyCreate.NewCurveByPoints(ptArr);

            //    _trans.Commit();
            //}

            //Assert.IsNotNull(cbp);

            //DynamoSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            //var selectionNode = DynamoSettings.Controller.DynamoModel.Nodes.Where(x => x is dynCurvesBySelection).First();
            //((dynCurvesBySelection)selectionNode).SelectedElement = cbp;

            ////delete the transaction node when testing
            ////var transNode = DynamoSettings.Controller.DynamoModel.Nodes.Where(x => x is dynTransaction).First();
            ////dynRevitSettings.Controller.RunCommand(vm.DeleteCommand, transNode);

            //DynamoSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);

            Assert.Inconclusive("Python examples do not play well with testing.");
        }

        //[Test]
        //public void CreateSineWaveFromSelectedPoints()
        //{
        //    var model = DynamoSettings.Controller.DynamoModel;

        //    string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\create sine wave from selected points.dyn");
        //    string testPath = Path.GetFullPath(samplePath);

        //    ReferencePoint p1 = null;
        //    ReferencePoint p2 = null;

        //    using (_trans = new Transaction(dynRevitSettings.Doc.Document))
        //    {
        //        _trans.Start("Create reference points for testing python node.");

        //        p1 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());
        //        p2 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(0, 10, 0));

        //        _trans.Commit();
        //    }

        //    DynamoSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);

        //    var selectionNodes = DynamoSettings.Controller.DynamoModel.Nodes.Where(x => x is dynPointBySelection);
        //    Assert.AreEqual(2, selectionNodes.Count());

        //    ((dynPointBySelection)selectionNodes.ElementAt(0)).SelectedElement = p1;
        //    ((dynPointBySelection)selectionNodes.ElementAt(1)).SelectedElement = p2;

        //    //delete the transaction node when testing
        //    //var transNode = DynamoSettings.Controller.DynamoModel.Nodes.Where(x => x is dynTransaction).First();
        //    //dynRevitSettings.Controller.RunCommand(vm.DeleteCommand, transNode);

        //    DynamoSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        //}
    }
}
