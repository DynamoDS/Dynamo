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
using CoreNodeModels;
using CoreNodeModels.Properties;


namespace CoreNodeModels.Input
{
    [NodeName("Custom Selection")]
    [NodeDescription("NeoDropdown node for user defined enums.")]
    [NodeCategory("DropdownUI")]
    [OutPortNames("value")]
    [OutPortTypes("object")]
    [OutPortDescriptions("Selected value")]
    [IsDesignScriptCompatible]
    public class CustomSelectionNodeModel : DSDropDownBase
    {
        #region Private members

        private ObservableCollection<CustomSelectionItemViewModel> enumItems = new ObservableCollection<CustomSelectionItemViewModel>();
        private CustomSelectionItemViewModel selectedCustomSelectionItem;

        #endregion

        #region Properties


        public ObservableCollection<CustomSelectionItemViewModel> EnumItems
        {
            get { return enumItems; }
            set
            {
                enumItems = value;

                foreach (var item in enumItems)
                    InitEnumItem(item);

                PopulateItems();

                RaisePropertyChanged("EnumItems");
            }
        }

        [JsonIgnore]
        public ObservableCollection<CustomSelectionItemViewModel> ValidEnumItems
        {
            get
            {
                return new ObservableCollection<CustomSelectionItemViewModel>(EnumItems.Where(item => item.IsValid));
            }
        }


        public CustomSelectionItemViewModel SelectedItem
        {
            get { return selectedCustomSelectionItem; }
            set
            {
                selectedCustomSelectionItem = value == null ? null : ValidEnumItems.FirstOrDefault(item => item.Name == value.Name);
                RaisePropertyChanged("SelectedItem");

                SelectedIndex = 0;
                foreach (CustomSelectionItemViewModel enumItem in EnumItems)
                {
                    if (enumItem == selectedCustomSelectionItem)
                        break;
                    SelectedIndex++;
                }

                OnNodeModified();
            }
        }


        #endregion

        #region Commands

        [JsonIgnore]
        public ICommand AddCommand { get; private set; }

        #endregion

        #region Constructors

        public CustomSelectionNodeModel() : base("Value")
        {
            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            EnumItems = new ObservableCollection<CustomSelectionItemViewModel>()
            {
                new CustomSelectionItemViewModel(new CustomSelectionItem {Name = "One", Value = "1"}),
                new CustomSelectionItemViewModel(new CustomSelectionItem {Name = "Two", Value = "2"}),
                new CustomSelectionItemViewModel(new CustomSelectionItem {Name = "Three", Value = "3"}),
            };

            SelectedItem = ValidEnumItems.FirstOrDefault();

            AddCommand = new RelayCommand(AddCommandExecute);
        }


        [JsonConstructor]
        CustomSelectionNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base("Value", inPorts, outPorts)
        {
            AddCommand = new RelayCommand(AddCommandExecute);
            enumItems.CollectionChanged += EnumItems_CollectionChanged;
        }


        private void EnumItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("ValidEnumItems");
            OnNodeModified();
        }

        #endregion

        #region Public methods

        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var value = GetSelectedValue();
            var associativeNode = AstFactory.BuildPrimitiveNodeFromObject(value);

            return new List<AssociativeNode> { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), associativeNode) };
        }

        [IsVisibleInDynamoLibrary(false)]
        public void AddCommandExecute(object param)
        {
            var newItem = new CustomSelectionItemViewModel(new CustomSelectionItem());
            InitEnumItem(newItem);

            EnumItems.Add(newItem);
        }

        [IsVisibleInDynamoLibrary(false)]
        private bool IsUnique(CustomSelectionItem item)
        {
            var items = EnumItems.Where(x => x.Name == item.Name);
            return items.Count() <= 1;
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Log(Exception ex)
        {
            base.Log(ex.Message, Dynamo.Logging.WarningLevel.Error);
            base.Log(ex.StackTrace, Dynamo.Logging.WarningLevel.Error);
        }

        private void EnumItem_ItemChanged()
        {
            RaisePropertyChanged("ValidEnumItems");

            foreach (var item in EnumItems)
                item.Validate();

            OnNodeModified();
        }

        private void EnumItem_RemoveRequested(CustomSelectionItemViewModel item)
        {
            EnumItems.Remove(item);
            RaisePropertyChanged("ValidEnumItems");
            RaisePropertyChanged("SelectedItem");
        }

        private object GetSelectedValue()
        {
            SelectedItem = enumItems[SelectedIndex];

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

        private void InitEnumItem(CustomSelectionItemViewModel customSelectionItem)
        {
            customSelectionItem.IsUnique += IsUnique;
            customSelectionItem.ItemChanged += EnumItem_ItemChanged;
            customSelectionItem.RemoveRequested += EnumItem_RemoveRequested;
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);

            var xmlDocument = element.OwnerDocument;
            var enumItemsNode = xmlDocument.CreateElement("CustomSelectionItems");

            foreach (var item in EnumItems)
            {
                if (item.IsValid)
                {
                    var itemNode = xmlDocument.CreateElement("CustomSelectionItem");
                    itemNode.SetAttribute("Name", item.Name);
                    itemNode.SetAttribute("Value", item.Value == null ? null : item.Value.ToString());

                    enumItemsNode.AppendChild(itemNode);
                }
            }

            element.AppendChild(enumItemsNode);

            if (SelectedItem != null)
            {
                var selectedItemNode = xmlDocument.CreateElement("CustomSelectionSelectedItem");
                selectedItemNode.SetAttribute("Name", SelectedItem.Name);

                element.AppendChild(selectedItemNode);
            }
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("CustomSelectionItems"))
                {
                    DeserializeEnumItems(subNode);
                }

                if (subNode.Name.Equals("CustomSelectionSelectedItem"))
                {
                    foreach (XmlAttribute attribute in subNode.Attributes)
                    {
                        if (attribute.Name == "Name")
                            SelectedItem = EnumItems.FirstOrDefault(item => item.Name == attribute.Value);
                    }
                }
            }
        }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            foreach (CustomSelectionItemViewModel enumItem in EnumItems)
                Items.Add(new DynamoDropDownItem(enumItem.Name, enumItem.Value));

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
                        name = attribute.Value;
                    else if (attribute.Name == "Value")
                        value = attribute.Value;
                }

                var newItem = new CustomSelectionItemViewModel(new CustomSelectionItem { Name = name, Value = value });
                InitEnumItem(newItem);

                EnumItems.Add(newItem);
            }
        }

        #endregion
    }
}