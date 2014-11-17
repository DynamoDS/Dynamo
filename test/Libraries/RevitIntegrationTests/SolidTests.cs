using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    class SolidTests : SystemTest
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void BlendSolid()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(workingDirectory, @".\Solid\BlendSolid.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //ViewModel.Model.RunExpression();

            //var blendNode = ViewModel.Model.Nodes.First(x => x is CreateBlendGeometry);
            //var result = (Solid)VisualizationManager.GetDrawablesFromNode(blendNode).Values.First();
            //double volumeMin = 3700000.0;
            //double volumeMax = 3900000.0;
            //double actualVolume = result.Volume;
            //Assert.Greater(actualVolume, volumeMin);
            //Assert.Less(actualVolume, volumeMax);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Loft()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(workingDirectory, @".\Solid\Loft.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //ViewModel.Model.RunExpression();

            //var fec = new FilteredElementCollector( DocumentManager.Instance.CurrentDBDocument );
            //fec.OfClass(typeof(GenericForm));

            ////verify one loft created
            //int count = fec.ToElements().Count;
            //Assert.IsInstanceOf(typeof(Form), fec.ToElements().First());
            //Assert.AreEqual(1, count);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void RevolveSolid()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(workingDirectory, @".\Solid\RevolveSolid.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //ViewModel.Model.RunExpression();

            //var revolveNode = ViewModel.Model.Nodes.First(x => x is CreateRevolvedGeometry);
            //var result = (Solid)VisualizationManager.GetDrawablesFromNode(revolveNode).Values.First();
            //double volumeMin = 13300.0;
            //double volumeMax = 13550.0;
            //double actualVolume = result.Volume;
            //Assert.Greater(actualVolume, volumeMin);
            //Assert.Less(actualVolume, volumeMax);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void SolidBySkeleton()
        {
            //if (!DocumentManager.GetInstance().CurrentUIApplication.Application.VersionNumber.Contains("2013") &&
            //                 DocumentManager.GetInstance().CurrentUIApplication.Application.VersionName.Contains("Vasari"))
            //{
            //    var model = ViewModel.Model;

            //    string samplePath = Path.Combine(workingDirectory, @".\Solid\SolidBySkeleton.dyn");
            //    string testPath = Path.GetFullPath(samplePath);

            //    model.Open(testPath);
            //    ViewModel.Model.RunExpression();

            //    var skeletonNode = ViewModel.Model.Nodes.First(x => x is SkinCurveLoops);
            //    var result = (Solid)VisualizationManager.GetDrawablesFromNode(skeletonNode).Values.First();
            //    double volumeMin = 82500.0;
            //    double volumeMax = 84500.0;
            //    double actualVolume = result.Volume;
            //    Assert.Greater(actualVolume, volumeMin);
            //    Assert.Less(actualVolume, volumeMax);
            //}
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void SweepToMakeSolid()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(workingDirectory, @".\Solid\SweepToMakeSolid.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //ViewModel.Model.RunExpression();

            //var sweepNode = ViewModel.Model.Nodes.First(x => x is CreateSweptGeometry);
            //var result = (Solid)VisualizationManager.GetDrawablesFromNode(sweepNode).Values.First();
            //double volumeMin = 11800.0;
            //double volumeMax = 12150.0;
            //double actualVolume = result.Volume;
            //Assert.Greater(actualVolume, volumeMin);
            //Assert.Less(actualVolume, volumeMax);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void SweptBlend()
        {
            //var model = ViewModel.Model;
            //System.Reflection.Assembly revitAPIAssembly = System.Reflection.Assembly.GetAssembly(typeof(GenericForm));
            //var FreeFormType = revitAPIAssembly.GetType("Autodesk.Revit.DB.FreeFormElement", false);
            //if (FreeFormType == null)
            //    Assert.Inconclusive("FreeFormType not available.");

            //string samplePath = Path.Combine(workingDirectory, @".\Solid\SweptBlend.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //ViewModel.Model.RunExpression();

            //var fec = new FilteredElementCollector( DocumentManager.Instance.CurrentDBDocument );
            //fec.OfClass(typeof(GenericForm));

            ////verify one loft created
            //int count = fec.ToElements().Count;

            //Assert.IsInstanceOf(FreeFormType, fec.ToElements().First());
            //Assert.AreEqual(1, count);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void BoxByCenterAndDimensions()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(workingDirectory, @".\Solid\BoxByCenterAndDimensions.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.OpenWorkspace(testPath);
            //Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void BoxByTwoCorners()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(workingDirectory, @".\Solid\BoxByTwoCorners.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.OpenWorkspace(testPath);
            //Assert.DoesNotThrow(() =>  ViewModel.Model.RunExpression());
        }
    }
}
