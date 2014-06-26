using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;
using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class ReferencePointTests : DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanCreateAndDeleteAReferencePoint()
        {
            using (var trans = new Transaction(DocumentManager.Instance.CurrentDBDocument, "CreateAndDeleteAreReferencePoint"))
            {
                trans.Start();

                FailureHandlingOptions fails = trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                trans.SetFailureHandlingOptions(fails);

                ReferencePoint rp = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ());

                //make a filter for reference points.
                ElementClassFilter ef = new ElementClassFilter(typeof(ReferencePoint));
                FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
                fec.WherePasses(ef);
                Assert.AreEqual(1, fec.ToElements().Count());

                DocumentManager.Instance.CurrentDBDocument.Delete(rp);
                trans.Commit();
            }
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ReferencePoint()
        {
            string testPath = Path.Combine(_testPath, @".\ReferencePoint\ReferencePoint.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.AreEqual(3, dynSettings.Controller.DynamoModel.Nodes.Count);

            dynSettings.Controller.RunExpression();
        }
    }
}
