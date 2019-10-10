using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Dynamo.Wpf.Views;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    //public delegate void CommandCallback(string commandTag);

    [TestFixture]
    public class CodeBlockNodeTests : RecordedUnitTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        //Run the CBN without an input to get the error message
        //Connect an input to the CBN and run
        //Change the value of the input and run 
        //  : the result will not be updated

        //Create CBN node with Math.Sin(x);
        //Execute
        //Create node with value 10 and attach to the CBN
        //Execute(InitialRun)
        //Update the value from 10 to 0
        //Execute(SecondRun)
        [Test, RequiresSTA]
        public void CodeBlockNode_ReassignInput()
        {
            RunCommandsFromFile("CodeBlockNode_ReassignInput.xml", (commandTag) =>
            {
                if (commandTag == "InitialRun")
                {
                    AssertPreviewValue("e31166a6-083f-4279-bd6b-8bfe57f7ee04", 0.173648);
                }
                else if (commandTag == "SecondRun")
                {
                    AssertPreviewValue("e31166a6-083f-4279-bd6b-8bfe57f7ee04", 0.000000);
                }
            });
        }

        //Assign a vale to a CodeBlockNode and run
        //Remove the input of this CBN by assign a literals to the CBN's text and run
        //Change the value of the literals and run 
        //  : the result will not be updated

        //Create CBN node with Math.Sin(x);
        //Create node with value 1 and attach to the CBN
        //Execute(InitialRun)
        //Disattach the two nodes and update the CBN to Math.Sin(2)
        //Execute(SecondRun)
        //Update the CBN to Math.Sin(3)
        //Execute(ThirdRun)
        [Test, RequiresSTA]
        public void CodeBlockNode_ReassignInput_2()
        {
            RunCommandsFromFile("CodeBlockNode_ReassignInput_2.xml", (commandTag) =>
            {
                if (commandTag == "InitialRun")
                {
                    AssertPreviewValue("f2b8dd8d-dcb2-4e50-bc9e-29fd1ecce84b", 0.017452);
                }
                else if (commandTag == "SecondRun")
                {
                    AssertPreviewValue("f2b8dd8d-dcb2-4e50-bc9e-29fd1ecce84b", 0.034899);
                }
                else if (commandTag == "ThirdRun")
                {
                    AssertPreviewValue("f2b8dd8d-dcb2-4e50-bc9e-29fd1ecce84b", 0.052336);
                }
            });
        }

        // Create a cyclic chain of three code block nodes, and verify that a
        // warning is shown on one of the cyclic nodes.
        // Reconnect a valid value to one of the chain items, and verify that the
        // warning is turned off and the values are evaluated properly.
        // Create another cyclic chain of two nodes, and verify the same behavior.
        //
        [Test, RequiresSTA, Category("Failure")]
        public void CodeBlockNode_ReassignCyclic()
        {
            RunCommandsFromFile("CodeBlockNode_ReassignCyclic.xml", (commandTag) =>
            {
                var nodeA = GetNode("2e0d1d7e-7ef3-4cf5-9884-93ac77697e5f") as NodeModel;
                var nodeB = GetNode("9699d07d-ec4e-48ad-9a3d-170154a4a106") as NodeModel;
                var nodeC = GetNode("73959903-fd79-4645-9b58-28fe88545f8b") as NodeModel;

                if (commandTag == "NormalThreeNodes")
                {
                    // Create four code block nodes [3;], [a;], [b;], [c;]
                    // Connect [3;]-[a;], connect [a;]-[b;], connect [b;]-[c;]

                    AssertPreviewValue("73959903-fd79-4645-9b58-28fe88545f8b", 3);
                }
                else if (commandTag == "CyclicThreeNodes")
                {
                    // Connect [c;]-[a;]

                    bool hasWarning = false;

                    if (nodeA.State == ElementState.Warning) hasWarning = true;
                    if (nodeB.State == ElementState.Warning) hasWarning = true;
                    if (nodeC.State == ElementState.Warning) hasWarning = true;

                    Assert.AreEqual(true, hasWarning);
                }
                else if (commandTag == "Recover")
                {
                    // Change the code block node [3;] into [4;]
                    // Connect [4;]-[c;]

                    bool hasWarning = false;

                    if (nodeA.State == ElementState.Warning) hasWarning = true;
                    if (nodeB.State == ElementState.Warning) hasWarning = true;
                    if (nodeC.State == ElementState.Warning) hasWarning = true;

                    Assert.AreEqual(false, hasWarning);
                    AssertPreviewValue("73959903-fd79-4645-9b58-28fe88545f8b", 4);
                }
                else if (commandTag == "CyclicTwoNodes")
                {
                    // Create two more code block nodes [d;] and [e;]
                    // Connect [4;]-[d;] and [d;]-[e;], then connect [e;]-[d;]

                    bool hasWarning = false;

                    var nodeD = GetNode("05126ec5-1612-47cb-9ccc-fd96aec269b1") as NodeModel;
                    var nodeE = GetNode("de271687-bb0d-49fc-81a1-e83680250f55") as NodeModel;

                    if (nodeD.State == ElementState.Warning) hasWarning = true;
                    if (nodeE.State == ElementState.Warning) hasWarning = true;

                    Assert.AreEqual(true, hasWarning);
                }
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestSyntaxHighlightRuleForDigits()
        {
            string text = "{-2468.2342E+04, dfsgdfg34534, 34534.345345, 23423, -98.7, 0..10..2, -555};";

            var rule = CodeHighlightingRuleFactory.CreateNumberHighlightingRule().Regex;
            var matches = rule.Matches(text);

            // Expected results (8):
            // -2468.2342E+04
            // 34534.345345
            // 23423
            // -98.7
            // 0
            // 10
            // 2
            // -555
            Assert.AreEqual(8, matches.Count);
            var actual = matches.Cast<Match>().Select(m => m.Value).ToArray();
            string[] expected = new string[] { "-2468.2342E+04", "34534.345345", "23423", "-98.7", "0", "10", "2", "-555" };
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [Test, RequiresSTA]
        public void TestFunctionMultipleBlocksDefaultParametersDeleteFirst()
        {
            RunCommandsFromFile("DeleteCrashCommands.xml", (commandTag) =>
            {
                // NOTE: Nothing needs to be tested here other than that this test does not crash
            });
        }

    }
}
