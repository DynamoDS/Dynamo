using System.IO;

using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    public class XYZTests : SystemTest
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void XYZFromReferencePoint()
        {
            //var model = ViewModel.Model;

            //string samplePath = Path.Combine(workingDirectory, @".\XYZ\XYZFromReferencePoint.dyn");
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
            //var node = ViewModel.Model.Nodes.Where(x => x is XyzFromReferencePoint).First();

            //FScheme.Value v = ((NodeWithOneOutput)node).Evaluate(args);
            //Assert.IsInstanceOf(typeof(XYZ), ((FScheme.Value.Container)v).Item);

            Assert.Inconclusive("Porting : XYZ");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void XYZByPolar()
        {
            string samplePath = Path.Combine(workingDirectory, @".\XYZ\XYZByPolar.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void XYZBySphericalCoordinates()
        {
            string samplePath = Path.Combine(workingDirectory, @".\XYZ\XYZBySphericalCoordinates.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void XYZToPolarCoordinates()
        {
            string samplePath = Path.Combine(workingDirectory, @".\XYZ\XYZToPolarCoordinates.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void XYZToSphericalCoordinates()
        {
            string samplePath = Path.Combine(workingDirectory, @".\XYZ\XYZToSphericalCoordinates.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }
    }
}
