using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;

namespace Dynamo.Tests
{
    [TestFixture]
    public class ReferencePointTests : DynamoRevitUnitTestBase
    {
        [Test]
        public void CanCreateAndDeleteAReferencePoint()
        {
            using (var trans = new Transaction(DocumentManager.GetInstance().CurrentDBDocument, "CreateAndDeleteAreReferencePoint"))
            {
                trans.Start();

                FailureHandlingOptions fails = trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                trans.SetFailureHandlingOptions(fails);

                ReferencePoint rp = DocumentManager.GetInstance().CurrentUIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ());

                //make a filter for reference points.
                ElementClassFilter ef = new ElementClassFilter(typeof(ReferencePoint));
                FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentUIDocument.Document);
                fec.WherePasses(ef);
                Assert.AreEqual(1, fec.ToElements().Count());

                DocumentManager.GetInstance().CurrentDBDocument.Delete(rp);
                trans.Commit();
            }
        }

        [Test]
        public void ReferencePoint()
        {
            var model = dynSettings.Controller.DynamoModel;

            string testPath = Path.Combine(_testPath, @".\ReferencePoint\ReferencePoint.dyn");
            model.Open(testPath);
            Assert.AreEqual(3, dynSettings.Controller.DynamoModel.Nodes.Count);

            dynSettings.Controller.RunExpression(true);
        }
    }
}
