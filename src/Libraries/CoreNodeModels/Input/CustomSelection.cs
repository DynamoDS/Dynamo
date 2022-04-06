using Autodesk.DesignScript.Runtime;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Xml;

[assembly: InternalsVisibleTo("CoreNodeModelsWpf")]

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
    [OutPortTypes("var")]
    [OutPortDescriptions("Selected value")]
    [IsDesignScriptCompatible]
    public class CustomSelectionNodeModel : DSDropDownBase
    {
        private ObservableCollection<CustomSelectionItemViewModel> items = new ObservableCollection<CustomSelectionItemViewModel>();
        private CustomSelectionItemViewModel selectedItem;

        /// <summary>
        /// This command is bound to the Add button in the GUI
        /// </summary>
        [JsonIgnore]
        public ICommand AddCommand { get; private set; }

        /// <summary>
        /// All menu items
        /// </summary>
        public ObservableCollection<CustomSelectionItemViewModel> Items
        {
            get => items;
            private set
            {
                items = value;

                foreach (var item in items)
                {
                    InitItem(item);
                }

                PopulateItems();
                RaisePropertyChanged(nameof(Items));
            }
        }

        /// <summary>
        /// The menu items with valid names and values. This property is bound to the combo box in the GUI
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<CustomSelectionItemViewModel> ValidItems
            => new ObservableCollection<CustomSelectionItemViewModel>(Items.Where(item => item.IsValid));


        /// <summary>
        /// the currently selected menu item
        /// </summary>
        public CustomSelectionItemViewModel SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value == null ? null : Items.FirstOrDefault(item => item.Name == value.Name);
                RaisePropertyChanged(nameof(SelectedItem));
                OnNodeModified();
            }
        }





        /// <summary>
        /// Construct a new Custom Dropdown Menu node
        /// </summary>
        public CustomSelectionNodeModel() : base("Value")
        {
            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            Items = new ObservableCollection<CustomSelectionItemViewModel>()
            {
                new CustomSelectionItemViewModel(new CustomSelectionItem() {Name = "One", Value = "1"}),
                new CustomSelectionItemViewModel(new CustomSelectionItem {Name = "Two", Value = "2"}),
                new CustomSelectionItemViewModel(new CustomSelectionItem {Name = "Three", Value = "3"}),
            };

            SelectedItem = Items.FirstOrDefault();

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
            RaisePropertyChanged(nameof(ValidItems));
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
        internal void Log(Exception ex)
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

        private void AddMenuItem()
        {
            var newItem = new CustomSelectionItemViewModel(new CustomSelectionItem());
            InitItem(newItem);

            Items.Add(newItem);
        }


        private bool IsUnique(CustomSelectionItem item)
        {
            return Items.Count(x => x.Name == item.Name) <= 1;
        }


        private void OnItemChanged()
        {
            RaisePropertyChanged(nameof(ValidItems));

            foreach (var item in Items)
                item.Validate();

            OnNodeModified();
        }


        private void OnRemoveRequested(CustomSelectionItemViewModel item)
        {
            item.IsUnique -= IsUnique;
            item.ItemChanged -= OnItemChanged;
            item.RemoveRequested -= OnRemoveRequested;
            Items.Remove(item);
            RaisePropertyChanged(nameof(ValidItems));
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
            if (Items.All(item => int.TryParse(item.Value, out intValue)))
            {
                int.TryParse(SelectedItem.Value, out intValue);
                return intValue;
            }

            double doubleValue = 0.0;
            if (Items.All(item => double.TryParse(item.Value, out doubleValue)))
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
            var enumItemsNode = xmlDocument.CreateElement("Items");

            foreach (CustomSelectionItemViewModel item in Items)
            {
                if (item.IsValid)
                {
                    var itemNode = xmlDocument.CreateElement("Item");
                    itemNode.SetAttribute("Name", item.Name);
                    itemNode.SetAttribute("Value", item.Value?.ToString());

                    enumItemsNode.AppendChild(itemNode);
                }
            }

            element.AppendChild(enumItemsNode);

            if (SelectedItem != null)
            {
                var selectedItemNode = xmlDocument.CreateElement("SelectedItem");
                selectedItemNode.SetAttribute("Name", SelectedItem.Name);

                element.AppendChild(selectedItemNode);
            }
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("Items"))
                {
                    DeserializeEnumItems(subNode);
                }

                if (subNode.Name.Equals("SelectedItem"))
                {
                    foreach (XmlAttribute attribute in subNode.Attributes)
                    {
                        if (attribute.Name == "Name")
                        {
                            SelectedItem = Items.FirstOrDefault(item => item.Name == attribute.Value);
                        }
                    }
                }
            }
        }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            base.Items.Clear();

            foreach (CustomSelectionItemViewModel enumItem in Items)
            {
                base.Items.Add(new DynamoDropDownItem(enumItem.Name, enumItem.Value));
            }

            SelectedIndex = 0;

            return SelectionState.Restore;
        }

        private void DeserializeEnumItems(XmlNode itemsNode)
        {
            Items.Clear();

            foreach (XmlNode childNode in itemsNode.ChildNodes)
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

                Items.Add(newItem);
                RaisePropertyChanged(nameof(Items));
            }
        }


        class AddMenuItemCommand : ICommand
        {
            private readonly Action execute;
            public event EventHandler CanExecuteChanged;

            public AddMenuItemCommand(Action execute)
            {
                this.execute = execute;
            }

            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => execute();
        }
    }
}