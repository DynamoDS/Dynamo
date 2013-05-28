using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.IO;
using System.Reflection;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

using Dynamo;
using Dynamo.Applications.Properties;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.FSchemeInterop;

using NUnit.Core;
using NUnit.Framework;

using Microsoft.Practices.Prism;

namespace DynamoRevitTests
{
    [TestFixture]
    internal class DynamoRevitTests
    {
        Transaction _trans;
        List<Element> _elements = new List<Element>();
        string _testPath;
        string _samplesPath;

        [TestFixtureSetUp]
        public void InitFixture()
        {
            
        }

        [TestFixtureTearDown]
        public void CleanupFixture()
        {
        }

        [SetUp]
        //Called before each test method
        public void Init()
        {
            //get the test path
            FileInfo fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            string testsLoc = Path.Combine(assDir, @"..\..\test\revit\");
            _testPath = Path.GetFullPath(testsLoc);

            //get the samples path
            string samplesLoc = Path.Combine(assDir, @"..\..\doc\distrib\Samples\");
            _samplesPath = Path.GetFullPath(samplesLoc);

        }

        [TearDown]
        //Called after each test method
        public void Cleanup()
        {
            _trans = null;
            using (_trans = new Transaction(dynRevitSettings.Doc.Document, "CreateAndDeleteAreReferencePoint"))
            {
                foreach (Element e in _elements)
                {
                    dynRevitSettings.Doc.Document.Delete(e);
                }
            }
        }

        [Test]
        public void CanCreateAndDeleteAReferencePoint()
        {
            using (_trans = _trans = new Transaction(dynRevitSettings.Doc.Document, "CreateAndDeleteAreReferencePoint"))
            {
                _trans.Start();

                FailureHandlingOptions fails = _trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                _trans.SetFailureHandlingOptions(fails);

                ReferencePoint rp = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());

                //make a filter for reference points.
                ElementClassFilter ef = new ElementClassFilter(typeof(ReferencePoint));
                FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
                fec.WherePasses(ef);
                Assert.AreEqual(1, fec.ToElements().Count());

                dynRevitSettings.Doc.Document.Delete(rp);
                _trans.Commit();
            }
        }

        //[Test]
        //public void ThrowsExceptionWithBadFileName()
        //{
        //    dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(@"blah.dyn");
        //    dynSettings.Controller.OnRunCompleted(this, true);
        //}

        [Test]
        public void CanOpenReferencePointTest()
        {
            string testPath = Path.Combine(_testPath, "ReferencePointTest.dyn");
            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.AreEqual(2, dynSettings.Controller.DynamoModel.Nodes.Count());

            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        //[Test]
        //public void CanOpenAndExecuteAllSamples()
        //{
        //    DirectoryInfo di = new DirectoryInfo(_samplesPath);
        //    OpenAllSamplesInDirectory(di);
        //}

        [Test]
        public void CreatePointSequenceSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\01 Create Point\create point_sequence.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void CreatePointEndSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\01 Create Point\create point - end.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void CreatePointSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\01 Create Point\create point.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void RefGridSlidersSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\02 Ref Grid Sliders\ref grid sliders.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void RefGridSlidersEndSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\02 Ref Grid Sliders\ref grid sliders - end.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }


        private static void OpenAllSamplesInDirectory(DirectoryInfo di)
        {
            foreach (FileInfo file in di.GetFiles())
            {
                if(file.Extension != ".dyn")
                    continue;
                dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(file.FullName);
                dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                OpenAllSamplesInDirectory(dir);
            }
        }
    }
}
