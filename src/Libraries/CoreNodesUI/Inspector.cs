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
        private int numDropDowns = 2;
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

        private List<int> selectedIndicies = new List<int>(0);
        public List<int> SelectedIndicies
        {
            get { return selectedIndicies; }
            set
            {
                //do not allow selected index to
                //go out of range of the items collection
                foreach (var index in selectedIndicies)
                {
                    if (index < Items.Count - 1)
                    {
                        SelectedIndicies = value;
                        RaisePropertyChanged("SelectedIndex");
                    }
                }
               
            }
        }






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
            
            // this is causing trouble,
            // since if we clear the items when opening one dropdpown, all others are reset as well....
            // items may need to be a list of lists
            Items.Clear();
            
            var propertyInfos = objectinput.GetType().GetProperties(
       BindingFlags.Public | BindingFlags.NonPublic // Get public and non-public
     | BindingFlags.Static | BindingFlags.Instance  // Get instance + static
     | BindingFlags.FlattenHierarchy); // Search up the hierarchy


            foreach (var prop in propertyInfos.ToList())
            {
                var val = prop.GetValue(objectinput, null);
                items.Add(new DynamoDropDownItem(string.Format("{0}:{1}",prop.Name,val),val));
            }

           Items = Items.OrderBy(x => x.Name).ToObservableCollection();

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

            var wp = new WrapPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            nodeUI.inputGrid.Children.Add(wp);


            RequestSelectChange += delegate
            {
                DispatchOnUIThread(delegate
                {
                    PopulateItems();

                    foreach (UIElement item in wp.Children)
                    {
                        if (item is ComboBox)
                        {
                            wp.Children.Remove(item);
                        }
                    }
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
                        System.Windows.Controls.Grid.SetColumn(combo, 0);
                        System.Windows.Controls.Grid.SetRow(combo, 0);

                        combo.DropDownOpened += combo_DropDownOpened;
                        combo.SelectionChanged += delegate
                        {
                            if (combo.SelectedIndex != -1)
                                RequiresRecalc = true;
                        };

                        combo.DropDownClosed += delegate
                        {
                            //disallow selection of nothing
                            if (combo.SelectedIndex == -1)
                            {
                                SelectedIndicies[i] = 0;
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
