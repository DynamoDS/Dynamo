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

using rt =  Microsoft.CSharp.RuntimeBinder;
using System;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace DSCoreNodesUI
{
    [IsDesignScriptCompatible]
    [NodeName("Inspector")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Displays Members of Any Object - Useful for debugging.")]
    public class Inspector : VariableInputNode, IWpfNode
    {
        /// <summary>
        /// wraps a combox box selexted index
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

        /// <summary>
        /// value that keeps track of desired number of dropdowns
        /// </summary>
        private int numDropDowns = 1;
        /// <summary>
        /// collections of items - populated from public properties in PopulateItems()
        /// </summary>
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




        /// <summary>
        /// list of comboboxes - can use this to track existing number of comboboxes
        /// </summary>
        public List<ComboBox> comboboxes = new List<ComboBox>();
        public List<combobox_selected_index_wrapper> previous_indicies = new List<combobox_selected_index_wrapper>();
       // public List<TextBox> tboxes = new List<TextBox>();


        public event EventHandler RequestSelectChange;
        protected virtual void OnRequestSelectChange(object sender, EventArgs e)
        {
            if (RequestSelectChange != null)
                RequestSelectChange(sender, e);
        }

        public Inspector()
        {
            InPortData.Add(new PortData("object", "Object to Inspect"));
            OutPortData.Add(new PortData("object", "The Object"));

            RegisterAllPorts();

            // subscribe this method to send an update event whenever any property on this object changes...
            // but that handler checks if IsUpdated is fired
            this.PropertyChanged += Inspector_PropertyChanged;


        }

        void Inspector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsUpdated")
                return;

            if (InPorts.Any(x => x.Connectors.Count == 0))
                return;

            OnRequestSelectChange(this, EventArgs.Empty);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {

            var outputobject = inputAstNodes[0];
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), outputobject)
            };
        }

        
        
        public static object GetDynamicValue(dynamic ob, string name)
        {
            CallSite<Func<CallSite, object, object>> site
                = CallSite<Func<CallSite, object, object>>.Create
                (rt.Binder.GetMember(rt.CSharpBinderFlags.None, name,
                 typeof(IDynamicMetaObjectProvider), new rt.CSharpArgumentInfo[] { rt.CSharpArgumentInfo.Create(rt.CSharpArgumentInfoFlags.None, null) }));
            return site.Target(site, ob);
        }


        public void PopulateItems()
        {
            // getting the input object song and dance
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

                    // check the type of this object, if it's dynamic have to parse membernames and then get values
                    if (objectinput is IDynamicMetaObjectProvider)
                    {
                        var names = new List<string>();
                        var dynobj = objectinput as IDynamicMetaObjectProvider;
                        if (dynobj != null)
                        {
                            names.AddRange(dynobj.GetMetaObject(System.Linq.Expressions.Expression.Constant(dynobj)).GetDynamicMemberNames());
                        }


                        foreach (var name in names)
                        {
                            var val = GetDynamicValue(objectinput, name);

                            if (val != null)
                            {
                                items.Add(new DynamoDropDownItem(string.Format("{0}:{1}", name, val), val));
                            }


                            /* object val = null;
                             MemberInfo mem = objectinput.GetType().GetMember(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).FirstOrDefault();

                             if (mem.MemberType == MemberTypes.Field)
                             {
                                 val = ((FieldInfo)mem).GetValue(objectinput);
                             }
                             else if (mem.MemberType == MemberTypes.Property)
                             {

                                 val = ((PropertyInfo)mem).GetValue(objectinput, null);
                             }
                             if (val != null){
                             items.Add(new DynamoDropDownItem(string.Format("{0}:{1}", name, val), val));
                             }
                         }
                             */

                        }

                    }

                    // if object was not dynamic    
                    else
                    {
                        var propertyInfos = objectinput.GetType().GetProperties(
                   BindingFlags.Public | BindingFlags.NonPublic // Get public and non-public
                 | BindingFlags.Static | BindingFlags.Instance  // Get instance + static
                 | BindingFlags.FlattenHierarchy); // Search up the hierarchy



                        foreach (var prop in propertyInfos.ToList())
                        {
                            var val = prop.GetValue(objectinput, null);
                            items.Add(new DynamoDropDownItem(string.Format("{0}:{1}", prop.Name, val), val));
                        }

                    }

                    //Items = Items.OrderBy(x => x.Name).ToObservableCollection();

                }

            }
        }
        protected override void AddInput()
        {
            // instead of actually adding an input
            // keep track of # of dropdowns
            // create a new indexwrapper and store it
            // then call setupcustomUIelements on the UIdispatch thread.
            numDropDowns = numDropDowns + 1;
            // combobox_selected_index_wrapper new_selected_index = new combobox_selected_index_wrapper();
            //indicies.Add(new_selected_index);
            RequiresRecalc = true;
            OnRequestSelectChange(this, EventArgs.Empty);
        }

        protected override void RemoveInput()
        {
            if (numDropDowns > 1)
            {
                numDropDowns = numDropDowns - 1;
                // not sure about this actually deleting the wrapper
                // var lastindex = indicies[indicies.Count - 1];
                //indicies.RemoveAt(indicies.Count - 1);

                RequiresRecalc = true;
                OnRequestSelectChange(this, EventArgs.Empty);
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

        /// <summary>
        /// When the dropdown is opened, the node's implementation of PopulateItems is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void combo_DropDownOpened(object sender, EventArgs e)
        {
            //PopulateItems();
        }


        public ComboBox gen_and_setup_combobox(WrapPanel wp)
        {
            // make a new index wrapper to bind to
            combobox_selected_index_wrapper new_selected_index = new combobox_selected_index_wrapper();
            //store it
            indicies.Add(new_selected_index);
            //we just added this so this is the index
            int index_into_index = indicies.Count - 1;

/*

            var tbox = new TextBox
            {
                Width = System.Double.NaN,
                MinWidth = 100,
                Height = Configurations.PortHeightInPixels,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center

            };
            wp.Children.Add(tbox);
           

            //text box for testing the index binding
            var tindexBinding = new Binding()
            {
                Path = new PropertyPath("SelectedIndex"),
                Mode = BindingMode.OneWay,
                Source = new_selected_index
            };
            tbox.SetBinding(TextBox.TextProperty, tindexBinding);

            */




            var combo = new ComboBox
                        {
                            Width = System.Double.NaN,
                            MinWidth = 100,
                            Height = Configurations.PortHeightInPixels,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Center
                        };
            wp.Children.Add(combo);
            comboboxes.Add(combo);
          //  tboxes.Add(tbox);
            //System.Windows.Controls.Grid.SetColumn(combo, 0);
            //System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += combo_DropDownOpened;
            combo.SelectionChanged += delegate
            {   // if we are changing the selection
                // this might occur because we've changed the selection
                // or because items has been refreshed
                // if items is refreshed - index will goto -1
                // and in that case we'll hold onto the current index
                if (combo.SelectedIndex == -1)
                {
                    //RequiresRecalc = true;
                    int index_of_this_box = comboboxes.IndexOf(combo);
                    //indicies[index_of_this_box] = previous_indicies[index_of_this_box];
                    
                    // if this combo box has been removed from the combobox list, then 
                    // the selected index should refer to the last remaining item in the list.
                    // actually this may be unture... might need to just delete the combobox, could do it here or where it is removed.
                    if (index_of_this_box > -1)
                    {   
                    
                    combo.SelectedIndex = previous_indicies[index_of_this_box].SelectedIndex;
                    }
                }
            };

            combo.DropDownClosed += delegate
            {
                //disallow selection of nothing
                if (combo.SelectedIndex == -1)
                {


                }
            };

            combo.DataContext = this;
            //bind this combo box to the selected item hash

            var bindingVal = new System.Windows.Data.Binding("Items") { Mode = BindingMode.TwoWay, Source = this };
            combo.SetBinding(ItemsControl.ItemsSourceProperty, bindingVal);

            //bind the selected index to the 
            var indexBinding = new Binding()
            {
                Path = new PropertyPath("SelectedIndex"),
                Mode = BindingMode.TwoWay,
                Source = new_selected_index
            };
            combo.SetBinding(Selector.SelectedIndexProperty, indexBinding);

            return combo;
        }

        public ComboBox remove_combo(WrapPanel wp)
        {
            // find the last combobox
            var last = comboboxes[comboboxes.Count - 1];
           // var lasttbox = tboxes[tboxes.Count-1];
            //remove it from the wrap panel
            wp.Children.Remove(last);
            comboboxes.Remove(last);
          //  wp.Children.Remove(lasttbox);
           // tboxes.Remove(lasttbox);
            // remove it's index from the list of indicies
            indicies.RemoveAt(Indicies.Count - 1);
            return last;

        }





        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {






            //add a drop down list to the window
            var addButton = new DynamoNodeButton(this, "AddInPort") { Content = "+", Width = 20 };
            //addButton.Height = 20;

            var subButton = new DynamoNodeButton(this, "RemoveInPort") { Content = "-", Width = 20 };
            //subButton.Height = 20;

            // first add all buttons
            //

            var wp = new WrapPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxWidth = 500,
            };
            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            nodeUI.inputGrid.Children.Add(wp);

            // add the first combobox
            var firstcombo = gen_and_setup_combobox(wp);


            RequestSelectChange += delegate
            {
                DispatchOnUIThread(delegate
                {
                    // first populate all items list

                    PopulateItems();
                    if (comboboxes.Count < numDropDowns)
                    {
                        var cur_combo_box = gen_and_setup_combobox(wp);
                    }
                    else if (comboboxes.Count > numDropDowns)
                    {
                        ComboBox removed_combo = remove_combo(wp);


                    }


                });
            };
        }
        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }


    }
}
