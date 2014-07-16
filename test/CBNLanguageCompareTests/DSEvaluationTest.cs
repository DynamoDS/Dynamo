using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.DSEngine;
using Dynamo.Utilities;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using System.Collections;
using Dynamo.Models;
using Dynamo.Nodes;

namespace Dynamo.Tests
{
    public class DSEvaluationUnitTest : DynamoUnitTest
    {
        public void OpenModel(string relativeFilePath)
        {
            string openPath = Path.Combine(GetTestDirectory(), relativeFilePath);
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);
        }

        public void RunModel(string relativeDynFilePath)
        {
            OpenModel(relativeDynFilePath);
            Assert.DoesNotThrow(() => Controller.RunExpression(null));
        }

        public void RunCurrentModel() // Run currently loaded model.
        {
            Assert.DoesNotThrow(() => Controller.RunExpression(null));
        }

        /// <summary>
        /// To selectively verify the result, which is a collection, at some
        /// positions.
        /// </summary>
        /// <param name="varname"></param>
        /// <param name="selectedValues">Values to verify</param>
        public void SelectivelyAssertValues(string varname, Dictionary<int, object> selectedValues)
        {
            var mirror = GetRuntimeMirror(varname);
            //Couldn't find the variable, so expected value should be null.
            if (mirror == null)
            {
                if (selectedValues!= null)
                    Assert.IsNotNull(mirror, string.Format("Variable : {0}, not found.", varname));
                return;
            }

            Console.WriteLine(varname + " = " + mirror.GetStringData());
            var svValue = mirror.GetData();
            SelectivelyAssertValues(svValue, selectedValues);
        }

        public void AssertValue(string varname, object value)
        {
            var mirror = GetRuntimeMirror(varname);
            //Couldn't find the variable, so expected value should be null.
            if (mirror == null)
            {
                if (value != null)
                    Assert.IsNotNull(mirror, string.Format("Variable : {0}, not found.", varname));
                return;
            }

            Console.WriteLine(varname + " = " + mirror.GetStringData());
            var svValue = mirror.GetData();
            AssertValue(svValue, value);
        }

        public void AssertPreviewValue(string guid, object value)
        {
            string previewVariable = GetVarName(guid);
            AssertValue(previewVariable, value);
        }

        public void SelectivelyAssertPreviewValues(string guid, Dictionary<int, object> selectedValue)
        {
            string previewVariable = GetVarName(guid);
            SelectivelyAssertValues(previewVariable, selectedValue);
        }

        public void AssertClassName(string guid, string className)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var classInfo = mirror.GetData().Class;
            Assert.AreEqual(classInfo.ClassName, className);
        }

        public void AssertPreviewCount(string guid, int count)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            var data = mirror.GetData();
            Assert.IsTrue(data.IsCollection);
            Assert.AreEqual(count, data.GetElements().Count);
        }

        public object GetPreviewValue(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            return mirror.GetData().Data;
        }

        public object GetPreviewValueAtIndex(string guid, int index)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            return mirror.GetData().GetElements()[index].Data;
        }

        private void SelectivelyAssertValues(MirrorData data, Dictionary<int, object> selectedValues)
        {
            Assert.IsTrue(data.IsCollection);

            if (data.IsCollection)
            {
                List<MirrorData> elements = data.GetElements();
                foreach (var pair in selectedValues)
                {
                    AssertValue(elements[pair.Key], pair.Value);
                }
            }
        }

        public void AssertValue(MirrorData data, object value)
        {
            if (data.IsCollection)
                AssertCollection(data, value as IEnumerable);
            else if (value == null)
                Assert.IsTrue(data.IsNull);
            else if (value is int)
                Assert.AreEqual((int)value, Convert.ToInt32(data.Data));
            else if (value is double)
                Assert.AreEqual((double)value, Convert.ToDouble(data.Data), 0.00001);
            else
                Assert.AreEqual(value, data.Data);
        }

        private void AssertCollection(MirrorData data, IEnumerable collection)
        {
            Assert.IsTrue(data.IsCollection);
            List<MirrorData> elements = data.GetElements();
            int i = 0;
            foreach (var item in collection)
            {
                AssertValue(elements[i++], item);
            }
        }

        public override void Cleanup()
        {
            Dynamo.DSEngine.LibraryServices.DestroyInstance();
            GraphToDSCompiler.GraphUtilities.Reset();
            base.Cleanup();
        }
    }

    [Category("DSExecution")]
    class DSEvaluationTest : DSEvaluationUnitTest
    {
        [Test]
        public void TestCodeBlockNode01()
        {
            // (1..5) + 1;
            RunModel(@"core\dsevaluation\cbn_nolhs.dyn");
            AssertPreviewValue("4e88b4a9-397a-422e-be13-f9ffcb27bc75", new int[] { 2, 3, 4, 5, 6 });
        }

        [Test]
        [Category("Failing")]
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
        [Category("Failing")]
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
        [Category("Failing")]
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
            AssertPreviewValue("d59b7582-7d01-41b5-ad25-d6133e85cd58",3);

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
            AssertValue("y_1086551ad8514eccb9bf9eeba3e9f6bf", new int[] {1});
            AssertValue("c_1086551ad8514eccb9bf9eeba3e9f6bf", 1 );

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

        [Ignore] //Ignored because empty code block nodes should not exist
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
            AssertPreviewValue("f5b252f3-67b9-4287-a003-eb8d3b8f9cb2",  new int[]{4});
        }
        [Test]
        public void CBN_Empty_722()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-722
            RunModel(@"core\dsevaluation\CBN_Empty_722.dyn");
            AssertPreviewValue("25b8e1de-925a-46fb-9fcf-5d4100a5af0d",  10.00);
        }
        [Test]
        public void CBN_array_indexnull_619()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-722
            RunModel(@"core\dsevaluation\CBN_array_indexnull_619.dyn");
            AssertPreviewValue("6985948e-992c-4420-8c39-1f5f5d57dc64", new int[] { 5 });
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
        public void Defect_MAGN_844()
        {
            // CBN: a = 1;
            // CBN b = a; 

            RunModel(@"core\dsevaluation\Defect_MAGN_844.dyn");

            // Change in implementation for Local variable this test case needs update in final value.
            AssertPreviewValue("8de1b8aa-c6c3-4360-9619-fe9d01a804f8", null);

        }

        [Test]
        public void Defect_MAGN_829_1()
        {
            // CBN ==> 1=a;
            var model = dynSettings.Controller.DynamoModel;

            RunModel(@"core\dsevaluation\Defect_MAGN_829_1.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count);
            Assert.Pass("Execution completed successfully");

        }

        [Test]
        public void Defect_MAGN_829_2()
        {
            // CBN ==> 1=1=a;
            var model = dynSettings.Controller.DynamoModel;

            RunModel(@"core\dsevaluation\Defect_MAGN_829_2.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count);
            Assert.Pass("Execution completed successfully");
        }

        [Test]
        public void Defect_MAGN_829_3()
        {
            // CBN ==> a=1=2=3;
            var model = dynSettings.Controller.DynamoModel;

            RunModel(@"core\dsevaluation\Defect_MAGN_829_3.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count);
            Assert.Pass("Execution completed successfully");
        }

        [Test]
        public void Defect_MAGN_829_4()
        {
            // CBN ==> a*a=1;;
            var model = dynSettings.Controller.DynamoModel;

            RunModel(@"core\dsevaluation\Defect_MAGN_829_4.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count);
            Assert.Pass("Execution completed successfully");
        }

        [Test]
        public void Defect_MAGN_829_5()
        {
            // Multiline CBN ==> a=1;
            //               ==> 1 = a;
            var model = dynSettings.Controller.DynamoModel;

            RunModel(@"core\dsevaluation\Defect_MAGN_829_5.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count);
            Assert.Pass("Execution completed successfully");
        }

        [Test]
        public void Defect_MAGN_610()
        {
            // Multiline CBN ==> a={1,2,3};
            //               ==> a[0]= 3;
            RunModel(@"core\dsevaluation\Defect_MAGN_610.dyn");
            AssertPreviewValue("aa78716b-f3f6-4676-bb72-2cb1c34181f8", new int[] { 3, 2, 3 });
            AssertValue("a_aa78716bf3f64676bb722cb1c34181f8", new int[] { 3, 2, 3 });
        }

        [Test]
        public void UsingFunctionObject01()
        {
            RunModel(@"core\dsevaluation\FunctionObject.dyn");
            AssertPreviewValue("5dad688a-e6f3-4153-b87e-d1713b645de9", 45);
        }

        [Test]
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
            var guid = System.Guid.Parse("0fc83562-2cfe-4a63-84f8-f6836cbaf9c5");
            var node = Controller.DynamoViewModel.Model.HomeSpace.Nodes.FirstOrDefault(n => n.GUID == guid);
            Assert.IsTrue(node.State == Models.ElementState.Warning);
        }

        [Test]
        public void NumberSequence()
        {
            RunModel(@"core\dsevaluation\NumberSequence.dyn");
            AssertPreviewValue("4d86876b-08a8-4166-b1f5-4194b8381dab", 
                new object[] {0.0, 1.0, 2.0, 3.0, 4.0});
        }

        [Test]
        public void Defect_MAGN_2363()
        {
            // CBN: 0..#3..10;
            RunModel(@"core\dsevaluation\Defect_MAGN_2363.dyn");
            AssertPreviewValue("f20147a0-21bb-4f23-b6bb-450591e62b31",
                new object[] { 0, 10, 20 });
        }

        [Test]
        public void Defect_MAGN_2479()
        {
            // Details are available in http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2479
            var model = dynSettings.Controller.DynamoModel;

            RunModel(@"core\dsevaluation\Defect_MAGN_2479.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);
            AssertPreviewValue("0ffe94bd-f926-4e81-83f7-7975e67a3713",
                new int[] { 2, 4, 6, 8, 10, 12, 14, 16 });
        }

        [Test]
        public void Defect_MAGN_2375_3487()
        {
            // This test case is addressing the following two defects:
            // Details are available in http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2375
            //                      and http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3487
            var model = dynSettings.Controller.DynamoModel;

            RunModel(@"core\dsevaluation\Defect_MAGN_2375_3487.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            model.AddToSelection(Controller.DynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("5a7f7549-fbef-4c3f-8578-c67471eaa87f"));

            model.Copy(null);

            model.Paste(null);

            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            //run the graph after copy paste
            dynSettings.Controller.RunExpression();

            var nodes = Controller.DynamoModel.Nodes.OfType<DSVarArgFunction>();
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
            AssertPreviewValue("abc4796e-b35d-4076-b6ff-2f814dda350f", new double[] { 1.0, 2.0, 3.0, 4.0, 5.0});
        }

        [Test]
        public void CBN_Geometry()
        {
            RunModel(@"core\dsevaluation\CBN_Geometry.dyn");
            AssertPreviewValue("9c51f2d5-a9f2-4825-bda6-f062e69efc46", 5.00000);
        }
        [Test]
        public void CBN_Range_1300()
        {
            RunModel(@"core\dsevaluation\CBN_Range_1300.dyn");
            AssertPreviewValue("372d31c2-b66e-494c-a33d-e82ae1ecf68a", new double[] { 0.000000, 2.000000, 10.392305, 16.000000, 10.392305, 2.000000, 0.000000, -2.000000, -10.392305, -16.000000, -10.392305, -2.000000, 0.000000 });
        }

        [Test]
        public void Apply_With_PropertyMethod()
        {
            // Function object to property method and used in apply 
            RunModel(@"core\dsevaluation\apply_property.dyn");
            AssertPreviewValue("2cbadbce-ee25-4f90-8309-3b81bf4fdfd9", 42.0);
        }

        [Test]
        public void Defect_MAGN_3264()
        {
            // Function object to property method and used in apply 
            RunModel(@"core\dsevaluation\Defect_MAGN_3264.dyn");
            AssertPreviewValue("eaa2b29f-b5f4-4017-a143-3fb2d4af349c", new double[] {0, 1, 2, 3, 4});
        }

        [Test]
        public void Defect_MAGN_2535()
        {
            // Function object to property method and used in apply 
            RunModel(@"core\dsevaluation\EvaluateFptrInOtherCBN.dyn");
            AssertPreviewValue("49048255-fc2c-463d-8e93-96e20f061a0d", 42);
        }

        [Test]
        public void Test_Formula_Lacing()
        {
            var model = dynSettings.Controller.DynamoModel;

            RunModel(@"core\formula\formula_lacing.dyn");

            AssertPreviewValue("d9b9d0a9-1fec-4b20-82c4-2d1665306509", new int[] { 4, 6, 7});
            AssertPreviewValue("c35f1c6d-b955-4638-802f-208f93112078", new object[] { new int[] { 4, 5, 6}, new int[] { 5, 6, 7}});
        }

        [Test]
        public void Formula_Simple()
        {
            
            RunModel(@"core\dsevaluation\SimpleFormula.dyn");
            AssertPreviewValue("6637546b-7998-4c48-bdb6-0bcf9f6ae997", new int[]{ 2, 6, 12, 20, 30 });
        }
        [Test]
        public void CBNAndFormula()
        {

            RunModel(@"core\dsevaluation\CBNWithFormula.dyn");
            AssertPreviewValue("60979b20-8089-4d5a-93bf-2cf829f74060", 3);
        }
        [Test]
        public void FormulaIntegration()
        {

            RunModel(@"core\dsevaluation\FormulaIntegration1.dyn");
            AssertPreviewValue("88d3bb73-42cd-4ffc-82e2-402c9550d5b1", new double[] { 0.000000, 0.001038, 0.002289, -0.007827, -0.035578, -0.046003, 0.034186, 0.216376, 0.315323, 0.000000 });
        }

        [Test]
        public void Test_Formula_InputWithUnit()
        {
            var model = dynSettings.Controller.DynamoModel;

            RunModel(@"core\formula\formula-inputWithUnit-test.dyn");

            AssertPreviewValue("152a2a64-8c73-4e8c-a418-06ceb4ac0637", 1);
        }
    }

    [Category("DSCustomNode")]
    class CustomNodeEvaluation : DSEvaluationUnitTest
    {
        [Test]
        public void CustomNodeNoInput01()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            Assert.IsTrue(
                Controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "NoInput.dyf"))
                != null);

            string openPath = Path.Combine(examplePath, "TestNoInput.dyn");
            //model.Open(openPath);

            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);

            AssertPreviewValue("f9c6aa7f-3fb4-40df-b4c5-6694e8c437cd", 
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }
        [Test]
        public void CustomNodeWithInput02()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            Assert.IsTrue(
                Controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "CNWithInput.dyf"))
                != null);

            string openPath = Path.Combine(examplePath, "TestCNWithInput.dyn");
            //model.Open(openPath);

            RunModel(openPath);

            // check all the nodes and connectors are loaded

            
            AssertPreviewValue("1bee0f0f-5c93-48b3-a90d-f8761fa6e221", 3);
        }
        [Test]
        public void CustomNodeWithCBNAndGeometry()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            Assert.IsTrue(
                Controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "Centroid.dyf"))
                != null);

            string openPath = Path.Combine(examplePath, "TestCentroid.dyn");
            //model.Open(openPath);

            RunModel(openPath);

            // check all the nodes and connectors are loaded
                       
            
            AssertPreviewValue("6542259f-b7c2-4a09-962b-7712ca269306", 0.00);
            AssertValue("x", 5.5);
            AssertValue("y", 3.0);
            AssertValue("z", 0.0);
        }

        [Test]
        public void CustomNodeMultipleInGraph()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            var dyfPath = Path.Combine(examplePath, "Poly.dyf");
            Assert.IsNotNull(Controller.CustomNodeManager.AddFileToPath(dyfPath));

            RunModel(Path.Combine(examplePath, "TestPoly.dyn"));

            AssertPreviewValue("6542259f-b7c2-4a09-962b-7712ca269306", 0.00);
            AssertValue("x", 5.5);
            AssertValue("y", 3.0);
            AssertValue("z", 0.0);
        }

        [Test]
        public void CustomNodeConditional()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            Assert.IsTrue(
                Controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "Conditional.dyf"))
                != null);

            string openPath = Path.Combine(examplePath, "TestConditional.dyn");
            //model.Open(openPath);

            RunModel(openPath);

            // check all the nodes and connectors are loaded


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
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            Assert.IsTrue(
                Controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "bar.dyf"))
                != null);

            string openPath = Path.Combine(examplePath, "foobar.dyn");

            Assert.DoesNotThrow(() => RunModel(openPath));
        }
    }
}
