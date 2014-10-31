using System.IO;

using NUnit.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class PythonTests : SystemTest
    {
        [Test]
        public void CanAddTwoInputsWithPython()
        {
            string graph = Path.Combine(workingDirectory, @".\Python\Python_add.dyn");
            string testPath = Path.GetFullPath(graph);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        public void ConnectTwoPointArraysWithoutPython()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\connect two point arrays without python.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //ViewModel.OpenCommand.Execute(testPath);
            //ViewModel.RunExpressionCommand.Execute(true);
            Assert.Inconclusive("Python examples do not play well with testing.");
        }

        [Test]
        public void ConnectTwoPointArrays()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\connect two point arrays.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //ViewModel.OpenCommand.Execute(testPath);
            //ViewModel.RunExpressionCommand.Execute(true);
            Assert.Inconclusive("Python examples do not play well with testing.");
        }

        [Test]
        public void CreateSineWaveFromSelectedCurve()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\create sine wave from selected curve.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //CurveByPoints cbp = null;
            //using (_trans = new Transaction(DocumentManager.Instance.CurrentDBDocument))
            //{
            //    _trans.Start("Create reference points for testing Python node.");

            //var doc = DocumentManager.Instance.CurrentDBDocument;
            //    ReferencePoint p1 = doc.FamilyCreate.NewReferencePoint(new XYZ());
            //    ReferencePoint p2 = doc.FamilyCreate.NewReferencePoint(new XYZ(0,10,0));
            //    ReferencePoint p3 = doc.FamilyCreate.NewReferencePoint(new XYZ(0,20,0));
            //    ReferencePointArray ptArr = new ReferencePointArray();
            //    ptArr.Append(p1);
            //    ptArr.Append(p2);
            //    ptArr.Append(p3);

            //    cbp = doc.FamilyCreate.NewCurveByPoints(ptArr);

            //    _trans.Commit();
            //}

            //Assert.IsNotNull(cbp);

            //ViewModel.OpenCommand.Execute(testPath);

            //var selectionNode = ViewModel.Model.Nodes.Where(x => x is dynCurvesBySelection).First();
            //((dynCurvesBySelection)selectionNode).SelectedElement = cbp;

            ////delete the transaction node when testing
            ////var transNode = ViewModel.Model.Nodes.Where(x => x is dynTransaction).First();
            ////ViewModel.RunCommand(vm.DeleteCommand, transNode);

            //ViewModel.RunExpressionCommand.Execute(true);

            Assert.Inconclusive("Python examples do not play well with testing.");
        }

        //[Test]
        //public void CreateSineWaveFromSelectedPoints()
        //{
        //    var model = ViewModel.Model;

        //    string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\create sine wave from selected points.dyn");
        //    string testPath = Path.GetFullPath(samplePath);

        //    ReferencePoint p1 = null;
        //    ReferencePoint p2 = null;

        //    using (_trans = new Transaction( DocumentManager.Instance.CurrentDBDocument))
        //    {
        //        _trans.Start("Create reference points for testing python node.");
        //var doc = DocumentManager.Instance.CurrentDBDocument;
        //        p1 = doc.FamilyCreate.NewReferencePoint(new XYZ());
        //        p2 = doc.FamilyCreate.NewReferencePoint(new XYZ(0, 10, 0));

        //        _trans.Commit();
        //    }

        //    ViewModel.OpenCommand.Execute(testPath);

        //    var selectionNodes = ViewModel.Model.Nodes.Where(x => x is dynPointBySelection);
        //    Assert.AreEqual(2, selectionNodes.Count());

        //    ((dynPointBySelection)selectionNodes.ElementAt(0)).SelectedElement = p1;
        //    ((dynPointBySelection)selectionNodes.ElementAt(1)).SelectedElement = p2;

        //    //delete the transaction node when testing
        //    //var transNode = ViewModel.Model.Nodes.Where(x => x is dynTransaction).First();
        //    //ViewModel.RunCommand(vm.DeleteCommand, transNode);

        //    ViewModel.RunExpressionCommand.Execute(true);
        //}
    }
}
