using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Xml;

using DSCoreNodesUI;

using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.UI.Commands;

namespace Dynamo.Nodes
{
    public delegate List<string> ElementsSelectionDelegate(string message,
    SelectionType selectionType, SelectionObjectType objectType, ILogger logger);

    public abstract class SelectionBase : NodeModel, IWpfNode
    {
        protected bool canSelect = true;
        protected string selectionMessage;
        protected List<string> selection = new List<string>();
        protected SelectionType selectionType;
        protected SelectionObjectType selectionObjectType;

        #region public properties

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public virtual List<string> Selection
        {
            get { return selection; }
            set
            {
                bool dirty = value != null;

                selection = value;

                if (dirty)
                {
                    RequiresRecalc = true;
                }

                RaisePropertyChanged("Selection");
                RaisePropertyChanged("Text");
            }
        }

        public DelegateCommand SelectCommand { get; set; }

        public string Prefix { get; set; }

        /// <summary>
        /// Whether or not the Select button is enabled in the UI.
        /// </summary>
        public bool CanSelect
        {
            get { return canSelect; }
            set
            {
                canSelect = value;
                RaisePropertyChanged("CanSelect");
            }
        }

        public string Text
        {
            get { return ToString(); }
        }

        #endregion

        #region protected constructors

        protected SelectionBase(WorkspaceModel workspaceModel,
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix)
            : base(workspaceModel)
        {
            selectionMessage = message;

            this.selectionType = selectionType;
            this.selectionObjectType = selectionObjectType;

            OutPortData.Add(new PortData("Elements", "The selected elements."));
            RegisterAllPorts();

            SelectCommand = new DelegateCommand(Select, CanBeginSelect);
            Prefix = prefix;
        }

        #endregion

        #region public methods

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var selectionControl = new ElementSelectionControl { DataContext = this };
            nodeUI.inputGrid.Children.Add(selectionControl);
        }

        public override string ToString()
        {
            return selection.Any() ?
                string.Format("{0} : {1}", Prefix, FormatSelectionText(selection)) :
                "Nothing Selected";
        }

        #endregion

        #region private methods

        protected bool CanBeginSelect(object parameter)
        {
            return CanSelect;
        }

        protected virtual string FormatSelectionText(IEnumerable<string> elements)
        {
            return Selection.Any()
                ? System.String.Join(" ", Selection.Take(20))
                : "";
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Callback when selection button is clicked. 
        /// Calls the selection action, and stores the ElementId(s) of the selected objects.
        /// </summary>
        protected virtual void Select(object parameter){}

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            if (Selection != null)
            {
                foreach (string selectedElement in selection.Where(x => x != null))
                {
                    XmlElement outEl = xmlDoc.CreateElement("instance");
                    outEl.SetAttribute("id", selectedElement);
                    nodeElement.AppendChild(outEl);
                }
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            selection =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name.Equals("instance") && subNode.Attributes != null)
                    .Select(subNode => subNode.Attributes[0].Value)
                    .ToList();

            RequiresRecalc = true;
            RaisePropertyChanged("Selection");
        }

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }

        #endregion
    }

    public class SelectionButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<object>)
            {
                if (!(value as IEnumerable<object>).Any())
                        return "Select";
            }

            return "Change";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class SelectionToTextConverter : IValueConverter
    {
        // parameter is the data context
        // value is the selection
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
