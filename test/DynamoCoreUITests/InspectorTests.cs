using NUnit.Framework;
using System.IO;
using Dynamo.Nodes;
using System.Collections.Generic;
using DSCoreNodesUI;
using System;
using DynamoCoreUITests;
using System.Reflection;
using System.Linq;
using Dynamo.Utilities;

namespace Dynamo.Tests
{
    [TestFixture]
    class InspectorTests : DynamoTestUI
    {
        // use reflection to grab properties from DSFunction node
        private static List<object> BuildTestValuesFromReflection(DSFunction ColorNode)
        {
            var OutputID = ColorNode.GetAstIdentifierForOutputIndex(0).Name;

            var mirror = dynSettings.Controller.EngineController.GetMirror(OutputID);

            List<object> testvalues = new List<object>();

            if (mirror != null)
            {
                object color = null;
                {
                    color = mirror.GetData().Data;
                }

                var propertyInfos = color.GetType().GetProperties(
               BindingFlags.Public | BindingFlags.NonPublic // Get public and non-public
             | BindingFlags.Static | BindingFlags.Instance  // Get instance + static
             | BindingFlags.FlattenHierarchy); // Search up the hierarchy



                foreach (var prop in propertyInfos.ToList())
                {
                    var val = prop.GetValue(color, null);
                    testvalues.Add(val);
                }
            }
            return testvalues;
        }


        public void AssertMatchingItemNames(Inspector inspector, List<string> names)
        {

            foreach (DynamoDropDownItem item in inspector.Items)
            {

                Assert.AreEqual(item.Name, names[inspector.Items.IndexOf(item)]);

            }


        }

        public void AssertMatchingItemValues(Inspector inspector, List<object> values)
        {

            foreach (DynamoDropDownItem item in inspector.Items)
            {

                Assert.AreEqual(item.Item, values[inspector.Items.IndexOf(item)]);

            }


        }

        public void AssertCorrectSelectedIndices(Inspector inspector, List<int> indexlist)
        {

            foreach (Inspector.combobox_selected_index_wrapper indexwrapper in inspector.Indicies)
            {
                int index = indexwrapper.SelectedIndex;
                Assert.AreEqual(index, indexlist[inspector.Indicies.IndexOf(indexwrapper)]);

            }


        }




        [Test]
        public void InspectorItemNamesAndIndiciesFromDSColor()
        {

            // load a saved file dyn file that takes a color constructor node and an inspector node
            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), "core", "inspector", "InspectorTest1.dyn");
           
            Model.Open(openPath);
            Assert.DoesNotThrow(() => Controller.RunExpression());

            
            
            Inspector inspectornode = Controller.DynamoModel.CurrentWorkspace.Nodes[1] as Inspector;
            var ColorNode = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DSFunction>();
           
            Assert.NotNull(inspectornode);
            Assert.NotNull(ColorNode);
            
            Assert.AreEqual(inspectornode.Items.Count, 5);
            Assert.AreEqual(inspectornode.Indicies.Count, 3);
            
           AssertMatchingItemNames(inspectornode, new List<string>(){"InternalColor:Color [A=255, R=255, G=255, B=100]","Red:255","Green:255","Blue:100", "Alpha:255"});
          
           
           AssertCorrectSelectedIndices(inspectornode, new List<int>() { 1, 2, 3 });
           
            // next test will grab public properties from the mirror data of the input node and assert that the value of these properties
            // is == to the values in the items list of the inspector node

           List<object> testvalues = BuildTestValuesFromReflection(ColorNode);
           AssertMatchingItemValues(inspectornode, testvalues);
        }




       //stubs for future tests
        public void InspectorAddAndRemoveDropDowns ()
        {
                
          // does this need to be a recorded test?      

        }

        public void InspectorRemoveInputAndRun()
        {
            //// does this need to be a recorded test?   
        }

        public void InspectorChangeSelections()
        {
            //// does this need to be a recorded test?   
        }


    }
}