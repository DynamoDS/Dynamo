using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Nodes;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Controls.Primitives;


namespace DSCoreNodesUI
{   
    [IsDesignScriptCompatible]
    [NodeName("Inspector")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Get a color given a color range.")]
    public class Inspector : VariableInputNode, IWpfNode
    {
        private int numDropDowns = 1;
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

        private List<int> selectedIndicies = new List<int>() {-1};
        public List<int> SelectedIndicies
        {
            get { return selectedIndicies; }
            set
            {
                
                
                        dynSettings.DynamoLogger.Log(value.ToString());
                        SelectedIndicies = value;
                        RaisePropertyChanged("SelectedIndicies");
                    
                
               
            }
        }

        public List<ComboBox> comboboxes = new List<ComboBox>();




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



        public void PopulateItems()
        {
            // getting the input object song and dance
            // crash might be occuring here because input is not ready
            if (Inputs.Count > 0)
            {

                var inputnode = InPorts[0].Connectors[0].Start.Owner;
                var inputIndex = InPorts[0].Connectors[0].Start.Index;

                var inputId = inputnode.GetAstIdentifierForOutputIndex(inputIndex).Name;

                var mirror = dynSettings.Controller.EngineController.GetMirror(inputId);

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


                Items.Clear();

                var propertyInfos = objectinput.GetType().GetProperties(
           BindingFlags.Public | BindingFlags.NonPublic // Get public and non-public
         | BindingFlags.Static | BindingFlags.Instance  // Get instance + static
         | BindingFlags.FlattenHierarchy); // Search up the hierarchy


                foreach (var prop in propertyInfos.ToList())
                {
                    var val = prop.GetValue(objectinput, null);
                    items.Add(new DynamoDropDownItem(string.Format("{0}:{1}", prop.Name, val), val));
                }

                Items = Items.OrderBy(x => x.Name).ToObservableCollection();

            }

        }

        protected override void AddInput()
        {
            // instead of actually adding an input
            // keep track of # of dropdowns
            // then call setupcustomUIelements on the UIdispatch thread.
            numDropDowns = numDropDowns + 1;
            SelectedIndicies.Add(0);
            RequiresRecalc = true;
            OnRequestSelectChange(this, EventArgs.Empty);
        }

        protected override void RemoveInput()
        {
            if (numDropDowns > 1)
            {
                numDropDowns = numDropDowns - 1;
                SelectedIndicies.RemoveAt(SelectedIndicies.Count - 1);
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
          
            RequestSelectChange += delegate
            {
                DispatchOnUIThread(delegate
                {
                    // first populate all items list

                    // delete existing combo boxes
                    foreach (ComboBox combo in comboboxes)
                    {

                        wp.Children.Remove(combo);

                    }
                    comboboxes.Clear();
                    PopulateItems();
                    //clear the list of stored boxes

                    // then build new combo boxes for each desired
                    // this number is set by the user
                    for (int i = 0; i < numDropDowns; i++)
                    {
                        
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
                        //System.Windows.Controls.Grid.SetColumn(combo, 0);
                        //System.Windows.Controls.Grid.SetRow(combo, 0);
                        
                        combo.DropDownOpened += combo_DropDownOpened;
                      /*  combo.SelectionChanged += delegate
                        {
                            if (combo.SelectedIndex != -1)
                            {
                                RequiresRecalc = true;
                                int index = comboboxes.IndexOf(combo);
                                SelectedIndicies[index] = combo.SelectedIndex;
                            }
                        };
                       */
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
                        var indexBinding = new Binding("SelectedIndex")
                        {
                            Mode = BindingMode.TwoWay,
                            Source = this
                        };
                        combo.SetBinding(Selector.SelectedIndexProperty, indexBinding);
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
