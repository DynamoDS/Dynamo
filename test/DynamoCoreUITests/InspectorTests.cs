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
using System;
using DynamoCoreUITests;

using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.UI.Controls;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.UpdateManager;
using DynamoCore.UI.Controls;

using DynamoUtilities;


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


            string openPath = Path.Combine(GetTestDirectory(), "core", "inspector", "InspectorTest1.dyn");
            Console.WriteLine(openPath);



            // open a new UI so that SetupUI() runs
           
            DynamoController.IsTestMode = true;
            Controller.DynamoViewModel = new DynamoViewModel(Controller, null);
            Controller.VisualizationManager = new VisualizationManager();

            //create the view
            var Ui = new DynamoView { DataContext = Controller.DynamoViewModel };
            var Vm = Controller.DynamoViewModel;
            Controller.UIDispatcher = Ui.Dispatcher;
            Ui.Show();                             


            Assert.DoesNotThrow(() => RunModel(openPath));
            
            //dynSettings.Controller.PreferenceSettings.NumberFormat = "f0";

            Inspector inspectornode = Controller.DynamoModel.CurrentWorkspace.Nodes[1] as Inspector;
            Console.WriteLine(inspectornode.GUID);
            Assert.NotNull(inspectornode);
            
            //for some reason RunModel is not setting the indicies or items on this inspector instance... investigate... 
            Assert.AreEqual(inspectornode.Items.Count, 5);
            Assert.AreEqual(inspectornode.Indicies.Count, 3);

           AssertMatchingItemNames(inspectornode, new List<string>(){"boogers","InternalColor","Red","Green","Blue", "Alpha"});
           //AssertMatchingItemValues(inspectornode, new List<object>(){};
           AssertCorrectSelectedIndices(inspectornode, new List<int>() { 100, 300, 3 });
           

            
        }

    }
}