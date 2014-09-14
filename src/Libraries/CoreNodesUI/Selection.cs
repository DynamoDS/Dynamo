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

    public class DefaultSelectionHelper : IModelSelectionHelper
    {
        public List<T> RequestSelectionOfType<T>(
            string selectionMessage, SelectionType selectionType, SelectionObjectType objectType,
            ILogger logger)
        {
            return new List<T>();
        }

        public List<T> RequestSubSelectionOfType<T>(
            string selectionMessage, SelectionType selectionType, SelectionObjectType objectType,
            ILogger logger)
        {
            return new List<T>();
        }
    }

    public abstract class SelectionBase<T> : NodeModel, IWpfNode
    {
        protected bool canSelect = true;
        protected string selectionMessage;
        protected List<T> selection = new List<T>();
        protected SelectionType selectionType;
        protected SelectionObjectType selectionObjectType;
        protected ILogger logger;

        #region public properties

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public virtual List<T> Selection
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

        public IModelSelectionHelper SelectionHelper { get; protected set; }

        #endregion

        #region protected constructors

        protected SelectionBase(WorkspaceModel workspaceModel,
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix)
            : base(workspaceModel)
        {
            SelectionHelper = new DefaultSelectionHelper();

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

        protected virtual string FormatSelectionText(List<T> elements)
        {
            return elements.Any()
                ? System.String.Join(" ", Selection.Take(20).Select(x=>x.ToString()))
                : "";
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Callback when selection button is clicked. 
        /// Calls the selection action, and stores the ElementId(s) of the selected objects.
        /// </summary>
        protected virtual void Select(object parameter)
        {
            try
            {
                CanSelect = false;

                //call the delegate associated with a selection type
                Selection =
                    SelectionHelper.RequestSelectionOfType<T>(
                        selectionMessage,
                        selectionType,
                        selectionObjectType,
                        logger);

                RequiresRecalc = true;

                CanSelect = true;
            }
            catch (Exception e)
            {
                CanSelect = true;
                logger.Log(e);
            }
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            if (!Selection.Any()) return;

            var uuids =
                Selection.Select(GetIdentifierFromModelObject).Where(x => x != null);
            foreach (var id in uuids)
            {
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", id);
                nodeElement.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            // Check the selection for valid elements
            var savedUuids =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name.Equals("instance") && subNode.Attributes != null)
                    .Select(subNode => subNode.Attributes[0].Value)
                    .ToList();

            Selection =
                savedUuids.Select(GetModelObjectFromIdentifer)
                    .Where(el => el  != null).ToList();

            RequiresRecalc = true;
            RaisePropertyChanged("Selection");
        }

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }

        /// <summary>
        /// Get an object in the model from a string identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The object or null if the object cannot be found.</returns>
        protected virtual T GetModelObjectFromIdentifer(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get an object's unique identifier.
        /// </summary>
        /// <param name="modelObject"></param>
        /// <returns>A unique identifier or null if no unique identifier can be derived.</returns>
        protected virtual string GetIdentifierFromModelObject(T modelObject)
        {
            throw new NotImplementedException();
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
