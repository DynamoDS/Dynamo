using System.Collections.ObjectModel;
using Dynamo.Utilities;
using NUnit.Framework;
using System.IO;
using Dynamo.Nodes;
using Dynamo.ViewModels;
using ProtoCore.Mirror;
using Dynamo.Models;
using System.Collections.Generic;
using DSCoreNodesUI;

namespace Dynamo.Tests
{
    [TestFixture]
    class InspectorTests : DSEvaluationUnitTest
    {




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
        public void InspectorItemNamesAndIndicies()
        {
            var model = Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\inspector\InspectorTest1.dyn");
            model.Open(openPath);

            Assert.DoesNotThrow(() => Controller.RunExpression(null));
            
            //dynSettings.Controller.PreferenceSettings.NumberFormat = "f0";

            var inspectornode = model.CurrentWorkspace.FirstNodeFromWorkspace<Inspector>();

            Assert.NotNull(inspectornode);
            
           AssertMatchingItemNames(inspectornode, new List<string>(){"InternalColor","Red","Green","Blue", "Alpha"});
           //AssertMatchingItemValues(inspectornode, new List<object>(){};
           AssertCorrectSelectedIndices(inspectornode, new List<int>() { 1, 2, 3 });
           

            
        }

    }
}