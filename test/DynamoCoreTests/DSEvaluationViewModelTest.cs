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
using System.Collections;
using Dynamo.Models;
using Dynamo.Nodes;

namespace Dynamo.Tests
{
    public class DSEvaluationViewModelUnitTest : DynamoViewModelUnitTest
    {
        protected LibraryServices libraryServices = null;
        protected ProtoCore.Core libraryServicesCore = null;

        public override void Init()
        {
            base.Init();

            var options = new ProtoCore.Options();
            options.RootModulePathName = string.Empty;
            libraryServicesCore = new ProtoCore.Core(options);
            libraryServicesCore.Executives.Add(ProtoCore.Language.kAssociative,
                new ProtoAssociative.Executive(libraryServicesCore));
            libraryServicesCore.Executives.Add(ProtoCore.Language.kImperative,
                new ProtoImperative.Executive(libraryServicesCore));

            libraryServices = new LibraryServices(libraryServicesCore);
        }

        public void OpenModel(string relativeFilePath)
        {
            string openPath = Path.Combine(GetTestDirectory(), relativeFilePath);
            ViewModel.OpenCommand.Execute(openPath);
        }

        public void OpenSampleModel(string relativeFilePath)
        {
            string openPath = Path.Combine(GetSampleDirectory(), relativeFilePath);
            ViewModel.OpenCommand.Execute(openPath);
        }

        public void RunModel(string relativeDynFilePath)
        {
            OpenModel(relativeDynFilePath);
            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());
        }

        public void RunCurrentModel() // Run currently loaded model.
        {
            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());
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

        public void AssertInfinity(string dsVariable, int startBlock = 0)
        {
            RuntimeMirror mirror = GetRuntimeMirror(dsVariable);
            MirrorData data = mirror.GetData();
            Assert.IsTrue(System.Double.IsInfinity(Convert.ToDouble(data.Data)));
        }

        public void AssertPreviewValue(string guid, object value)
        {
            string previewVariable = GetVarName(guid);
            AssertValue(previewVariable, value);
        }

        /// <summary>
        /// Compares preview value of two nodes and asserts they are same.
        /// </summary>
        /// <param name="guid1">guid for first node</param>
        /// <param name="guid2">guid for second node</param>
        public void AssertSamePreviewValues(string guid1, string guid2)
        {
            string var1 = GetVarName(guid1);
            var data1 = GetRuntimeMirror(var1).GetData();
            string var2 = GetVarName(guid2);
            var data2 = GetRuntimeMirror(var2).GetData();
            AssertMirrorData(data1, data2);
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
            Assert.IsTrue(data.IsCollection, "preview data is not a list");
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

        private void AssertMirrorData(MirrorData data1, MirrorData data2)
        {
            if (data1.IsNull)
                Assert.True(data2.IsNull);
            else if (data1.IsCollection)
            {
                Assert.True(data2.IsCollection);
                List<MirrorData> elems1 = data1.GetElements();
                List<MirrorData> elems2 = data2.GetElements();
                Assert.AreEqual(elems1.Count, elems2.Count);
                int i = 0;
                foreach (var item in elems1)
                {
                    AssertMirrorData(item, elems2[i++]);
                }
            }
            else
                Assert.AreEqual(data1.Data, data2.Data);
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
            if (libraryServicesCore != null)
            {
                libraryServicesCore.Cleanup();
                libraryServicesCore = null;
            }
            libraryServices = null;

            base.Cleanup();
            DynamoUtilities.DynamoPathManager.DestroyInstance();
        }
    }

    [Category("DSExecution")]
    class DSEvaluationViewModelTest : DSEvaluationViewModelUnitTest
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
            var model = ViewModel.Model;

            RunModel(@"core\dsevaluation\Defect_MAGN_829_1.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count());
            Assert.Pass("Execution completed successfully");

        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_829_2()
        {
            // CBN ==> 1=1=a;
            var model = ViewModel.Model;

            RunModel(@"core\dsevaluation\Defect_MAGN_829_2.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count());
            Assert.Pass("Execution completed successfully");
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_829_3()
        {
            // CBN ==> a=1=2=3;
            var model = ViewModel.Model;

            RunModel(@"core\dsevaluation\Defect_MAGN_829_3.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count());
            Assert.Pass("Execution completed successfully");
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_829_4()
        {
            // CBN ==> a*a=1;;
            var model = ViewModel.Model;

            RunModel(@"core\dsevaluation\Defect_MAGN_829_4.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count());
            Assert.Pass("Execution completed successfully");
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_829_5()
        {
            // Multiline CBN ==> a=1;
            //               ==> 1 = a;
            var model = ViewModel.Model;

            RunModel(@"core\dsevaluation\Defect_MAGN_829_5.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, model.CurrentWorkspace.Connectors.Count());
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
            var guid = System.Guid.Parse("0fc83562-2cfe-4a63-84f8-f6836cbaf9c5");
            var node = ViewModel.HomeSpace.Nodes.FirstOrDefault(n => n.GUID == guid);
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
            var model = ViewModel.Model;

            RunModel(@"core\dsevaluation\Defect_MAGN_2479.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count());
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
            var model = ViewModel.Model;

            RunModel(@"core\dsevaluation\Defect_MAGN_2375_3487.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count());

            model.AddToSelection(ViewModel.Model.CurrentWorkspace.NodeFromWorkspace
                ("5a7f7549-fbef-4c3f-8578-c67471eaa87f"));

            model.Copy();
            model.Paste();

            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count());

            //run the graph after copy paste
            ViewModel.HomeSpace.Run();

            var nodes = ViewModel.Model.CurrentWorkspace.Nodes.OfType<DSVarArgFunction>();
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
        [Category("Failure")]
        [Category("RegressionTests")]
        public void Defect_MAGN_3264()
        {
            // Function object to property method and used in apply 
            RunModel(@"core\dsevaluation\Defect_MAGN_3264.dyn");
            AssertPreviewValue("eaa2b29f-b5f4-4017-a143-3fb2d4af349c", new double[] {0, 1, 2, 3, 4});
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

            AssertPreviewValue("d9b9d0a9-1fec-4b20-82c4-2d1665306509", new int[] { 4, 6, 7});
            AssertPreviewValue("c35f1c6d-b955-4638-802f-208f93112078", new object[] { new int[] { 4, 5, 6}, new int[] { 5, 6, 7}});
        }

        [Test]
        public void CBNAndFormula()
        {
            RunModel(@"core\dsevaluation\CBNWithFormula.dyn");
            var id =
                ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>().GUID;
            AssertPreviewValue(id.ToString(), 3);
        }

        [Test]
        public void Test_Formula_InputWithUnit()
        {
            RunModel(@"core\formula\formula-inputWithUnit-test.dyn");
            AssertPreviewValue("152a2a64-8c73-4e8c-a418-06ceb4ac0637", 1);
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
            AssertPreviewValue("70b5aeba-80b7-48cc-a48d-6c755c28555c", new object []{ 1, 1, 1, new object []{ -2, -1, 0, 1, 2 },new object[] { -2, -1, 0, 1, 2 } });
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
            AssertPreviewValue("dace0b8c-381e-4de5-824d-c1651ec94bc6", new double[] {1,1,1,1,1,2,3});
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
            AssertPreviewValue("80d14b69-4796-48c9-a34d-f447abf7b5ba", new object[] {1,1,new double[]{-2,-1,0,1,2},1,1});
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
            var model = ViewModel.Model;

            RunModel(@"core\dsevaluation\Test_IfINCustomNode_4058.dyn");

            AssertPreviewValue("4c70f814-5c42-4fb9-89b0-b3cfe6f93b6d", 6.00);
        }
        [Test]
        [Category("RegressionTests")]
        public void TestSingleIFRecusion_4058()
        {
            var model = ViewModel.Model;
            RunModel(@"core\dsevaluation\TestSingleIFRecusion_4058.dyn");
            AssertPreviewValue("f2b979b2-7824-428c-a960-5e7ca8cac1f1", 4);
            
        }
        [Test]
        [Category("RegressionTests")]
        public void TestMultipleIFCN_4058()
        {
            var model = ViewModel.Model;
            

            RunModel(@"core\dsevaluation\testMultipleIFCN_4058.dyn");

            AssertPreviewValue("201866a4-e368-41e9-8264-bf4d8fb65ed1", 10);

        }
        [Test]
        [Category("RegressionTests")]
        public void TestMultipleIFRecursion_4058()
        {
            var model = ViewModel.Model;
            Assert.Inconclusive("MultipleIF Recursion ");

            RunModel(@"core\dsevaluation\testMultipleIFRecursion_4058.dyn");

            AssertPreviewValue("201866a4-e368-41e9-8264-bf4d8fb65ed1", new double []{ 1,1,2,3,5});

        }
        [Test]
        [Category("RegressionTests")]
        [Category("Failure")]
        public void TestNestedIFNORecursion_4058()
        {
            var model = ViewModel.Model;

            RunModel(@"core\dsevaluation\TestNestedIfNoRecursion_4058.dyn");

            AssertPreviewValue("f7f34898-2eb3-48fb-9a65-6084446dfbd0", 11);

        }
        [Test]
        [Category("RegressionTests")]
        public void TestNestedIFRecursion_4058()
        {
            var model = ViewModel.Model;
            RunModel(@"core\dsevaluation\NestedIFRecursion_4058.dyn");
            AssertPreviewValue("2a09f286-b0fe-443a-be87-591f5c6e9264", "Odd");

        }
        [Test]
        [Category("RegressionTests")]
        public void NestedIFRecursionMultiple_4058()
        {
            
            var model = ViewModel.Model;
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

            var model = ViewModel.Model;

            RunModel(@"core\dsevaluation\createCube_codeBlockNode.dyn");
            AssertPreviewValue("3669d05c-c741-44f9-87ab-8961e7f5f112", 150);
            var guid = System.Guid.Parse("3669d05c-c741-44f9-87ab-8961e7f5f112");
            var node = ViewModel.HomeSpace.Nodes.FirstOrDefault(n => n.GUID == guid);
            Assert.IsTrue(node.State != Models.ElementState.Warning);


        }
        [Test]
        [Category("RegressionTests")]
        public void DoubleToInt_NoWarning_5109()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5109
            //verify  Warning converting double to int is removed
            RunModel(@"core\dsevaluation\DoubleToInt_5109.dyn");
            var guid = System.Guid.Parse("d66d3d3e-e13b-460e-a8a7-056c434ee620");
            var node = ViewModel.HomeSpace.Nodes.FirstOrDefault(n => n.GUID == guid);
            Assert.IsTrue(node.State != Models.ElementState.Warning);
        }
       

     
        [Test]
        [Category("RegressionTests")]
        public void CBN_Variable_Type_5480()
        {
            // MAGN-5480 - Defect in parsing typed identifiers in CBN

            var model = ViewModel.Model;

            RunModel(@"core\dsevaluation\CBN_Variable_Type_5480.dyn");
            AssertPreviewValue("fabaccff-5b8a-4505-b752-7939cba90dc4", 1);
        }

        [Test]
        public void TestDefaultValueInFunctionObject()
        {
            // Regression test case for
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5233
            var dynFilePath = Path.Combine(GetTestDirectory(), @"core\default_values\defaultValueInFunctionObject.dyn");

            RunModel(dynFilePath);

            AssertPreviewValue("4218d135-a2c4-4dee-8415-8f0bf1de671c", new[] { 1, 1 });


        }
        [Test]
        public void TestRunTimeWarning_3132()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3132
            // test for run time warning is thrown or not 

            var dynFilePath = Path.Combine(GetTestDirectory(), @"core\dsfunction\RunTimeWarning_3132.dyn");

            RunModel(dynFilePath);
            var guid = System.Guid.Parse("88f376fa-634b-422e-b853-6afa8af8d286");
            var node = ViewModel.HomeSpace.Nodes.FirstOrDefault(n => n.GUID == guid);
           
            Assert.IsTrue(node.State == Models.ElementState.Warning);
        }
        
        [Test]
        public void List_Map_Default_5233()
        {
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5233
            //List.map with default arguments 

            var dynFilePath = Path.Combine(GetTestDirectory(), @"core\list\List_Map_DefaultArg5233.dyn");
            RunModel(dynFilePath);
            AssertPreviewValue("6a0207d9-78d7-4fd3-829f-d19644acdc1b", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }
    }

    [Category("DSCustomNode")]
    class CustomNodeEvaluationViewModel : DSEvaluationViewModelUnitTest
    {
        [Test]
        public void CustomNodeNoInput01()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            CustomNodeInfo info;
            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(examplePath, "NoInput.dyf"),
                    true,
                    out info));

            string openPath = Path.Combine(examplePath, "TestNoInput.dyn");
            //model.Open(openPath);

            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(1, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);

            AssertPreviewValue("f9c6aa7f-3fb4-40df-b4c5-6694e8c437cd", 
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }
        [Test]
        public void CustomNodeWithInput02()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            CustomNodeInfo info;
            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(examplePath, "CNWithInput.dyf"),
                    true,
                    out info));

            string openPath = Path.Combine(examplePath, "TestCNWithInput.dyn");
            //model.Open(openPath);

            RunModel(openPath);

            // check all the nodes and connectors are loaded

            
            AssertPreviewValue("1bee0f0f-5c93-48b3-a90d-f8761fa6e221", 3);
        }
        [Test]
        public void CustomNodeWithCBNAndGeometry()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            CustomNodeInfo info;
            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(
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
        public void CustomNodeMultipleInGraph()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            var dyfPath = Path.Combine(examplePath, "Poly.dyf");
            CustomNodeInfo info;
            Assert.IsTrue(ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(dyfPath, true, out info));

            RunModel(Path.Combine(examplePath, "TestPoly.dyn"));
            
            AssertPreviewValue("8453b5c7-2efc-4ff2-a8f3-7c376d22c240", 5.5);
            AssertPreviewValue("a9868848-0443-431b-bedd-9f63c25157e0", 3.0);
            AssertPreviewValue("9b569c4f-1f09-4ffb-a621-d0341f1fe890", 0.0);
        }

        [Test]
        public void CustomNodeConditional()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            CustomNodeInfo info;
            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(Path.Combine(examplePath, "Conditional.dyf"), true, out info));

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
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\");

            CustomNodeInfo info;
            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(Path.Combine(examplePath, "bar.dyf"), true, out info));

            string openPath = Path.Combine(examplePath, "foobar.dyn");

            Assert.DoesNotThrow(() => RunModel(openPath));
        }

        [Test]
        public void Regress_Magn_4837()
        {
            // Test nested custom node: run and reset engine and re-run.
            // Original defect: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4837

            var dynFilePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\Regress_Magn_4837.dyn");

            RunModel(dynFilePath);
 
            AssertPreviewValue("42693721-622d-475e-a82e-bfe793ddc153", new object[] {2, 3, 4, 5, 6});

            // Reset engine and mark all nodes as dirty. A.k.a., force re-execute.
            ViewModel.Model.ForceRun();

            AssertPreviewValue("42693721-622d-475e-a82e-bfe793ddc153", new object[] {2, 3, 4, 5, 6});
        }
    }
}
