using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

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

        [Test]
        public void ThrowsExceptionWithBadFileName()
        {
            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(@"blah.dyn");
            dynSettings.Controller.OnRunCompleted(this, true);
        }

        [Test]
        public void CanOpenReferencePointTest()
        {
            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(@"..\..\test\revit\ReferencePointTest.dyn");
            Assert.AreEqual(2, dynSettings.Controller.DynamoModel.Nodes.Count());

            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }
    }
}
