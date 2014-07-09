using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;
using RTF.Framework;
using Transaction = Autodesk.Revit.DB.Transaction;

namespace Dynamo.Tests
{
    [TestFixture]
    public class XYZTests :DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void XYZFromReferencePoint()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //string samplePath = Path.Combine(_testPath, @".\XYZ\XYZFromReferencePoint.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //ReferencePoint rp;
            //using (_trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document))
            //{
            //    _trans.Start("Create a reference point.");

            //    rp = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ());

            //    _trans.Commit();

            //}
            //FSharpList<FScheme.Value> args = FSharpList<FScheme.Value>.Empty;
            //args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(rp), args);

            ////find the XYZFromReferencePoint node
            //var node = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is XyzFromReferencePoint).First();

            //FScheme.Value v = ((NodeWithOneOutput)node).Evaluate(args);
            //Assert.IsInstanceOf(typeof(XYZ), ((FScheme.Value.Container)v).Item);

            Assert.Inconclusive("Porting : XYZ");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void XYZByPolar()
        {
            string samplePath = Path.Combine(_testPath, @".\XYZ\XYZByPolar.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void XYZBySphericalCoordinates()
        {
            string samplePath = Path.Combine(_testPath, @".\XYZ\XYZBySphericalCoordinates.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void XYZToPolarCoordinates()
        {
            string samplePath = Path.Combine(_testPath, @".\XYZ\XYZToPolarCoordinates.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void XYZToSphericalCoordinates()
        {
            string samplePath = Path.Combine(_testPath, @".\XYZ\XYZToSphericalCoordinates.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }
    }
}
