using System.Collections.Generic;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class DefaultArgumentMigrationTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FFITarget.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestDisableDefaultArgumentOfFunctionObject()
        {
            RunModel(@"core\default_values\defaultArgumentDisabled.dyn");
            var sphereNode1 =
                CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("60d00bbc-4c16-4d5c-8a7e-54fcc303a783");

            Assert.IsTrue(!sphereNode1.InPorts[0].UsingDefaultValue);

            var sphereNode2 =
                CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("e95d0530-7cd6-49b1-becb-26971bcbf147");

            Assert.IsTrue(!sphereNode2.InPorts[1].UsingDefaultValue);

            var applyFunctionNode =
                CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("d0abacc3-d00a-424f-b816-e2fc1620e475");

            Assert.IsTrue(!applyFunctionNode.InPorts[0].UsingDefaultValue);
        }

        [Test]
        public void TestEnableDefaultArgumentForAddedParamComplexType()
        {
            RunModel(@"core\default_values\defaultArgumentAdded1.dyn");

            var dummyLineNode1 =
                CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("17ef637c-ea48-4d40-b8e9-54bafd182708");
            // The saved node only has 1 input param but after loading the node should be migrated to the new
            // version with 2 input params.
            Assert.AreEqual(dummyLineNode1.InPorts.Count, 2);

            // Since the second param has default argument, UsingDefaultArgument should be enabled.
            Assert.IsTrue(dummyLineNode1.InPorts[1].UsingDefaultValue);

            // Migration from @Point to @Point,Vector with Vector having default value.
            var dummyLineNode2 = 
                CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("1fa06b8d-84eb-4090-961f-1af0e8f6ec9d");

            Assert.AreEqual(dummyLineNode2.InPorts.Count, 2);

            Assert.IsTrue(dummyLineNode2.InPorts[1].UsingDefaultValue);
        }

        [Test]
        public void TestEnableDefaultArgumentForAddedParamSimpleType()
        {
            RunModel(@"core\default_values\defaultArgumentAdded2.dyn");

            var dummyNode1 =
                CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("48339420-2ad4-490e-803d-4e3a7c708fb6");
            // The saved node only has 2 input params (Foobar@int,int) but after loading the node should be
            // migrated to the new version with 3 input params (Foobar@int,int,bool) instead of another overload
            // with 2 input params (Foobar@double,double)
            Assert.AreEqual(dummyNode1.InPorts.Count, 3);

            // Since the 3rd param has default argument, UsingDefaultArgument should be enabled.
            Assert.IsTrue(dummyNode1.InPorts[2].UsingDefaultValue);


            // Migration from Barfoo@int to Barfoo@int,double,double with the last two having default values
            var dummyNode2 = 
                CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("cd8508ed-04ac-4009-a169-abea517650e6");
            Assert.AreEqual(dummyNode2.InPorts.Count, 3);
            Assert.IsTrue(dummyNode2.InPorts[1].UsingDefaultValue);
            Assert.IsTrue(dummyNode2.InPorts[2].UsingDefaultValue);
        }

        [Test]
        public void TestEnableDefaultArgumentForAddedParamInstanceMethod()
        {
            RunModel(@"core\default_values\defaultArgumentAdded3.dyn");

            var dummyNode1 =
                CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("1ef1f522-3ab4-43a1-9ab4-43cf46b973cd");
            // The saved instance node only has 1 input param (InstanceFooBar@int) but after loading the node should be
            // migrated to the new version with 2 input params (InstanceFooBar@int,bool) 
            Assert.AreEqual(dummyNode1.InPorts.Count, 3);

            // Since the 3rd param has default argument, UsingDefaultArgument should be enabled.
            Assert.IsTrue(dummyNode1.InPorts[2].UsingDefaultValue);
        }
    }
}
