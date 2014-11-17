using System.IO;

using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class LevelTests : SystemTest
    {
        [Test]
        [TestModel(@".\Level\Level.rvt")]
        public void Level()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(workingDirectory, @".\Level\Level.dyn");
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

        [Test]
        [Category("IntegrationTests")]
        [TestModel(@".\empty.rfa")]
        public void CreateLevelsUsingAllLevelCreationNodes()
        {
            /* This Test Case serves as a perpose of Smoke Test For Level creation. 
             Curently it is marked as Integration Tests */

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Level\Levels.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            RunCurrentModel();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            var levelByElevationAndName = GetPreviewValue
                                    ("f004b19e-f67a-4422-8e4e-5fd4eeea4dff") as Revit.Elements.Level;
            Assert.IsNotNull(levelByElevationAndName);

            var levelByLevelAndOffset = GetPreviewValue
                                    ("bc16e986-6b1e-4e50-845c-970782509145") as Revit.Elements.Level;
            Assert.IsNotNull(levelByLevelAndOffset);

            var levelByLevelOffsetAndName = GetPreviewValue
                                    ("c3894b49-270a-4936-8d68-8ede789fe9f2") as Revit.Elements.Level;
            Assert.IsNotNull(levelByLevelOffsetAndName);

            var levelByElevation = GetPreviewValue
                                    ("eb310915-91b0-481e-ae45-3764207c8b95") as Revit.Elements.Level;
            Assert.IsNotNull(levelByElevation);

        }


    }
}
