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

namespace Dynamo.Tests
{
    public class DSEvaluationUnitTest : DynamoUnitTest
    {
        public void OpenModel(string relativeFilePath)
        {
            var model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), relativeFilePath);
            model.Open(openPath);
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

        private string GetVarName(string guid)
        {
            var model = Controller.DynamoModel;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return  node.VariableToPreview;
        }

        private RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = Controller.EngineController.GetMirror(varName));
            return mirror;
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

        private void AssertValue(MirrorData data, object value)
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
            AssertPreviewValue("f83a463d-1ca4-4586-a544-5df47697e483", new int[] { 0, 100, 4, 6, 8, 10 });
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
            AssertPreviewValue("60e002e2-e723-4e39-b059-d761596f24da", new int[] { 0, 1, 100, 3, 4, 5, 6, 7, 8, 9, 10 });
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
            AssertValue("a", 1);
            AssertValue("b", 2);
        }

        [Test]
        public void CBN_Multiline_614_2()
        {
            // With empty line 
            // a=1;
            // b=2;

            RunModel(@"core\dsevaluation\multiline_614_2.dyn");
            AssertValue("a", 1);
            AssertValue("b", 2);
        }
        [Test]
        public void CBN_String_599()
        {
            // With empty line 
            // a="Dynamo";

            RunModel(@"core\dsevaluation\CBN_string_599.dyn");
            AssertValue("a", "Dynamo");
            
        }
        [Test]
        public void CBN_Create_697()
        {
        // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-697
            // a=1;
            RunModel(@"core\dsevaluation\CBN_Create_697.dyn");
            AssertValue("a", 1);
        }
        [Test]
        public void CBN_Math_Pi_621()
        {
        // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-621
            RunModel(@"core\dsevaluation\CBN_Math_Pi_621.dyn");
            AssertValue("a", Math.PI);

        }
        
        [Test]
        public void CBN_Multiple_Assignment614()
        {
        // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-614
            RunModel(@"core\dsevaluation\CBN_Multiple_Assignement_614.dyn");
            AssertValue("a", 1);
            AssertValue("b", 1);

        }
        [Test]
        public void CBN_Conditional_612()
        {
        // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-612
            RunModel(@"core\dsevaluation\CBN_Conditional_612.dyn");
            AssertValue("a", new int[] { 1,2 });
            
        }
        [Test]
        public void CBN_Reference_593()
        {
        // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-593
            RunModel(@"core\dsevaluation\CBN_Reference_593.dyn");
            AssertValue("b", 3);

        }
        [Test]
        public void CBN_Reference_593_2()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-593
            RunModel(@"core\dsevaluation\CBN_Reference_593_2.dyn");
            AssertValue("b", new int[] {1,2,3,4,5});

        }
        [Test]
        public void CBN_Binary_607()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-607
            RunModel(@"core\dsevaluation\CBN_binary_607.dyn");
            AssertValue("c", true);

        }
        [Test]
        public void CBN_Multiple_binary_607()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-607
            RunModel(@"core\dsevaluation\CBN_multiple_binary_607.dyn");
            AssertValue("c", true);

        }
        [Test]
        public void CBN_Conditionals_597()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-597
            RunModel(@"core\dsevaluation\CBN_conditionals_597.dyn");
            AssertValue("b", 1);

        }
        [Test]
        public void CBN_Conditionals_597_2()
        {
        // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-597
            RunModel(@"core\dsevaluation\CBN_conditionals_597_2.dyn");
            AssertValue("b", 5);

        }
        [Test]
        public void CBN_Nested_Conditionals_608()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-608
            RunModel(@"core\dsevaluation\CBN_nested_Conditionals_608.dyn");
            AssertValue("a", false);

        }
        [Test]
        public void CBN_Nested_Conditionals_608_2()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-608
            RunModel(@"core\dsevaluation\CBN_nested_Conditionals_608_2.dyn");
            AssertValue("a", false);

        }
        [Test]
        public void CBN_Nested_Conditionals_612()
        {
        // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-612
            RunModel(@"core\dsevaluation\CBN_ConditionalWithExpression_612.dyn");
            AssertValue("a", new int[] { 1, 2 });

        }
        [Test]
        public void CBN_Nested_Conditionals_612_2()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-612
            RunModel(@"core\dsevaluation\CBN_ConditionalWithExpression_612_2.dyn");
            AssertValue("a", new int[] { 1, 2 });

        }
        [Test]
        public void CBN_Multiline_705()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-705
            RunModel(@"core\dsevaluation\CBN_multiline_705.dyn");
            AssertValue("c",3);

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
            AssertValue("b", 0.00);

        }

        [Test]
        public void CBN_Geometry_RangeExpression_609()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-609
            RunModel(@"core\dsevaluation\CBN_Geometry_RangeExpression_609.dyn");
            AssertValue("b", new int[] {1,2});

        }
        [Test]
        public void CBN_Geometry_Expression_609_2()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-609
            RunModel(@"core\dsevaluation\CBN_Geometry_Expression_609_2.dyn");
            AssertValue("b", 3);

        }
        [Test]
        public void CBN_Geometry_Conditional_609_3()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-609
            RunModel(@"core\dsevaluation\CBN_Geometry_Conditional_609_3.dyn");
            AssertValue("b", 3.00);

        }
        [Test]
        public void CBN_Geometry_Array_609_4()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-609
            RunModel(@"core\dsevaluation\CBN_Geometry_Array_609_4.dyn");
            AssertValue("b", new double[] {1.0,2.0});

        }
        [Test]
        public void CBN_Double_Array_330()
        {

            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-330
            RunModel(@"core\dsevaluation\CBN_Double_Array_330.dyn");
            AssertValue("y", 4);

        }
             
        [Test]
        public void CBN_Dynamic_Array_622()
        {

            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-622
            RunModel(@"core\dsevaluation\CBN_Dynamic_Array_622.dyn");
            AssertValue("y", new int[] {1});
            AssertValue("c", 1 );

        }
        [Test]
        public void CBN_Dynamic_Array_622_2()
        {

        //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-622
            RunModel(@"core\dsevaluation\CBN_Dynamic_Array_622_2.dyn");
            AssertValue("z", 4);

        }
        [Test]
        public void CBN_Dynamic_Array_592()
        {

            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-592
            RunModel(@"core\dsevaluation\CBN_nestedrange592.dyn");
            AssertValue("a", new int[][] {new int[]{1,2,3,4,5,6,7,8,9,10,11},new int[]{2,4,6,8,10,12},new int[] {3,6,9,12}});

        }
        [Test]
        public void CBN_Dynamic_Array_592_2()
        {

            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-592
            RunModel(@"core\dsevaluation\CBN_nestedrange592_2.dyn");
            AssertValue("a", new int[][] { new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, new int[] { 2, 3, 4, 5, 6, 7,8,9,10 }, new int[] { 3, 4,5,6,7,8,9,10 } });
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
            AssertValue("a", new int[][] { new int[] { 1, 2 } });
        }
        [Test]
        public void CBN_Array_Range_629_2()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-629
            RunModel(@"core\dsevaluation\CBN_Array_Range_629_2.dyn");
            AssertValue("a",  new int[]{4});
        }
        [Test]
        public void CBN_Empty_722()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-722
            RunModel(@"core\dsevaluation\CBN_Empty_722.dyn");
            AssertValue("a",  10.00);
        }
        [Test]
        public void CBN_array_indexnull_619()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-722
            RunModel(@"core\dsevaluation\CBN_array_indexnull_619.dyn");
            AssertValue("a",  new int []{5});
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
            RunModel(@"core\dsevaluation\Defect_MAGN_844.dyn");
            AssertPreviewValue("8de1b8aa-c6c3-4360-9619-fe9d01a804f8", 1);

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
            AssertValue("a", new int[] { 3, 2, 3 });
        }

        [Test]
        public void UsingFunctionObject01()
        {
            RunModel(@"core\dsevaluation\FunctionObject.dyn");
            AssertValue("r", 45);
        }

        [Test]
        public void UsingFunctionObject02()
        {
            RunModel(@"core\dsevaluation\Apply.dyn");
            AssertPreviewValue("11b2c7b2-2854-4e46-a8fa-4d1d52ebf4b7", 20);
        }
    }
}