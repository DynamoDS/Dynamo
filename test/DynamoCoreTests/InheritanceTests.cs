using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class InheritanceTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("Builtin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FFITarget.dll");

            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestInheritanceNotDerivedFromClassA()
        {
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceA.dyn");
            RunModel(openPath);
            AssertPreviewValue("1115abe3-eddb-4f18-9aae-01f34a8fe0e9", 2);
            AssertPreviewValue("c8b71784-3570-40fb-86d4-19437963d263", null);
        }

        [Test]
        public void TestInheritanceDerivedFromInterfaceA()
        {
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceB.dyn");
            RunModel(openPath);
            AssertPreviewValue("976be3c4-96c7-49eb-b309-d2c606b7a0ac", 1);
            AssertPreviewValue("db85e13a-ff8a-493a-a77f-04b0319e68cc", 1);
        }

        [Test]
        public void TestInheritanceDerivedFromInterfaceAWithReplication()
        {
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceC.dyn");
            RunModel(openPath);
            AssertPreviewValue("21ba383c-cbb9-426c-a688-703325d6c96f", new int[]{ 1 });
        }

        [Test]
        public void TestInheritanceHidesMethodFromClassA()
        {
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceD.dyn");
            RunModel(openPath);
            AssertPreviewValue("68d8ddfe-4e39-4450-b872-58fc30a78997", 3);
            AssertPreviewValue("0089e5a4-ed09-414c-9657-4312d9ad0ed5", 3);
        }

        [Test]
        public void TestInheritanceHidesMethodFromClassAInverse()
        {
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceE.dyn");
            RunModel(openPath);
            AssertPreviewValue("968031ae-975f-4095-a513-64d95d65e16a", 0);
            AssertPreviewValue("c09cde58-3e62-4adc-821d-3885ef769410", 0);
        }

        [Test]
        public void TestInheritanceOverridesMethodFromClassA()
        {
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceF.dyn");
            RunModel(openPath);
            AssertPreviewValue("4e62c57d-14d3-43b2-8737-f7b221192b4b", 100);
            AssertPreviewValue("165fe6fa-412a-4031-ab7f-3b19c3add6cf", 99);
            AssertPreviewValue("b54994bc-6ec4-4a5d-8abc-10c6045879d9", 99);
            AssertPreviewValue("08793bd5-0b8f-49bc-af70-72182b718a15", 100);
            AssertPreviewValue("2eecd234-ee7b-4f7f-a290-315e61e4ed86", 0);
            AssertPreviewValue("94ec01cd-ba19-4ae2-a268-7592db1eca20", 3);
            AssertPreviewValue("7740d481-c057-4171-a51f-2d8d05168a0e", 3);
            AssertPreviewValue("b0f78fb9-f5c4-40ac-b40a-a8da606d5f77", 0);
        }

        [Test, Category("Failure")]
        public void TestStaticMethodFromBothClasses()
        {
            // TODO: This causes a Crash! Tracked here: https://jira.autodesk.com/browse/QNTM-3648
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceG.dyn");
            RunModel(openPath);
            AssertPreviewValue("ca0398e0-f871-4b5f-a06d-e9f6b5963ed3", 23);
            AssertPreviewValue("49a8b4d4-b184-4892-8d88-476753d603f7", 234);
            
        }
    }
}
