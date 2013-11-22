﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.DSEngine;
using Dynamo.Utilities;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.Mirror;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class DSEvaluationTest : DynamoUnitTest
    {
        private void OpenModel(string relativeFilePath)
        {
            var model = Controller.DynamoModel;
            string openPath = Path.Combine(GetTestDirectory(), relativeFilePath);
            model.Open(openPath);
        }

        private void RunModel(string relativeDynFilePath)
        {
            OpenModel(relativeDynFilePath);
            Assert.DoesNotThrow(() => Controller.RunExpression(null));
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
            Assert.IsNotNull(mirror);
            return mirror;
        }

        private void AssertValue(string varname, object value)
        {
            var mirror = GetRuntimeMirror(varname);

            Console.WriteLine(varname + " = " + mirror.GetStringData());
            StackValue svValue = mirror.GetData().GetStackValue();

            if (value == null)
            {
                Assert.IsTrue(StackUtils.IsNull(svValue));
            }
            else if (value is double)
            {
                Assert.AreEqual(svValue.opdata_d, Convert.ToDouble(value));
            }
            else if (value is int)
            {
                Assert.AreEqual(svValue.opdata, Convert.ToInt64(value));
            }
            else if (value is IEnumerable<int>)
            {
                var values = (value as IEnumerable<int>).ToList().Select(v => (object)v).ToList();
                Assert.IsTrue(mirror.GetUtils().CompareArrays(varname, values, typeof(Int64)));
            }
            else if (value is IEnumerable<double>)
            {
                var values = (value as IEnumerable<double>).ToList().Select(v => (object)v).ToList();
                Assert.IsTrue(mirror.GetUtils().CompareArrays(varname, values, typeof(double)));
            }
        }

        private void AssertPreviewValue(string guid, object value)
        {
            string previewVariable = GetVarName(guid);
            AssertValue(previewVariable, value);
        }

        private void AssertIsPointer(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);

            StackValue svValue = mirror.GetData().GetStackValue();
            Assert.IsTrue(StackUtils.IsValidPointer(svValue));
        }

        [TearDown]
        public override void Cleanup()
        {
            GraphToDSCompiler.GraphUtilities.CleanUp();
            base.Cleanup();
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
            AssertIsPointer("8774296c-5269-450b-959d-ce4020ddbf80");
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
            // a=1;
            // b=2;

            RunModel(@"core\dsevaluation\CBN_string_599.dyn");
            AssertValue("a", "Dynamo");
            
        }
        [Test]
        public void CBN_Create_697()
        {
        // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-697
            // a=1;
            RunModel(@"core\dsevaluation\CBN_Create_697.dyn");
            AssertValue("a", "Dynamo");
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
            RunModel(@"core\dsevaluation\CBN_nestedrange592_2.dyn");
            AssertValue("a", null);
        }
        [Test]
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
    }
}