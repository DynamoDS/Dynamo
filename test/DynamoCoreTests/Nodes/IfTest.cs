using System;
using System.Collections.Generic;
using System.IO;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using ProtoCore.AST.ImperativeAST;

namespace Dynamo.Tests
{
    [TestFixture]
    class IfTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        string testFolder { get { return Path.Combine(TestDirectory, "core", "logic", "conditional"); } }

        [Test]
        public void TestIFBasic()
        {
            string testFilePath = Path.Combine(testFolder, "testIfBasic.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("7d6e8c70-3abf-4fc4-864e-948f548e7ba2", 5.0);
            AssertPreviewValue("d5f5336d-3569-4a88-9a59-5538d6914037", new object[] { 1.0, 1.0, 1.0, 1.0, 1, 2, 3}); 
        }

        [Test]
        public void TestIfAsFunctionObject()
        {
            string testFilePath = Path.Combine(testFolder, "testIFAsFunctionObject.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("82a84012-1c28-4fe1-a38e-4c751e5a2077", new object[] {43, 144, 144, 43});
            AssertPreviewValue("ab4e17e1-0065-441a-97f2-9210d968a9ee", new object[] {1, 2, 3, 4});
        }

        [Test]
        public void TestIfInCustomNode1()
        {
            string testFilePath = Path.Combine(testFolder, "testIfInCustomNode1.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("bb5928fe-56ac-43e1-b3d4-96ea3ee4580f", new object[] { 1, 1, 1, 1, 1, 2, 3 });
        }


        [Test]
        public void TestIfInCustomNode2()
        {
            string testFilePath = Path.Combine(testFolder, "testIFInCustomNode2.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("2569020c-9952-46c5-8962-02bbf8c9f027", 1);
            AssertPreviewValue("4cda88e3-b54e-46c1-9c80-2647de6e3468", 5);
        }


        [Test]
        public void TestScopeIfForFactorial()
        {
            string testFilePath = Path.Combine(testFolder, "callFactorial.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("d70fab7e-7a2c-495e-a301-0b0797d86118", 720);
        }

        [Test]
        [Category("SmokeTest")]
        public void TestScopeIfForPreview()
        {
            string testFilePath = Path.Combine(testFolder, "testScopeIf.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("9fe8e82f-760d-43a6-90b2-5f9c252139d7", 42);
            AssertPreviewValue("23a03082-5807-4e44-9a3d-2d1eec4a914c", 42);
        }

        [Test]
        [Category("SmokeTest")] 
        public void TestIfNodeForPreview()
        {
            string testFilePath = Path.Combine(testFolder, "testNewIf.dyn");
            OpenModel(testFilePath);
            BoolSelector boolSelectorNode = (BoolSelector) this.CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("886a464b-9b2b-4e66-a033-c18d0753c2cf");

            boolSelectorNode.Value = true;
            AssertPreviewValue("886a464b-9b2b-4e66-a033-c18d0753c2cf", true);
            AssertPreviewValue("f945529e-62e3-4c7a-b07a-0b18d7449f9b", new object[] { "a1", "b1", "c1" });

            boolSelectorNode.Value = false;
            AssertPreviewValue("886a464b-9b2b-4e66-a033-c18d0753c2cf", false);
            AssertPreviewValue("f945529e-62e3-4c7a-b07a-0b18d7449f9b", new object[] { new object[] { "a2" }, "b2" });

            /* Test partially connected 'If' node.
             * The 'Function Apply' node has the following inputs:
             *              function: partially connected 'If' node whose truevalue is [1,[2],3] and falsevalue is [4,5].
             *              argument0: [true,false]
             *              
             * This 'Function Apply' node will call the partially connected "If" function 2 times. One for true and the other one for false.
             * So the output should be [[1,[2],3], [4,5]]
            */
            AssertPreviewValue("d38be78d-1d6d-4c4d-a67f-e16208f5feb6", new object[] { new object[] { 1, new object[] { 2 }, 3 }, new object[] { 4, 5 } });
        }

        [Test]
        [Category("SmokeTest")]
        public void TestIfNodeLacingOptions()
        {
            string testFilePath = Path.Combine(testFolder, "testNewIf.dyn");
            OpenModel(testFilePath);

            /* IF node inputs:
            *   true value: ["a1","b1","c1"];
            *   false value: [["a2"],"b2"];
            */

            BoolSelector boolSelectorNode = (BoolSelector)this.CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("886a464b-9b2b-4e66-a033-c18d0753c2cf");

            /* ArgumentLacing: Auto
             * 
             * expected output: if test value is true, output will be ["a1","b1","c1"]
             *                  if test value is false, output will be  [["a2"],"b2"]
             */

            NodeModel ifNode = this.CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("f945529e-62e3-4c7a-b07a-0b18d7449f9b");
            Assert.AreEqual(LacingStrategy.Auto, ifNode.ArgumentLacing);
            boolSelectorNode.Value = true;
            AssertPreviewValue("f945529e-62e3-4c7a-b07a-0b18d7449f9b", new object[] { "a1", "b1", "c1" });
            boolSelectorNode.Value = false;
            AssertPreviewValue("f945529e-62e3-4c7a-b07a-0b18d7449f9b", new object[] { new object[] { "a2" }, "b2" });

            /* ArgumentLacing: Longest
             * 
             * expected output: if test value is true, output will be ["a1","b1","c1"]
             *                  if test value is false, output will be  [["a2"],"b2","b2"]
             */
            CurrentDynamoModel.CurrentWorkspace.UpdateModelValue(new List<Guid> { ifNode.GUID }, "ArgumentLacing", "Longest");
            Assert.AreEqual(LacingStrategy.Longest, ifNode.ArgumentLacing);
            boolSelectorNode.Value = true;
            AssertPreviewValue("f945529e-62e3-4c7a-b07a-0b18d7449f9b", new object[] { "a1", "b1", "c1" });
            boolSelectorNode.Value = false;
            AssertPreviewValue("f945529e-62e3-4c7a-b07a-0b18d7449f9b", new object[] { new object[] { "a2" }, "b2", "b2"});


            /* ArgumentLacing: Shortest
             * 
             * expected output: if test value is true, output will be ["a1"]
             *                  if test value is false, output will be  [["a2"]]
             */
            CurrentDynamoModel.CurrentWorkspace.UpdateModelValue(new List<Guid> { ifNode.GUID }, "ArgumentLacing", "Shortest");
            Assert.AreEqual(LacingStrategy.Shortest, ifNode.ArgumentLacing);
            boolSelectorNode.Value = true;
            AssertPreviewValue("f945529e-62e3-4c7a-b07a-0b18d7449f9b", new object[] { "a1" });
            boolSelectorNode.Value = false;
            AssertPreviewValue("f945529e-62e3-4c7a-b07a-0b18d7449f9b", new object[] { new object[] { "a2" }});
        }

        [Test]
        [Category("SmokeTest")]
        public void TestIfNodeListAtLevel()
        {
            string testFilePath = Path.Combine(testFolder, "testNewIf.dyn");
            OpenModel(testFilePath);
            BoolSelector boolSelectorNode = (BoolSelector)this.CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("886a464b-9b2b-4e66-a033-c18d0753c2cf");

            /* IF node inputs:
             *   true value: [1,[2],3]; List level set at L3
             *   false value: [4,5];    List level set at L3
             * 
             * expected output: if test value is true, output will be [[ 1, [2], 3]]
             *                  if test value is false, output will be [[[ 4 , 5 ]]]
             */

            boolSelectorNode.Value = true;
            AssertPreviewValue("699f9236-9fb0-4b91-a6a3-fb5dd23ed70b", new object[] { new object[] { 1, new object[] { 2 }, 3 } });

            boolSelectorNode.Value = false;
            AssertPreviewValue("699f9236-9fb0-4b91-a6a3-fb5dd23ed70b", new object[] { new object[] { new object[] { 4, 5 } } });
        }
    }
}