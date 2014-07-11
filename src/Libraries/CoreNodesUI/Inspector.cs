using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using Dynamo.Nodes;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Dynamic;
using System.Xml;
using rt = Microsoft.CSharp.RuntimeBinder;
using System.Runtime.CompilerServices;

namespace DSCoreNodesUI
{
    [IsDesignScriptCompatible]
    [NodeName("Inspector")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Displays Public Properties of Any Object - Useful for Debugging.")]
    public class Inspector : VariableInputNode, IWpfNode
    {
        /// <summary>
        /// wraps a comobox's selexted index so that we can data bind to it for a specific combobox
        /// </summary>
        public class combobox_selected_index_wrapper : INotifyPropertyChanged
        {

            public combobox_selected_index_wrapper()
            {

            }

            private int selectedIndex = 0;
            public int SelectedIndex
            {
                get { return selectedIndex; }
                set
                {

                    selectedIndex = value;
                    NotifyPropertyChanged("SelectedIndex");
                }
            }
            private void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;


        }
        // this list of items that storeproperties actually populates
        // PopulateItems() adds these items to the Items list, this is done
        // so that we can collect the properties outside of the UiThread.
        // Observalble collection Items complains about being modified since it is databound to comboboxes
        public List<DynamoDropDownItem> membersandvals = new List<DynamoDropDownItem>();

        /// <summary>
        /// value that keeps track of desired number of dropdowns
        /// </summary>
        private int numDropDowns = 1;
        /// <summary>
        /// collections of items - populated from public properties in PopulateItems()
        /// </summary>
        /// 
        private ObservableCollection<DynamoDropDownItem> items = new ObservableCollection<DynamoDropDownItem>();
        public ObservableCollection<DynamoDropDownItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged("Items");
            }
        }
        /// <summary>
        /// collection of index wrappers
        /// </summary>
        private ObservableCollection<combobox_selected_index_wrapper> indicies = new ObservableCollection<combobox_selected_index_wrapper>();
        public ObservableCollection<combobox_selected_index_wrapper> Indicies
        {
            get { return indicies; }
            set
            {
                indicies = value;
                RaisePropertyChanged("Indicies");
            }
        }

        private StackPanel InnerStackPanel;


        /// <summary>
        /// list of comboboxes - can use this to track existing number of comboboxes
        /// </summary>
        private List<ComboBox> comboboxes = new List<ComboBox>();
        /// <summary>
        /// store the previous indicies of all comboboxes so when populate items() runs and databind changes all indicies to -1, we can set them back correctly
        /// </summary>
        private List<combobox_selected_index_wrapper> previous_indicies = new List<combobox_selected_index_wrapper>();
        /// <summary>
        /// list that is only populated with deserialized indicies on load so that indicies can be set correctly by SetupUI method
        /// </summary>
        private List<int> loaded_indices = new List<int>();

        public event EventHandler UiNeedsUpdate;
        protected virtual void OnUiNeedsUpdate(object sender, EventArgs e)
        {
            if (UiNeedsUpdate != null)
                UiNeedsUpdate(sender, e);
        }




        public Inspector()
        {
            InPortData.Add(new PortData("object", "Object to Inspect"));
            OutPortData.Add(new PortData("object", "The Object"));

            RegisterAllPorts();

            // subscribe this method to send an update event whenever any property on this object changes...
            // but that handler checks if IsUpdated is fired
            this.PropertyChanged += Inspector_PropertyChanged;
            dynSettings.Controller.EvaluationCompleted += Controller_EvaluationCompleted;

        }

        // only grab properties via reflection when controller fires eval completed, then dispatch ui changes
        void Controller_EvaluationCompleted(object sender, EventArgs e)
        {
            StoreProperties();
            OnUiNeedsUpdate(this, EventArgs.Empty);

        }

        void Inspector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsUpdated")
                return;

            if (InPorts.Any(x => x.Connectors.Count == 0))
                return;


            OnUiNeedsUpdate(this, EventArgs.Empty);
        }
        /// <summary>
        /// pass through of the object to the output
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {

            var outputobject = inputAstNodes[0];

            return new[]
            {   
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), outputobject)
            };



        }


        //method for grabbing member values from a dynamic object, we use this for python objects...
        //  http://stackoverflow.com/questions/1926776/getting-a-value-from-a-dynamic-object-dynamically
        private static object GetDynamicValue(dynamic ob, string name)
        {
            CallSite<Func<CallSite, object, object>> site
                = CallSite<Func<CallSite, object, object>>.Create
                (rt.Binder.GetMember(rt.CSharpBinderFlags.None, name,
                 typeof(IDynamicMetaObjectProvider), new rt.CSharpArgumentInfo[] { rt.CSharpArgumentInfo.Create(rt.CSharpArgumentInfoFlags.None, null) }));
            return site.Target(site, ob);
        }



        /// <summary>
        /// this method is run at evaluation time and collects the properties of the input object
        /// hopefully running this method on eval will automatically wrap revit objects in neccesary transactions
        /// to collect things like point locations
        /// </summary>
        private void StoreProperties()
        {

            if (InPorts.All(x => x.Connectors.Count != 0))
            {


                var inputnode = InPorts[0].Connectors[0].Start.Owner;
                var inputIndex = InPorts[0].Connectors[0].Start.Index;

                var inputId = inputnode.GetAstIdentifierForOutputIndex(inputIndex).Name;

                var mirror = dynSettings.Controller.EngineController.GetMirror(inputId);

                if (mirror != null)
                {
                    object objectinput = null;


                    if (mirror.GetData().IsCollection)
                    {
                        objectinput = mirror.GetData().GetElements().Select(x => x.Data).FirstOrDefault();
                    }
                    else
                    {
                        objectinput = mirror.GetData().Data;
                    }

                    objectinput = objectinput as object;

                    membersandvals.Clear();
                    // the input object exists, lets determine it's type and reflect over it.

                    if (objectinput is IDynamicMetaObjectProvider)
                    {
                        var names = new List<string>();
                        var dynobj = objectinput as IDynamicMetaObjectProvider;
                        if (dynobj != null)
                        {
                            names.AddRange(dynobj.GetMetaObject(System.Linq.Expressions.Expression.Constant(dynobj)).GetDynamicMemberNames());
                        }

                        //filter names so that python private and builtin members do not show
                        var filterednames = names.Where(x => x.StartsWith("__") != true).ToList();

                        foreach (var name in filterednames)
                        {

                            var val = GetDynamicValue(objectinput, name);

                            if (val != null)
                            {
                                membersandvals.Add(new DynamoDropDownItem(string.Format("{0}:{1}", name, val), val));
                            }

                        }


                    }



                  // if object was not dynamic use regular reflection
                    else
                    {
                        var propertyInfos = objectinput.GetType().GetProperties(
                   BindingFlags.Public | BindingFlags.NonPublic // Get public and non-public
                 | BindingFlags.Static | BindingFlags.Instance  // Get instance + static
                 | BindingFlags.FlattenHierarchy); // Search up the hierarchy



                        foreach (var prop in propertyInfos.ToList())
                        {
                            var val = prop.GetValue(objectinput, null);
                            membersandvals.Add(new DynamoDropDownItem(string.Format("{0}:{1}", prop.Name, val), val));
                        }

                    }



                }

            }

        }


        private void PopulateItems()
        {


            //before clearing the items and forcing updates
            // of the selections, lets grab all current selected indicies
            previous_indicies.Clear();
            foreach (combobox_selected_index_wrapper wrapper in Indicies)
            {
                var x = new combobox_selected_index_wrapper();
                x.SelectedIndex = wrapper.SelectedIndex;
                previous_indicies.Add(x);
            }

            Items.Clear();

            // now we just set items to equal membervals...
            foreach (DynamoDropDownItem dropdown in membersandvals)
            {

                Items.Add(new DynamoDropDownItem(dropdown.Name, dropdown.Item));

            }




        }



        protected override void AddInput()
        {
            // instead of actually adding an input
            // keep track of # of dropdowns
            // then call setupcustomUIelements on the UIdispatch thread by firing event.
            numDropDowns = numDropDowns + 1;

            RequiresRecalc = true;
            OnUiNeedsUpdate(this, EventArgs.Empty);
        }

        protected override void RemoveInput()
        {
            if (numDropDowns > 1)
            {
                numDropDowns = numDropDowns - 1;

                RequiresRecalc = true;
                OnUiNeedsUpdate(this, EventArgs.Empty);
            }
        }

        protected override string GetInputName(int index)
        {
            return "object" + index;
        }

        protected override string GetInputTooltip(int index)
        {
            return "object" + index;
        }

        protected override int GetInputIndex()
        {
            return InPortData.Count;
        }


        void combo_DropDownOpened(object sender, EventArgs e)
        {
            //PopulateItems();
        }


        public ComboBox gen_and_setup_combobox(StackPanel sp)
        {
            // make a new index wrapper to bind to
            combobox_selected_index_wrapper new_selected_index = new combobox_selected_index_wrapper();
            //store it
            indicies.Add(new_selected_index);
            //we just added this so this is the index
            int index_into_index = indicies.Count - 1;

            var combo = new ComboBox
                        {
                            Width = System.Double.NaN,
                            MinWidth = 100,
                            Height = Configurations.PortHeightInPixels,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Center
                        };
            sp.Children.Add(combo);
            comboboxes.Add(combo);


            combo.DropDownOpened += combo_DropDownOpened;
            combo.SelectionChanged += delegate
            {   // if we are changing the selection
                // this might occur because we've changed the selection
                // or because items has been refreshed
                // if items is refreshed - index will goto -1
                // and in that case we'll hold onto the current index
                if (combo.SelectedIndex == -1)
                {

                    int index_of_this_box = comboboxes.IndexOf(combo);


                    // if we found the combobox in the list of comboboxes, it has not been removed
                    // and we need to set its index to what it was previously before data bind changed it to -1
                    if (index_of_this_box > -1)
                    {

                        combo.SelectedIndex = previous_indicies[index_of_this_box].SelectedIndex;
                    }
                }
            };

            combo.DropDownClosed += delegate
            {

            };

            combo.DataContext = this;
            //bind this combo box to the selected item hash

            var bindingVal = new System.Windows.Data.Binding("Items") { Mode = BindingMode.TwoWay, Source = this };
            combo.SetBinding(ItemsControl.ItemsSourceProperty, bindingVal);

            //bind the selected index to the selected index property of the index wrapper we just created above
            var indexBinding = new Binding()
            {
                Path = new PropertyPath("SelectedIndex"),
                Mode = BindingMode.TwoWay,
                Source = new_selected_index
            };
            combo.SetBinding(Selector.SelectedIndexProperty, indexBinding);

            return combo;
        }

        public ComboBox remove_combo(StackPanel sp)
        {
            // find the last combobox
            var last = comboboxes[comboboxes.Count - 1];

            //remove it from the stack panel
            sp.Children.Remove(last);
            comboboxes.Remove(last);

            // remove its index from the list of indicies
            indicies.RemoveAt(Indicies.Count - 1);
            return last;

        }

        public void SetComboBoxes()
        {
            // first populate all items list
            // then add necessary combo boxes or remove

            PopulateItems();
            if (comboboxes.Count < numDropDowns)
            {
                var cur_combo_box = gen_and_setup_combobox(InnerStackPanel);
            }
            else if (comboboxes.Count > numDropDowns)
            {
                ComboBox removed_combo = remove_combo(InnerStackPanel);


            }


        }



        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {


            //add a drop down list to the window
            var addButton = new DynamoNodeButton(this, "AddInPort") { Content = "+", Width = 20 };


            var subButton = new DynamoNodeButton(this, "RemoveInPort") { Content = "-", Width = 20 };


            // first add all buttons
            //
            addButton.MaxHeight = Configurations.PortHeightInPixels;
            subButton.MaxHeight = Configurations.PortHeightInPixels;

            addButton.VerticalAlignment = VerticalAlignment.Top;
            subButton.VerticalAlignment = VerticalAlignment.Top;


            // outer panel contains buttons
            var OuterWrapPanel = new WrapPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxWidth = 500,
            };
            OuterWrapPanel.Children.Add(addButton);
            OuterWrapPanel.Children.Add(subButton);

            // inner panel contains dropboxes
            var InnerStackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                MinHeight = Configurations.PortHeightInPixels,


            };

            this.InnerStackPanel = InnerStackPanel;

            OuterWrapPanel.Children.Add(InnerStackPanel);

            nodeUI.inputGrid.Children.Add(OuterWrapPanel);


            // if the loaded indicies exist then create the correct number of boxes and set their indicies
            if (loaded_indices.Count > 0)
            {
                for (int i = 0; i < loaded_indices.Count; i++)
                {
                    SetComboBoxes();
                    Indicies[i].SelectedIndex = loaded_indices[i];
                }
                // clear this list after loading the first time
                loaded_indices.Clear();
            }


            UiNeedsUpdate += delegate
            {

                DispatchOnUIThread(SetComboBoxes);
            };
        }

        // drop all indicies into a csv list
        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            var stringlist = Indicies.Select(x => x.SelectedIndex.ToString()).ToList();
            string joined = string.Join(",", stringlist.ToArray());
            nodeElement.SetAttribute("indices", joined);

            nodeElement.SetAttribute("numdropdowns", numDropDowns.ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            try
            {
                string[] strings = nodeElement.Attributes["indices"].Value.Split(',');
                List<int> ints = strings.Select(x => Convert.ToInt32(x)).ToList();

                numDropDowns = Convert.ToInt32(nodeElement.Attributes["numdropdowns"].Value);

                foreach (int index in ints)
                {

                    // record the loaded indicies and let setupui handle this when it loads the UI
                    loaded_indices.Add(index);
                }


            }
            catch { }



        }

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }


    }
}
