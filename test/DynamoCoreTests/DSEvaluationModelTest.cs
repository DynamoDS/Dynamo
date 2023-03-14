using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreNodeModels;
using DesignScript.Builtin;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using NUnit.Framework;


namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class DSEvaluationModelTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSCPython.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestCodeBlockNode01()
        {
            // (1..5) + 1;
            RunModel(@"core\dsevaluation\cbn_nolhs.dyn");
            AssertPreviewValue("4e88b4a9-397a-422e-be13-f9ffcb27bc75", new int[] { 2, 3, 4, 5, 6 });
        }

        [Test]
        public void Regress561()
        {
            // 1; ----> x
            // 2; ----> y Point.ByCoordinates(x, y, z);
            // 3; ----> z
            RunModel(@"core\dsevaluation\regress561.dyn");
            AssertClassName("8774296c-5269-450b-959d-ce4020ddbf80", "Autodesk.DesignScript.Geometry.Point");
        }

        [Test]
        public void Regress616()
        {
            // a=0..10;
            // b=a;
            // b[2]=100;
            // c=a;
            // d=b[0..(Count(b)-1)..2];  
            // 
            // d == {0, 100, 4, 6, 8, 10}
            RunModel(@"core\dsevaluation\regress616.dyn");
            AssertPreviewValue("f83a463d-1ca4-4586-a544-5df47697e483",
                new int[] { 0, 100, 4, 6, 8, 10 });
        }

        [Test]
        public void Regress618()
        {
            // a=0..10;
            // b=a;
            // b[2]=100;
            //
            // x = b; 
            // f = 0;
            // 
            // x + f;
            RunModel(@"core\dsevaluation\regress618.dyn");
            AssertPreviewValue("60e002e2-e723-4e39-b059-d761596f24da",
                new int[] { 0, 1, 100, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [Test]
        public void Regress586()
        {
            RunModel(@"core\dsevaluation\regress586.dyn");
            AssertPreviewValue("3c9b3bb2-726f-4dbf-b889-9332604c1c01", 3);
        }

        [Test]
        public void Regress657()
        {
            // 5; ----> y = 1..x;
            RunModel(@"core\dsevaluation\regress657.dyn");
            AssertPreviewValue("dbd24f59-3483-4e86-b433-54738746fe14", new int[] { 1, 2, 3, 4, 5 });
        }

        [Test]
        public void Regress664()
        {
            // 1..5 ----> y = x;
            RunModel(@"core\dsevaluation\regress664.dyn");
            AssertPreviewValue("34ad4880-deaf-4b03-a8b6-7545c9d0159c", new int[] { 2, 3, 4, 5, 6 });
        }

        [Test]
        public void CBN_Multiline_614()
        {
            // a=1;
            // b=2;
            RunModel(@"core\dsevaluation\multiline_614.dyn");
            AssertValue("a_a826eb6109534282aca0f888679fe084", 1);
            AssertValue("b_a826eb6109534282aca0f888679fe084", 2);
        }

        [Test]
        public void CBN_Multiline_614_2()
        {
            // With empty line 
            // a=1;
            // b=2;

            RunModel(@"core\dsevaluation\multiline_614_2.dyn");
            AssertValue("a_a826eb6109534282aca0f888679fe084", 1);
            AssertValue("b_a826eb6109534282aca0f888679fe084", 2);
        }
        [Test]
        public void CBN_String_599()
        {
            // With empty line 
            // a="Dynamo";

            RunModel(@"core\dsevaluation\CBN_string_599.dyn");
            AssertPreviewValue("f098c8de-8714-49a4-b556-7dee2953590a", "Dynamo");

        }
        [Test]
        public void CBN_Create_697()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-697
            // a=1;
            RunModel(@"core\dsevaluation\CBN_Create_697.dyn");
            AssertPreviewValue("df8f3354-78a6-4786-9a0f-8844073b898f", 1);
        }
        [Test]
        public void CBN_Math_Pi_621()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-621
            RunModel(@"core\dsevaluation\CBN_Math_Pi_621.dyn");
            AssertPreviewValue("b1d53709-114b-4ea6-8687-5f4456ddc336", Math.PI);

        }

        [Test]
        public void CBN_Multiple_Assignment614()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-614
            RunModel(@"core\dsevaluation\CBN_Multiple_Assignement_614.dyn");
            AssertValue("a_42f211b3faf04b99add9764a0f9d0df4", 1);
            AssertValue("b_42f211b3faf04b99add9764a0f9d0df4", 1);

        }
        [Test]
        public void CBN_Conditional_612()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-612
            RunModel(@"core\dsevaluation\CBN_Conditional_612.dyn");
            AssertPreviewValue("59084c1a-86c9-47e7-bae1-7edce5ffb83b", new int[] { 1, 2 });

        }
        [Test]
        public void CBN_Reference_593()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-593
            RunModel(@"core\dsevaluation\CBN_Reference_593.dyn");
            AssertPreviewValue("e7b304bf-3d30-482d-951b-5e28c811c411", 3);

        }
        [Test]
        public void CBN_Reference_593_2()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-593
            RunModel(@"core\dsevaluation\CBN_Reference_593_2.dyn");
            AssertPreviewValue("d177c6b4-2bb4-4f3a-a2e9-6d8d9e8db17b", new int[] { 1, 2, 3, 4, 5 });

        }
        [Test]
        public void CBN_Binary_607()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-607
            RunModel(@"core\dsevaluation\CBN_binary_607.dyn");
            AssertPreviewValue("81f25df3-a55e-48c9-9fe8-0ce0cc17fdc3", true);

        }
        [Test]
        public void CBN_Multiple_binary_607()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-607
            RunModel(@"core\dsevaluation\CBN_multiple_binary_607.dyn");
            AssertPreviewValue("56383e77-7cfc-47af-b136-e268edd21486", true);

        }
        [Test]
        public void CBN_Conditionals_597()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-597
            RunModel(@"core\dsevaluation\CBN_conditionals_597.dyn");
            AssertPreviewValue("5e1e0e66-793e-4766-8935-81c56d8b1dc3", 1);

        }
        [Test]
        public void CBN_Conditionals_597_2()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-597
            RunModel(@"core\dsevaluation\CBN_conditionals_597_2.dyn");
            AssertPreviewValue("5e1e0e66-793e-4766-8935-81c56d8b1dc3", 5);

        }
        [Test]
        public void CBN_Nested_Conditionals_608()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-608
            RunModel(@"core\dsevaluation\CBN_nested_Conditionals_608.dyn");
            AssertPreviewValue("ecc394fe-4253-4c1f-b83b-0cc56ba69fdd", false);

        }
        [Test]
        public void CBN_Nested_Conditionals_608_2()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-608
            RunModel(@"core\dsevaluation\CBN_nested_Conditionals_608_2.dyn");
            AssertPreviewValue("688763a6-4891-45f2-9ce6-7722546c48b8", false);

        }
        [Test]
        public void CBN_Nested_Conditionals_612()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-612
            RunModel(@"core\dsevaluation\CBN_ConditionalWithExpression_612.dyn");
            AssertPreviewValue("688763a6-4891-45f2-9ce6-7722546c48b8", new int[] { 1, 2 });

        }
        [Test]
        public void CBN_Nested_Conditionals_612_2()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-612
            RunModel(@"core\dsevaluation\CBN_ConditionalWithExpression_612_2.dyn");
            AssertPreviewValue("688763a6-4891-45f2-9ce6-7722546c48b8", new int[] { 1, 2 });

        }
        [Test]
        public void CBN_Multiline_705()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-705
            RunModel(@"core\dsevaluation\CBN_multiline_705.dyn");
            AssertPreviewValue("d59b7582-7d01-41b5-ad25-d6133e85cd58", 3);

        }


        [Test]
        public void CBN_Undefined_692()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-692
            RunModel(@"core\dsevaluation\CBN_Undefined_692.dyn");
        }

        [Test]
        public void CBN_Class_GetterProperty_625()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-625
            RunModel(@"core\dsevaluation\CBN_Class_GetterProperty_625.dyn");
            AssertPreviewValue("0cec98ab-010c-4335-bd84-34d1847326ab", 0.00);

        }

        [Test]
        public void CBN_Geometry_RangeExpression_609()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-609
            RunModel(@"core\dsevaluation\CBN_Geometry_RangeExpression_609.dyn");
            //AssertValue("b", new int[] {1,2});
            AssertPreviewValue("056d9dc6-2905-4575-803c-f023005b8e6a", new int[] { 1, 2 });

        }
        [Test]
        public void CBN_Geometry_Expression_609_2()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-609
            RunModel(@"core\dsevaluation\CBN_Geometry_Expression_609_2.dyn");
            //AssertValue("b", 3);
            AssertPreviewValue("056d9dc6-2905-4575-803c-f023005b8e6a", 3);

        }
        [Test]
        public void CBN_Geometry_Conditional_609_3()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-609
            RunModel(@"core\dsevaluation\CBN_Geometry_Conditional_609_3.dyn");
            //AssertValue("b", 3.00);
            AssertPreviewValue("056d9dc6-2905-4575-803c-f023005b8e6a", 3.00);

        }
        [Test]
        public void CBN_Geometry_Array_609_4()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-609
            RunModel(@"core\dsevaluation\CBN_Geometry_Array_609_4.dyn");
            //AssertValue("b", new double[] {1.0,2.0});
            AssertPreviewValue("056d9dc6-2905-4575-803c-f023005b8e6a", new double[] { 1.0, 2.0 });
        }
        [Test]
        public void CBN_Double_Array_330()
        {

            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-330
            RunModel(@"core\dsevaluation\CBN_Double_Array_330.dyn");
            AssertPreviewValue("da9c8ae9-034b-4ab7-b189-b40ad597110b", 4);

        }

        [Test]
        public void CBN_Dynamic_Array_622()
        {

            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-622
            RunModel(@"core\dsevaluation\CBN_Dynamic_Array_622.dyn");
            AssertValue("y_1086551ad8514eccb9bf9eeba3e9f6bf", new int[] { 1 });
            AssertValue("c_1086551ad8514eccb9bf9eeba3e9f6bf", 1);

        }
        [Test]
        public void CBN_Dynamic_Array_622_2()
        {

            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-622
            RunModel(@"core\dsevaluation\CBN_Dynamic_Array_622_2.dyn");
            AssertPreviewValue("1086551a-d851-4ecc-b9bf-9eeba3e9f6bf", 4);

        }
        [Test]
        public void CBN_Dynamic_Array_592()
        {

            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-592
            RunModel(@"core\dsevaluation\CBN_nestedrange592.dyn");
            AssertPreviewValue("ae1def01-2d92-43ab-9c64-68b36ee4d4aa",
                new int[][] { new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                    new int[] { 2, 4, 6, 8, 10, 12 }, new int[] { 3, 6, 9, 12 } });

        }
        [Test]
        public void CBN_Dynamic_Array_592_2()
        {

            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-592
            RunModel(@"core\dsevaluation\CBN_nestedrange592_2.dyn");
            AssertPreviewValue("ae1def01-2d92-43ab-9c64-68b36ee4d4aa",
                new int[][] { new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                    new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                    new int[] { 3, 4, 5, 6, 7, 8, 9, 10 } });
        }
        [Test]
        public void CBN_Dynamic_Array_433()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-433
            RunModel(@"core\dsevaluation\CBN_Dynamic_Array_433.dyn");
            AssertValue("a", null);
        }

        [Ignore("empty codeblocks should not exist")] //Ignored because empty code block nodes should not exist
        public void Regress722()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-722
            RunModel(@"core\dsevaluation\regress722.dyn");
            AssertValue("x", 42);
        }
        [Test]
        public void CBN_Array_Range_629()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-629
            RunModel(@"core\dsevaluation\CBN_Array_Range_629.dyn");
            AssertPreviewValue("f5b252f3-67b9-4287-a003-eb8d3b8f9cb2",
                new int[][] { new int[] { 1, 2 } });
        }
        [Test]
        public void CBN_Array_Range_629_2()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-629
            RunModel(@"core\dsevaluation\CBN_Array_Range_629_2.dyn");
            AssertPreviewValue("f5b252f3-67b9-4287-a003-eb8d3b8f9cb2", new int[] { 4 });
        }
        [Test]
        public void CBN_Empty_722()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-722
            RunModel(@"core\dsevaluation\CBN_Empty_722.dyn");
            AssertPreviewValue("25b8e1de-925a-46fb-9fcf-5d4100a5af0d", 10.00);
        }
        [Test]
        public void CBN_array_indexnull_619()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-722
            RunModel(@"core\dsevaluation\CBN_array_indexnull_619.dyn");
            AssertPreviewValue("6985948e-992c-4420-8c39-1f5f5d57dc64", new object[] {});

            ProtoCore.RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            Assert.AreEqual(1, runtimeCore.RuntimeStatus.WarningCount);

            ProtoCore.Runtime.WarningEntry warningEntry = runtimeCore.RuntimeStatus.Warnings.ElementAt(0);
            Assert.AreEqual(ProtoCore.Runtime.WarningID.InvalidArrayIndexType, warningEntry.ID);
        }

        [Test]
        public void CBN_TypedIdentifier01()
        {
            // MAGN-7463
            RunModel(@"core\dsevaluation\CBN_TypedIdentifier01.dyn");
            AssertPreviewValue("1472b79b-59b0-40ab-ab0e-3504fbc7be83", 1);
        }

        [Test]
        public void Regress737()
        {
            RunModel(@"core\dsevaluation\regress737.dyn");
            AssertPreviewValue("ccad1780-f570-4ccc-ae7a-0ad1b663c3dd", 21);
        }


        [Test]
        public void Regress781()
        {
            OpenModel(@"core\dsevaluation\makeSpiralFromBasePtCenterPtHeight.dyf");
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_844()
        {
            // CBN: a = 1;
            // CBN b = a; 

            RunModel(@"core\dsevaluation\Defect_MAGN_844.dyn");

            // Change in implementation for Local variable this test case needs update in final value.
            AssertPreviewValue("8de1b8aa-c6c3-4360-9619-fe9d01a804f8", null);

        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_829_1()
        {
            // CBN ==> 1=a;
            RunModel(@"core\dsevaluation\Defect_MAGN_829_1.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.Pass("Execution completed successfully");

        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_829_2()
        {
            // CBN ==> 1=1=a;
            RunModel(@"core\dsevaluation\Defect_MAGN_829_2.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.Pass("Execution completed successfully");
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_829_3()
        {
            // CBN ==> a=1=2=3;
            RunModel(@"core\dsevaluation\Defect_MAGN_829_3.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.Pass("Execution completed successfully");
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_829_4()
        {
            // CBN ==> a*a=1;;
            RunModel(@"core\dsevaluation\Defect_MAGN_829_4.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.Pass("Execution completed successfully");
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_829_5()
        {
            // Multiline CBN ==> a=1;
            //               ==> 1 = a;
            RunModel(@"core\dsevaluation\Defect_MAGN_829_5.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.Pass("Execution completed successfully");
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_610()
        {
            // Multiline CBN ==> a={1,2,3};
            //               ==> a[0]= 3;
            RunModel(@"core\dsevaluation\Defect_MAGN_610.dyn");
            AssertPreviewValue("aa78716b-f3f6-4676-bb72-2cb1c34181f8", new int[] { 3, 2, 3 });
            AssertValue("a_aa78716bf3f64676bb722cb1c34181f8", new int[] { 3, 2, 3 });
        }

        [Test]
        [Category("Failure")]
        public void UsingFunctionObject01()
        {
            RunModel(@"core\dsevaluation\FunctionObject.dyn");
            AssertPreviewValue("5dad688a-e6f3-4153-b87e-d1713b645de9", 45);
        }

        [Test]
        [Category("Failure")]
        public void UsingFunctionObject02()
        {
            RunModel(@"core\dsevaluation\Apply.dyn");
            AssertPreviewValue("11b2c7b2-2854-4e46-a8fa-4d1d52ebf4b7", 20);
        }


        [Test]
        public void DefaultValueTest()
        {
            RunModel(@"core\dsevaluation\DefaultValue.dyn");
            AssertPreviewValue("be9d1181-a83e-4f25-887f-6197aa8d581e", 5.0);
        }

        [Test]
        public void BasicRuntimeWarning()
        {
            RunModel(@"core\dsevaluation\BasicRuntimeWarning.dyn");
            var guid = Guid.Parse("0fc83562-2cfe-4a63-84f8-f6836cbaf9c5");
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(n => n.GUID == guid);
            Assert.IsTrue(node.State != ElementState.Warning);
        }

        [Test]
        public void NumberSequence()
        {
            RunModel(@"core\dsevaluation\NumberSequence.dyn");
            AssertPreviewValue("4d86876b-08a8-4166-b1f5-4194b8381dab",
                new object[] { 0.0, 1.0, 2.0, 3.0, 4.0 });
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_2363()
        {
            // CBN: 0..#3..10;
            RunModel(@"core\dsevaluation\Defect_MAGN_2363.dyn");
            AssertPreviewValue("f20147a0-21bb-4f23-b6bb-450591e62b31",
                new object[] { 0, 10, 20 });
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_2479()
        {
            // Details are available in http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2479
            RunModel(@"core\dsevaluation\Defect_MAGN_2479.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            AssertPreviewValue("0ffe94bd-f926-4e81-83f7-7975e67a3713",
                new int[] { 2, 4, 6, 8 });
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_2375_3487()
        {
            // This test case is addressing the following two defects:
            // Details are available in http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2375
            //                      and http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3487
            RunModel(@"core\dsevaluation\Defect_MAGN_2375_3487.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            CurrentDynamoModel.AddToSelection(CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("5a7f7549-fbef-4c3f-8578-c67471eaa87f"));

            CurrentDynamoModel.Copy();
            CurrentDynamoModel.Paste();

            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            //run the graph after copy paste
            BeginRun();

            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DSVarArgFunction>();
            foreach (var item in nodes)
            {
                AssertPreviewValue(item.GUID.ToString(), new string[] { "Dynamo", "DS" });
            }
        }

        [Test]
        public void Map_With_PropertyMethod()
        {
            // Function object to property method and used in map
            RunModel(@"core\dsevaluation\map_property.dyn");
            AssertPreviewValue("abc4796e-b35d-4076-b6ff-2f814dda350f", new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 });
        }

        [Test]
        public void CBN_Geometry()
        {
            RunModel(@"core\dsevaluation\CBN_Geometry.dyn");
            AssertPreviewValue("a23b89fc-f219-46ca-ab7a-5a3f0ee93ba4", 5.00000);
        }
        [Test]
        [Category("RegressionTests")]
        public void CBN_Range_1300()
        {
            RunModel(@"core\dsevaluation\CBN_Range_1300.dyn");
            AssertPreviewValue("372d31c2-b66e-494c-a33d-e82ae1ecf68a", new double[] { 0.000000, 2.000000, 10.392305, 16.000000, 10.392305, 2.000000, 0.000000, -2.000000, -10.392305, -16.000000, -10.392305, -2.000000, 0.000000 });
        }

        [Test]
        [Category("Failure")]
        public void Apply_With_PropertyMethod()
        {
            // Function object to property method and used in apply 
            RunModel(@"core\dsevaluation\apply_property.dyn");
            AssertPreviewValue("2cbadbce-ee25-4f90-8309-3b81bf4fdfd9", 42.0);
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_3264()
        {
            // Function object to property method and used in apply 
            RunModel(@"core\dsevaluation\Defect_MAGN_3264.dyn");
            AssertPreviewValue("eaa2b29f-b5f4-4017-a143-3fb2d4af349c", new double[] { 0, 1, 2, 3, 4 });
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_2535()
        {
            // Function object to property method and used in apply 
            RunModel(@"core\dsevaluation\EvaluateFptrInOtherCBN.dyn");
            AssertPreviewValue("49048255-fc2c-463d-8e93-96e20f061a0d", 42);
        }

        [Test]
        public void Test_Formula_Lacing()
        {
            RunModel(@"core\formula\formula_lacing.dyn");

            AssertPreviewValue("d9b9d0a9-1fec-4b20-82c4-2d1665306509", new int[] { 4, 6, 7 });
            AssertPreviewValue("c35f1c6d-b955-4638-802f-208f93112078", new object[] { new int[] { 4, 5, 6 }, new int[] { 5, 6, 7 } });
        }

        [Test]
        public void Test_Longest_Lacing()
        {
            RunModel(@"core\dsevaluation\longest_lacing.dyn");

            AssertPreviewValue("c9476b21a972476788e184982918700e", new object[]
            {
                new[] {3, 4, 5}, new[] {7, 8, 9}, new[] {8, 9, 10}
            });
        }

        [Test]
        public void Test_Auto_Lacing()
        {
            RunModel(@"core\dsevaluation\auto_lacing.dyn");

            AssertPreviewValue("c9476b21a972476788e184982918700e",
                new object[] {new[] {3, 4, 5}, new[] {7, 8, 9}});
        }

        [Test]
        public void Test_Cross_Lacing()
        {
            RunModel(@"core\dsevaluation\cross_lacing.dyn");

            AssertPreviewValue("c9476b21a972476788e184982918700e", new object[]
            {
                new object[] {new[] {3, 4, 5}, new[] {4, 5, 6}, new[] {5, 6, 7}},
                new object[] {new[] {6, 7, 8}, new[] {7, 8, 9}, new[] {8, 9, 10}}
            });
        }

        [Test]
        public void Test_Shortest_Lacing()
        {
            RunModel(@"core\dsevaluation\shortest_lacing.dyn");

            AssertPreviewValue("c9476b21a972476788e184982918700e", new object[] {new[] {3, 4, 5}});
        }

        [Test]
        public void CBNAndFormula()
        {
            RunModel(@"core\dsevaluation\CBNWithFormula.dyn");
            var id = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>().GUID;
            AssertPreviewValue(id.ToString(), 3);
        }

        [Test]
        [Category("RegressionTests")]
        public void Test_IFnode_3483_1()
        {
            RunModel(@"core\dsevaluation\ifnode_3483.dyn");
            AssertPreviewValue("516de967-91ac-4a93-87ce-7f130774268a", 5.00);
        }
        [Test]
        [Category("RegressionTests")]
        public void Test_IFnode_3483_2()
        {
            RunModel(@"core\dsevaluation\ifnode_3483_2.dyn");
            AssertPreviewValue("70b5aeba-80b7-48cc-a48d-6c755c28555c", new object[] { 1, 1, 1, 1, 2 });
        }
        [Test]
        [Category("RegressionTests")]
        public void Test_ConditionalCustomFunction_3483()
        {
            RunModel(@"core\dsevaluation\conditionalCustomFunction_3483.dyn");
            AssertPreviewValue("46f484ed-eecd-45cd-9904-0020a3c98999", 2.647);
        }
        [Test]
        [Category("RegressionTests")]
        public void Test_CustomFunctionobject_3483()
        {
            RunModel(@"core\dsevaluation\CustomFunctionobject_3483.dyn");
            AssertPreviewValue("dace0b8c-381e-4de5-824d-c1651ec94bc6", new double[] { 1, 1, 1, 1, 1, 2, 3 });
        }

        [Test]
        [Category("RegressionTests")]
        public void Test_IfinputFunctionObject_3483()
        {
            RunModel(@"core\dsevaluation\IfinputFunctionObject_3483.dyn");
            AssertPreviewValue("079fa696-e6e7-402f-866f-9bf4306b5020", 1.00);
        }

        [Test]
        [Category("RegressionTests")]
        public void Test_IfAsFunctionobject_3483()
        {
            RunModel(@"core\dsevaluation\IfAsFunctionobject_3483.dyn");
            AssertPreviewValue("80d14b69-4796-48c9-a34d-f447abf7b5ba", new object[] {
                new object[] { 1, 1, 1, 1, 1},
                new object[] { 1, 1, 1, 1, 1},
                new double[] {-2,-1, 0, 1, 2},
                new object[] { 1, 1, 1, 1, 1},
                new object[] { 1, 1, 1, 1, 1},
                });
        }

        [Test]
        [Category("RegressionTests")]
        public void Test_IfOutputFunctionObject_3483()
        {
            RunModel(@"core\dsevaluation\IfOutputCustomFunction_3483.dyn");
            AssertPreviewValue("dace0b8c-381e-4de5-824d-c1651ec94bc6", new bool[] { true, true, true, false, false });
        }

        [Test]
        [Category("RegressionTests")]
        public void Test_If_CustomNode_4058()
        {
            RunModel(@"core\dsevaluation\Test_IfINCustomNode_4058.dyn");

            AssertPreviewValue("4c70f814-5c42-4fb9-89b0-b3cfe6f93b6d", 6.00);
        }
        [Test]
        [Category("RegressionTests")]
        public void TestSingleIFRecusion_4058()
        {
            RunModel(@"core\dsevaluation\TestSingleIFRecusion_4058.dyn");
            AssertPreviewValue("f2b979b2-7824-428c-a960-5e7ca8cac1f1", 4);

        }
        [Test]
        [Category("RegressionTests")]
        public void TestMultipleIFCN_4058()
        {
            RunModel(@"core\dsevaluation\testMultipleIFCN_4058.dyn");

            AssertPreviewValue("201866a4-e368-41e9-8264-bf4d8fb65ed1", 10);

        }
        [Test]
        [Category("RegressionTests")]
        public void TestMultipleIFRecursion_4058()
        {
            Assert.Inconclusive("MultipleIF Recursion ");

            RunModel(@"core\dsevaluation\testMultipleIFRecursion_4058.dyn");

            AssertPreviewValue("201866a4-e368-41e9-8264-bf4d8fb65ed1", new double[] { 1, 1, 2, 3, 5 });

        }
        [Test]
        [Category("RegressionTests")]
        [Category("Failure")]
        public void TestNestedIFNORecursion_4058()
        {
            RunModel(@"core\dsevaluation\TestNestedIfNoRecursion_4058.dyn");

            AssertPreviewValue("f7f34898-2eb3-48fb-9a65-6084446dfbd0", 11);

        }
        [Test]
        [Category("RegressionTests")]
        public void TestNestedIFRecursion_4058()
        {
            RunModel(@"core\dsevaluation\NestedIFRecursion_4058.dyn");
            AssertPreviewValue("2a09f286-b0fe-443a-be87-591f5c6e9264", "Odd");

        }
        [Test]
        [Category("RegressionTests")]
        public void NestedIFRecursionMultiple_4058()
        {
            Assert.Inconclusive("MultipleIF Recursion");

            RunModel(@"core\dsevaluation\NestedIFRecursionMultiple_4058.dyn");

            AssertPreviewValue("e6a9eec4-a18d-437d-8779-adfd6141bf19", 9);

        }

        [Test]
        [Category("RegressionTests")]
        public void CBN_warning_5236()
        {
            // Functions does not work in the code block node but works if expanded as a graph
            // This test regression - issue is described in detail in the bug
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5236

            RunModel(@"core\dsevaluation\createCube_codeBlockNode.dyn");
            AssertPreviewValue("8187805a-cc0d-4220-8595-4ee38bbee079", 150);
            var guid = Guid.Parse("3669d05c-c741-44f9-87ab-8961e7f5f112");
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(n => n.GUID == guid);
            Assert.IsTrue(node.State != ElementState.Warning);


        }
        [Test]
        [Category("RegressionTests")]
        public void DoubleToInt_NoWarning_5109()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5109
            //verify  Warning converting double to int is removed
            RunModel(@"core\dsevaluation\DoubleToInt_5109.dyn");
            var guid = System.Guid.Parse("d66d3d3e-e13b-460e-a8a7-056c434ee620");
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(n => n.GUID == guid);
            Assert.IsTrue(node.State != ElementState.Warning);
        }



        [Test]
        [Category("RegressionTests")]
        public void CBN_Variable_Type_5480()
        {
            // MAGN-5480 - Defect in parsing typed identifiers in CBN

            RunModel(@"core\dsevaluation\CBN_Variable_Type_5480.dyn");
            AssertPreviewValue("fabaccff-5b8a-4505-b752-7939cba90dc4", 1);
        }

        [Test]
        public void TestDefaultValueInFunctionObject()
        {
            // Regression test case for
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5233
            var dynFilePath = Path.Combine(TestDirectory, @"core\default_values\defaultValueInFunctionObject.dyn");

            RunModel(dynFilePath);

            AssertPreviewValue("4218d135-a2c4-4dee-8415-8f0bf1de671c", new[] { 1, 1 });


        }
        [Test]
        public void TestRunTimeWarning_3132()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3132
            // test for run time warning is thrown or not 

            var dynFilePath = Path.Combine(TestDirectory, @"core\dsfunction\RunTimeWarning_3132.dyn");

            RunModel(dynFilePath);
            var guid = Guid.Parse("88f376fa-634b-422e-b853-6afa8af8d286");
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(n => n.GUID == guid);

            Assert.IsTrue(node.State == ElementState.Warning);
        }

        [Test]
        public void List_Map_Default_5233()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5233
            //List.map with default arguments 

            var dynFilePath = Path.Combine(TestDirectory, @"core\list\List_Map_DefaultArg5233.dyn");
            RunModel(dynFilePath);
            AssertPreviewValue("6a0207d9-78d7-4fd3-829f-d19644acdc1b", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [Test]
        public void TestListCombineRegress5561()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\regress5561.dyn");
            RunModel(dynFilePath);
            AssertPreviewValue("4fb0a4ef-8151-4e5f-a2e6-9c3fcd2c1e8f", new object[] { "1foo", null });
        }

        [Test]
        public void TestMod()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsfunction\modDoesntWork.dyn");
            RunModel(dynFilePath);
            AssertPreviewValue("77c95ace-e4f1-4119-87fc-7163f9b3b8b0", true);
            AssertPreviewValue("21f58def-725d-41c9-abc7-063cc3642420", true);
            AssertPreviewValue("dbd73c1b-0b40-4138-af69-4dd3da2de62d", true);
        }

        [Test]
        public void TestDefaultValueAttribute()
        {
            var dynFilePath = Path.Combine(TestDirectory,
                @"core\default_values\defaultValueAttributeTest.dyn");

            RunModel(dynFilePath);
            AssertPreviewValue("4f0c05a7-4e52-4d60-807a-08824baa23bb", true);
        }

        [Test]
        public void TestDefaulArgumentAttributeNegative()
        {
            // This is to test FFITarget.TestData.MultiplyBy3NonParsableDefaultArgument() whose
            // DefaultArgumentAttribute is invalid. In this case, we should make sure that
            // no default argument is used, even null. So this function should be compiled to
            // a function object and Apply() should work on it. 
            var dynFilePath = Path.Combine(TestDirectory, @"core\default_values\invalidDefaultArgument.dyn");
            RunModel(dynFilePath);
            AssertPreviewValue("1b2fa812-960d-424c-b679-8b850abe2e26", 12);
        }

        [Test]
        public void TestDefaultValueAttributeForDummyLine()
        {
            var dynFilePath = Path.Combine(TestDirectory,
                @"core\default_values\defaultValueAttributeForDummyLine.dyn");

            RunModel(dynFilePath);
            AssertPreviewValue("e95a634b-aab9-4b6e-bb33-2f9669381ad6", 5);
        }

        [Test]
        public void ModuloDividendLargerThanDivisor()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\math");

            string openPath = Path.Combine(examplePath, "ModuloDividendLargerThanDivisor.dyn");
            RunModel(openPath);
            double[] Dlist = new double[4];
            Dlist[0] = 0.129000;
            Dlist[1] = 0.026000;
            Dlist[2] = 1.899000;
            Dlist[3] = 1.830000;

            AssertPreviewValue("9433d723-3708-4773-9b9c-c6def0f17b18", Dlist);
            AssertPreviewValue("55f03be2-8720-4648-b989-996b261e1502", new[] { false, false, false, false });
            AssertPreviewValue("0b9eca5b-835b-493e-af4b-84a3366d75d3", Dlist);
            AssertPreviewValue("7e1f9810-2f4f-4911-975b-d392afc3a674", Dlist);
        }

        [Test, Category("UnitTests")]
        public void TestDefaultArgumentTooltip()
        {
            var node =
                new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("Autodesk.DesignScript.Geometry.Point.ByCoordinates@double,double"));
            CurrentDynamoModel.ExecuteCommand(new Dynamo.Models.DynamoModel.CreateNodeCommand(node, 0, 0, true, false));
            Assert.IsTrue(node.InPorts[0].ToolTip.Equals("X coordinate\n\ndouble\nDefault value : 0"));
            node.InPorts[0].UsingDefaultValue = false;
            Assert.IsTrue(node.InPorts[0].ToolTip.Equals("X coordinate\n\ndouble\nDefault value : 0 (disabled)"));
        }
        [Test]
        public void Reorder_7573()
        {

            // Original defect: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7573

            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\reorder.dyn");

            RunModel(dynFilePath);

            AssertPreviewValue("c739b941-ece7-4b87-ae69-9a16f04dbe5d", new object[] { null, null, null, null });

            // Reset engine and mark all nodes as dirty. A.k.a., force re-execute.
            CurrentDynamoModel.ForceRun();

            AssertPreviewValue("79d158b3-fa40-4069-8bb5-153e6fb13858", new object[] { 2, 3, 6, 5 });
        }
        [Test]
        public void Removekey_7573()
        {

            // Original defect: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7573

            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\removekey.dyn");

            RunModel(dynFilePath);
            // Fix expected result after MAGN-7639 is fixed.

            AssertPreviewValue("bd89982a-c3e6-4a4e-898c-2bdc8f1f8c3e", null);

            // Reset engine and mark all nodes as dirty. A.k.a., force re-execute.
            CurrentDynamoModel.ForceRun();

            // Fix expected result after MAGN-7639 is fixed.
            AssertPreviewValue("980dcd47-84e7-412c-8d9e-d66f166d2370", new object[] { 1, 2, 3, 4 });

        }

        [Test]
        [Category("RegressionTests")]
        public void TestListJoin()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\list\ListJoin.dyn");
            RunModel(dynFilePath);
            AssertPreviewValue("ea031ca8-9c49-4d14-a702-54022cb60e0f", 5);
        }

        [Test]
        public void TestCallingStaticMethod()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\TestCallingStaticMethod.dyn");
            RunModel(dynFilePath);
            AssertPreviewValue("dc61bae7-a661-477f-a438-ace939d958f4", 5.0);
        }

        [Test]
        public void Regress9279_NoRandomNull()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\regress9297.dyn");
            OpenModel(dynFilePath);
            var filename = this.CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<CoreNodeModels.Input.Filename>();
            filename.Value = filename.Value.Replace(@"{path}", Path.Combine(TestDirectory, @"core\dsevaluation\layer3.png"));
            RunCurrentModel();

            AssertPreviewValue("2a944080-94e1-4cf9-88e2-64556335c838", 2);
        }

        [Test]
        public void Regress7808()
        {
            // Verify that updating the function defintion will execute code blocks node that use
            // this function.
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\regress7808.dyn");
            OpenModel(dynFilePath);

            // Original function defintion is 
            // def foo() { return = 21;}
            var watchNodeGuid = "aef2375c-3dd8-4be0-8230-d964a2417f99";
            AssertPreviewValue(watchNodeGuid, 21);

            // change to
            // def foo() { return = 42; }
            var cbnGuid = Guid.Parse("6a260ba7-d658-4350-a777-49511f725454");
            var command = new Dynamo.Models.DynamoModel.UpdateModelValueCommand(Guid.Empty, cbnGuid, "Code", @"def foo() { return = 42; }");
            CurrentDynamoModel.ExecuteCommand(command);
            RunCurrentModel();

            AssertPreviewValue(watchNodeGuid, 42);
        }

        [Test]
        public void Regress9450()
        {
            // Verify the function with default argument works.
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\regress9450.dyn");
            OpenModel(dynFilePath);
            AssertPreviewValue("25a90516-9e46-4268-a745-266524844158", 6);
        }

        [Test]
        public void TestContainsUsingEqualityTest()
        {
            // Verify the function with default argument works.
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsfunction\contains.dyn");
            OpenModel(dynFilePath);
            AssertPreviewValue("a612292b-6c72-41f7-bf91-753925f7a776", true);
            AssertPreviewValue("ad5b2297-537c-4445-b8db-9eca720787ec", true);
        }

        [Test]
        public void TestContainsArray()
        {
            // Verify the function with default argument works.
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsfunction\contains2.dyn");
            OpenModel(dynFilePath);
            AssertPreviewValue("863e8d06-0175-42ec-8613-305e9efa95d0", true);
            AssertPreviewValue("b0d7b844-93a9-43fa-b8f1-15cbb7469a84", true);
            AssertPreviewValue("d1942e84-355f-4083-bb1c-6b7203ee192c", true);
        }

        [Test]
        public void TestIntegerOverflow()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\integer_overflow.dyn");
            OpenModel(dynFilePath);
            AssertPreviewValue("17aae6a5-c4d5-4ba9-862c-5fd2e99c334e", 8388608);
        }

        [Test]
        public void TestDictionaryDefintion()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\define_dictionary.dyn");
            OpenModel(dynFilePath);

            var validationData1 = Dictionary.ByKeysValues(new[] {"a", "b", "c"}, new object[] {1, 2, 3});
            AssertPreviewValue("1e454a5a38284c74bf53fc3249704183", validationData1);

            validationData1 = Dictionary.ByKeysValues(new[] {"a", "b", "c"}, new[] {"Bob", "Sally", "Pat"});
            AssertPreviewValue("ab406c15327240858fdb2662bcc52276", validationData1);
            
        }

        [Test]
        public void TestDictionaryDefinition2()
        {
            // Regression test for https://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-10382
            // To test that variable could still be properly renamed.
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\define_dictionary2.dyn");
            RunModel(dynFilePath);

            ProtoCore.RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            Assert.AreEqual(6, runtimeCore.RuntimeStatus.WarningCount);

            foreach (var warning in runtimeCore.RuntimeStatus.Warnings)
            {
                Assert.AreEqual(ProtoCore.Runtime.WarningID.InvalidArrayIndexType, warning.ID);
            }
        }

        [Test]
        public void TestMAGN9507()
        {
            // x = [1, 2, 3];
            // x = Count(x);
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\MAGN-9507.dyn");
            OpenModel(dynFilePath);
            AssertPreviewValue("3bf992eb-ecc9-4fcc-a90b-9b1ee7e925e9", 3);
        }

        [Test, Category("UnitTests")]
        public void TestHeterogenousList()
        {
            // open test graph
            RunModel(@"core\dsevaluation\test_hetereogenous_list.dyn");

            var guidX = "0cb9b9ea3c004c099c31ddfac1ebbb09";
            var guidY = "e7711f22858f4ea6bb23112f274b8914";
            var guidZ = "a8519f77028643d4af2c0bc415a163fc";
            AssertPreviewValue(guidX, new object[] { null, null, null, 5 });
            AssertPreviewValue(guidY, new object[] { null, null, null, 10.2 });
            AssertPreviewValue(guidZ, new object[] { null, null, null, 15.2 });
        }

        [Test, Category("UnitTests")]
        public void ReplicationWithEmptySubLists()
        {
            RunModel(@"core\dsevaluation\Replication_EmptySublist.dyn");
            var guidCurveLength = "1b247af2b1c046fb9f8e3e27761ab5a9";
            var guidCodeBlock = Guid.Parse("b9dec880d99347eb8a203783f54763e6");
            AssertPreviewValue(guidCurveLength, new object[] { new object[] { }, new object[] { 6.283185 } });

            var command = new Dynamo.Models.DynamoModel.UpdateModelValueCommand(Guid.Empty, guidCodeBlock, "Code", @"[[c],[]]");
            CurrentDynamoModel.ExecuteCommand(command);
            RunCurrentModel();
            AssertPreviewValue(guidCurveLength, new object[] { new object[] { 6.283185 }, new object[] { } });
        }
    }

    [Category("DSCustomNode")]
    class CustomNodeEvaluationViewModel : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void CustomNodeNoInput01()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\CustomNodes\");

            CustomNodeInfo info;
            Assert.IsTrue(
                CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(examplePath, "NoInput.dyf"),
                    true,
                    out info));

            string openPath = Path.Combine(examplePath, "TestNoInput.dyn");
            //model.Open(openPath);

            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            AssertPreviewValue("f9c6aa7f-3fb4-40df-b4c5-6694e8c437cd",
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }
        [Test]
        public void CustomNodeWithInput02()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\CustomNodes\");

            CustomNodeInfo info;
            Assert.IsTrue(
                CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(examplePath, "CNWithInput.dyf"),
                    true,
                    out info));

            string openPath = Path.Combine(examplePath, "TestCNWithInput.dyn");
            //model.Open(openPath);

            RunModel(openPath);

            // check all the nodes and connectors are loaded


            AssertPreviewValue("1bee0f0f-5c93-48b3-a90d-f8761fa6e221", 3);
        }
        [Test, Category("Failure")]
        public void CustomNodeWithCBNAndGeometry()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\CustomNodes\");

            CustomNodeInfo info;
            Assert.IsTrue(
                CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(examplePath, "Centroid.dyf"),
                    true,
                    out info));
            string openPath = Path.Combine(examplePath, "TestCentroid.dyn");

            RunModel(openPath);

            AssertPreviewValue("6ad5aa92-b3f5-492f-aa7c-4ae307587967", 5.5);
            AssertPreviewValue("7095a283-62e8-4f95-b1bf-f8919b700c96", 3.0);
            AssertPreviewValue("7a4b9510-c64c-48cb-81c7-24616cec56fc", 0.0);
        }

        [Test]
        public void CustomNodeWithSimpleGeometry()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\CustomNodes\");

            CustomNodeInfo info;
            Assert.IsTrue(
                CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(examplePath, "Point.dyf"),
                    true,
                    out info));
            string openPath = Path.Combine(examplePath, "TestPoint.dyn");

            RunModel(openPath);

            AssertPreviewValue("5ed80f52-ea60-4a07-8dd0-514f0eb70a28", 2);
        }

        [Test, Category("Failure")]
        public void CustomNodeMultipleInGraph()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\CustomNodes\");

            var dyfPath = Path.Combine(examplePath, "Poly.dyf");
            CustomNodeInfo info;
            Assert.IsTrue(CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNode(dyfPath, true, out info));

            RunModel(Path.Combine(examplePath, "TestPoly.dyn"));

            AssertPreviewValue("8453b5c7-2efc-4ff2-a8f3-7c376d22c240", 5.5);
            AssertPreviewValue("a9868848-0443-431b-bedd-9f63c25157e0", 3.0);
            AssertPreviewValue("9b569c4f-1f09-4ffb-a621-d0341f1fe890", 0.0);
        }

        [Test]
        public void CustomNodeConditional()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\CustomNodes\");

            CustomNodeInfo info;
            Assert.IsTrue(
                CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNode(Path.Combine(examplePath, "Conditional.dyf"), true, out info));

            string openPath = Path.Combine(examplePath, "TestConditional.dyn");

            RunModel(openPath);
            
            AssertPreviewValue("ec2e79de-35ed-44ad-9dea-4bedc526c612", false);
            AssertPreviewValue("7be13594-8d09-4377-98aa-d3cf1c716288", true);
        }

        [Test]
        public void TestProxyCustomNode()
        {
            // foobar.dyn reference to bar.dyf, bar.dyf references to foo.dyf
            // which cannot be found, so foo.dyf would be a proxy custom node,
            // as opening a dyn file will compile all custom nodes, the 
            // compilation of that proxy custom node should have any problem.
            var examplePath = Path.Combine(TestDirectory, @"core\CustomNodes\");

            CustomNodeInfo info;
            Assert.IsTrue(
                CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNode(Path.Combine(examplePath, "bar.dyf"), true, out info));

            string openPath = Path.Combine(examplePath, "foobar.dyn");

            Assert.DoesNotThrow(() => RunModel(openPath));
        }

        [Test, Category("Failure")]
        public void Regress_Magn_4837()
        {
            // Test nested custom node: run and reset engine and re-run.
            // Original defect: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4837

            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\Regress_Magn_4837.dyn");

            RunModel(dynFilePath);

            AssertPreviewValue("42693721-622d-475e-a82e-bfe793ddc153", new object[] { 2, 3, 4, 5, 6 });

            // Reset engine and mark all nodes as dirty. A.k.a., force re-execute.
            CurrentDynamoModel.ForceRun();

            AssertPreviewValue("42693721-622d-475e-a82e-bfe793ddc153", new object[] { 2, 3, 4, 5, 6 });
        }

        [Test, Category("Failure")]
        public void Regression_Magn_10015()
        {
            // no crash
            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\10015.dyn");
            RunModel(dynFilePath);
            AssertPreviewValue("deb457c6-1b4b-4703-9476-db312b34a8e2", new object[] { null, null, null, null });
        }

        [Test]
        public void LogicUINodesDeleted()
        {
            RunModel(@"core\dsevaluation\testuilogicnodes.dyn");
            this.CurrentDynamoModel.CurrentWorkspace.RequestRun();
            var codeblock = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.Name == "Code Block").First();
            var uiANDnode = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.Name == "And").First();
            var ztANDnode = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.Name == "&&").First();
            AssertPreviewValue(ztANDnode.GUID.ToString(),  true);


            //delete binary expression AND node
            this.CurrentDynamoModel.CurrentWorkspace.RemoveAndDisposeNode(uiANDnode);
            this.CurrentDynamoModel.CurrentWorkspace.RequestRun();

            //assert other node value is still valid
            AssertPreviewValue(ztANDnode.GUID.ToString(), true);
            AssertPreviewValue(codeblock.GUID.ToString(), true);
        }
    }

    [Category("GithubIssues")]
    class GithubIssueRegressionTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSCPython.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void Issue6729()
        {
            // (1..5) + 1;
            RunModel(@"core\dsevaluation\githubissue_6729.dyn");
            AssertPreviewValue("a1a9e8a5-a791-4924-82e1-0dc6dd2215ed", new int[] { 4, 5 });
        }

        [Test]
        //String.split yields different results with linebreak separator
        public void Issue3446()
        {   
            
            RunModel(@"core\dsevaluation\githubissue_3446.dyn");

            // Test CBN '"\n";' value
            AssertPreviewValue("07658815-d706-42b4-bb29-b0a65986c58d", new int[] { 1, 2, 3, 4, 5, 6 });

            // Test String Newline value
            AssertPreviewValue("6713ee10-a5e7-4049-b514-4388c7e09105", new int[] { 1, 2, 3, 4, 5, 6 });
            AssertSamePreviewValues("07658815-d706-42b4-bb29-b0a65986c58d", "6713ee10-a5e7-4049-b514-4388c7e09105");

            // Test case when CBN has a string - using both CBN and Strings as separators
            AssertPreviewValue("803d08aa-ca57-41ba-85a9-50fc47104427", new int[] { 1, 2, 3, 4, 5, 6 });
            AssertSamePreviewValues("07658815-d706-42b4-bb29-b0a65986c58d", "803d08aa-ca57-41ba-85a9-50fc47104427");

            AssertPreviewValue("39447de2-f20c-4482-8f08-50c5001ed971", new int[] { 1, 2, 3, 4, 5, 6 });
            AssertSamePreviewValues("07658815-d706-42b4-bb29-b0a65986c58d", "39447de2-f20c-4482-8f08-50c5001ed971");
        }
    }
}
