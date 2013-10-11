using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class PointTests :DynamoRevitUnitTestBase
    {
        [Test]
        public void SanitCheck()
        {
            Assert.Equals(0, 0);
        }

        [Test]
        public void CanCreateAndDeleteAReferencePoint()
        {
            using (var trans = new Transaction(dynRevitSettings.Doc.Document, "CreateAndDeleteAreReferencePoint"))
            {
                trans.Start();

                FailureHandlingOptions fails = trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                trans.SetFailureHandlingOptions(fails);

                ReferencePoint rp = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());

                //make a filter for reference points.
                ElementClassFilter ef = new ElementClassFilter(typeof(ReferencePoint));
                FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
                fec.WherePasses(ef);
                Assert.AreEqual(1, fec.ToElements().Count());

                dynRevitSettings.Doc.Document.Delete(rp);
                trans.Commit();
            }
        }

        [Test]
        public void ReferencePoint()
        {
            var model = dynSettings.Controller.DynamoModel;

            string testPath = Path.Combine(_testPath, "ReferencePoint.dyn");
            model.Open(testPath);
            Assert.AreEqual(3, dynSettings.Controller.DynamoModel.Nodes.Count);

            dynSettings.Controller.RunExpression(true);
        }
    }
}
