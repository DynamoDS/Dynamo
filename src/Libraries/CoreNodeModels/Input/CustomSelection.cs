using Autodesk.DesignScript.Runtime;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Xml;


namespace CoreNodeModels.Input
{
    /// <summary>
    /// This node allow the user to create a dropdown menu with an with an arbitrary number of customization items
    /// </summary>
    [NodeName("Custom Dropdown Menu")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("CustomSelectionNodeDescription", typeof(Properties.Resources))]
    [NodeSearchTags("CustomSelectionSearchTags", typeof(Properties.Resources))]
    [OutPortNames("value")]
    [OutPortTypes("object")]
    [OutPortDescriptions("Selected value")]
    [IsDesignScriptCompatible]
    public class CustomSelectionNodeModel : DSDropDownBase
        {
            private ObservableCollection<CustomSelectionItemViewModel> items = new ObservableCollection<CustomSelectionItemViewModel>();
            private CustomSelectionItemViewModel selectedItem;

            /// <summary>
            /// All menu items
            /// </summary>
            public ObservableCollection<CustomSelectionItemViewModel> EnumItems
            {
                get => items;
                set
                {
                    items = value;

                    foreach (var item in items)
                    {
                        InitItem(item);
                    }

                    PopulateItems();
                    RaisePropertyChanged(nameof(EnumItems));
                }
            }

            /// <summary>
            /// The menu items with valid names and values
            /// </summary>
            [JsonIgnore]
            public ObservableCollection<CustomSelectionItemViewModel> ValidEnumItems
                => new ObservableCollection<CustomSelectionItemViewModel>(EnumItems.Where(item => item.IsValid));


            /// <summary>
            /// the currently selected menu item
            /// </summary>
            public CustomSelectionItemViewModel SelectedItem
            {
                get => selectedItem;
                set
                {
                    selectedItem = value == null ? null : ValidEnumItems.FirstOrDefault(item => item.Name == value.Name);
                    RaisePropertyChanged(nameof(SelectedItem));

                    OnNodeModified();
                }
            }


            /// <summary>
            /// Command for adding a new menu item
            /// </summary>
            [IsVisibleInDynamoLibrary(false)]
            public ICommand AddCommand { get; private set; }



            /// <summary>
            /// Construct a new Custom Dropdown Menu node
            /// </summary>
            public CustomSelectionNodeModel() : base("Value")
            {
                RegisterAllPorts();

                ArgumentLacing = LacingStrategy.Disabled;

                EnumItems = new ObservableCollection<CustomSelectionItemViewModel>()
                {
                    new CustomSelectionItemViewModel(new CustomSelectionItem() {Name = "One", Value = "1"}),
                    new CustomSelectionItemViewModel(new CustomSelectionItem {Name = "Two", Value = "2"}),
                    new CustomSelectionItemViewModel(new CustomSelectionItem {Name = "Three", Value = "3"}),
                };

                SelectedItem = ValidEnumItems.FirstOrDefault();

                AddCommand = new AddMenuItemCommand(AddMenuItem);
            }


            [JsonConstructor]
            private CustomSelectionNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base("Value", inPorts, outPorts)
            {
                AddCommand = new AddMenuItemCommand(AddMenuItem);
                items.CollectionChanged += OnCollectionChanged;
            }


            private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                RaisePropertyChanged(nameof(ValidEnumItems));
                OnNodeModified();
            }


            /// <summary>
            /// Build the AST for this node
            /// </summary>
            /// <param name="inputAstNodes"></param>
            /// <returns></returns>
            [IsVisibleInDynamoLibrary(false)]
            public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
            {
                object value = GetSelectedValue();
                AssociativeNode associativeNode = AstFactory.BuildPrimitiveNodeFromObject(value);

                return new List<AssociativeNode> { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), associativeNode) };
            }

            /// <summary>
            /// Log an exception
            /// </summary>
            /// <param name="ex"></param>
            [IsVisibleInDynamoLibrary(false)]
            public void Log(Exception ex)
            {
                base.Log(ex.Message, Dynamo.Logging.WarningLevel.Error);
                base.Log(ex.StackTrace, Dynamo.Logging.WarningLevel.Error);
            }

            /// <summary>
            /// Dispose this node
            /// </summary>
            public override void Dispose()
            {
                base.Dispose();
                items.CollectionChanged -= OnCollectionChanged;
            }

            private void AddMenuItem(object param)
            {
                var newItem = new CustomSelectionItemViewModel(new CustomSelectionItem());
                InitItem(newItem);

                EnumItems.Add(newItem);
            }


            private bool IsUnique(CustomSelectionItem item)
            {
                IEnumerable<CustomSelectionItemViewModel> items = EnumItems.Where(x => x.Name == item.Name);
                return items.Count() <= 1;
            }


            private void OnItemChanged()
            {
                RaisePropertyChanged(nameof(ValidEnumItems));

                foreach (var item in EnumItems)
                    item.Validate();

                OnNodeModified();
            }


            private void OnRemoveRequested(CustomSelectionItemViewModel item)
            {
                item.IsUnique -= IsUnique;
                item.ItemChanged -= OnItemChanged;
                item.RemoveRequested -= OnRemoveRequested;
                EnumItems.Remove(item);
                RaisePropertyChanged(nameof(ValidEnumItems));
                RaisePropertyChanged(nameof(SelectedItem));
            }


            /// <summary>
            /// Return the selected item as an int, or a double, or a string
            /// </summary>
            /// <returns></returns>
            private object GetSelectedValue()
            {
                if (SelectedItem == null)
                    return null;

                int intValue = 0;
                if (EnumItems.All(item => int.TryParse(item.Value, out intValue)))
                {
                    int.TryParse(SelectedItem.Value, out intValue);
                    return intValue;
                }

                double doubleValue = 0.0;
                if (EnumItems.All(item => double.TryParse(item.Value, out doubleValue)))
                {
                    double.TryParse(SelectedItem.Value, out doubleValue);
                    return doubleValue;
                }

                return SelectedItem.Value;
            }

            private void InitItem(CustomSelectionItemViewModel item)
            {
                item.IsUnique += IsUnique;
                item.ItemChanged += OnItemChanged;
                item.RemoveRequested += OnRemoveRequested;
            }

            protected override void SerializeCore(XmlElement element, SaveContext context)
            {
                base.SerializeCore(element, context);

                var xmlDocument = element.OwnerDocument;
                var enumItemsNode = xmlDocument.CreateElement("CustomeSelectionItem");

                foreach (CustomSelectionItemViewModel item in EnumItems)
                {
                    if (item.IsValid)
                    {
                        var itemNode = xmlDocument.CreateElement("CustomeSelectionItem");
                        itemNode.SetAttribute("Name", item.Name);
                        itemNode.SetAttribute("Value", item.Value?.ToString());

                        enumItemsNode.AppendChild(itemNode);
                    }
                }

                element.AppendChild(enumItemsNode);

                if (SelectedItem != null)
                {
                    var selectedItemNode = xmlDocument.CreateElement("CustomeSelectionSelectedItem");
                    selectedItemNode.SetAttribute("Name", SelectedItem.Name);

                    element.AppendChild(selectedItemNode);
                }
            }

            protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
            {
                base.DeserializeCore(nodeElement, context);

                foreach (XmlNode subNode in nodeElement.ChildNodes)
                {
                    if (subNode.Name.Equals("CustomeSelectionItem"))
                    {
                        DeserializeEnumItems(subNode);
                    }

                    if (subNode.Name.Equals("CustomeSelectionSelectedItem"))
                    {
                        foreach (XmlAttribute attribute in subNode.Attributes)
                        {
                            if (attribute.Name == "Name")
                            {
                                SelectedItem = EnumItems.FirstOrDefault(item => item.Name == attribute.Value);
                            }
                        }
                    }
                }
            }

            protected override SelectionState PopulateItemsCore(string currentSelection)
            {
                Items.Clear();

                foreach (CustomSelectionItemViewModel enumItem in EnumItems)
                {
                    Items.Add(new DynamoDropDownItem(enumItem.Name, enumItem.Value));
                }

                SelectedIndex = 0;

                return SelectionState.Restore;
            }

            private void DeserializeEnumItems(XmlNode enumItemsNode)
            {
                EnumItems.Clear();

                foreach (XmlNode childNode in enumItemsNode.ChildNodes)
                {
                    var name = string.Empty;
                    var value = string.Empty;

                    foreach (XmlAttribute attribute in childNode.Attributes)
                    {
                        if (attribute.Name == "Name")
                        {
                            name = attribute.Value;
                        }
                        else if (attribute.Name == "Value")
                        {
                            value = attribute.Value;
                        }
                    }

                    var newItem = new CustomSelectionItemViewModel(new CustomSelectionItem() { Name = name, Value = value });
                    InitItem(newItem);

                    EnumItems.Add(newItem);
                }
            }


            class AddMenuItemCommand : ICommand
            {
                private readonly Action<object> execute;
                public event EventHandler CanExecuteChanged;

                public AddMenuItemCommand(Action<object> execute)
                {
                    this.execute = execute;
                }

                public bool CanExecute(object parameter) => true;
                public void Execute(object parameter) => execute(parameter);
            }
        }
}