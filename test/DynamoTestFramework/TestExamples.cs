using System;
using NUnit.Framework;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Dynamo.Tests
{
    [TestFixture]
    public class TestExamples
    {
        [Test]
        public void TestOne()
        {
            using (var t = new Transaction(DocumentManager.Instance.CurrentDBDocument))
            {
                if (t.Start("Test one.") == TransactionStatus.Started)
                {
                    //create a reference point
                    var pt = DocumentManager.Instance.CurrentDBDocument.FamilyCreate.NewReferencePoint(new XYZ(5, 5, 5));

                    if (t.Commit() != TransactionStatus.Committed)
                    {
                        t.RollBack();
                    }
                }
                else
                {
                    throw new Exception("Transaction could not be started.");
                }
            }

            //verify that the point was created
            var collector = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            collector.OfClass(typeof (ReferencePoint));

            Assert.AreEqual(1, collector.ToElements().Count);
        }

        [Test]
        public void TestTwo()
        {
            Assert.Inconclusive("This is inconclusive.");
        }

        [Test]
        public void TestThree()
        {
            //this will pass.
            Assert.AreEqual(0,0);
        } 
    }

    [TestFixture]
    public class TestExampleFixture
    {
        [Test]
        public void TestA()
        {
            Assert.Fail("This test fails");
        }

        [Test]
        public void TestB()
        {
            Assert.AreEqual(0, 0);
        }

        [Test]
        public void TestC()
        {
            Assert.Inconclusive("This is inconclusive");
        }
    }
}
