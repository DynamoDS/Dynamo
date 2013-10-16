using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using NUnit.Framework;
using Transaction = Autodesk.Revit.DB.Transaction;

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
            using (var trans = new Transaction(RevitData.Document.Document, "CreateAndDeleteAreReferencePoint"))
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

                RevitData.Document.Document.Delete(rp);
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

        [Test]
        public void XYZFromReferencePoint()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\XYZFromReferencePoint.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            ReferencePoint rp;
            using (_trans = new Transaction(dynRevitSettings.Doc.Document))
            {
                _trans.Start("Create a reference point.");

                rp = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());

                _trans.Commit();

            }
            FSharpList<FScheme.Value> args = FSharpList<FScheme.Value>.Empty;
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(rp), args);

            //find the XYZFromReferencePoint node
            var node = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is XyzFromReferencePoint).First();

            FScheme.Value v = ((NodeWithOneOutput)node).Evaluate(args);
            Assert.IsInstanceOf(typeof(XYZ), ((FScheme.Value.Container)v).Item);
        }
    }
}
