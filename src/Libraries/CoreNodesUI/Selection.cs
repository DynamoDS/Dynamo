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

    /// <summary>
    /// The base class for all selection nodes.
    /// </summary>
    /// <typeparam name="T1">The type which is used to constrain the selection.</typeparam>
    /// <typeparam name="T2">The type which is returned from the selection or subselection.</typeparam>
    public abstract class SelectionBase<T1, T2> : NodeModel, IWpfNode
    {
        protected bool canSelect = true;
        protected string selectionMessage;
        protected List<T1> selection = new List<T1>();
        protected List<T2> subSelection = new List<T2>();
        protected SelectionType selectionType;
        protected SelectionObjectType selectionObjectType;
        protected ILogger logger;
        protected Func<List<T1>, List<T2>> SubElementUpdate;

        #region public properties

        /// <summary>
        /// A list of selected model objects
        /// </summary>
        public virtual List<T1> Selection
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

        /// <summary>
        /// A list of model objects which are sub-selections
        /// of those in Selection
        /// </summary>
        public virtual List<T2> SubSelection
        {
            get { return subSelection; }
            set
            {
                bool dirty = value != null;

                subSelection = value;

                if (dirty)
                {
                    RequiresRecalc = true;
                }

                RaisePropertyChanged("SubSelection");
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
            if (SubSelection.Any())
            {
                return string.Format("{0} : {1}", Prefix, FormatSelectionText(SubSelection));
            }
            
            if (Selection.Any())
            {
                return string.Format("{0} : {1}", Prefix, FormatSelectionText(Selection));
            }

            return "Nothing selected.";
        }

        public void ClearSelections()
        {
            Selection = new List<T1>();
            SubSelection = new List<T2>();
        }

        #endregion

        #region private methods

        protected bool CanBeginSelect(object parameter)
        {
            return CanSelect;
        }

        protected virtual string FormatSelectionText<T>(List<T> elements)
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

                // Call the delegate associated with a selection type
                Selection =
                    SelectionHelper.RequestSelectionOfType<T1>(
                        selectionMessage,
                        selectionType,
                        selectionObjectType,
                        logger);

                // If there is a sub element selctor, then run it
                // using the selection as an input. If not, attempt
                // to cast the temp selection type to the 
                // stored collection type.
                if (SubElementUpdate != null)
                {
                    SubSelection = SubElementUpdate.Invoke(Selection); 
                }

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

            // ELEMENTS
            var uuidsSelection =
                Selection.Select(x=>GetIdentifierFromModelObject(x)).Where(x => x != null);

            foreach (var id in uuidsSelection)
            {
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", id);
                nodeElement.AppendChild(outEl);
            }

            // SUB ELEMENTS
            //var uuidsSubSelection =
            //    SubSelection.Select(x=>GetIdentifierFromModelObject(x)).Where(x => x != null);

            //foreach (var id in uuidsSubSelection)
            //{
            //    XmlElement outEl = xmlDoc.CreateElement("instance_sub");
            //    outEl.SetAttribute("id", id);
            //    nodeElement.AppendChild(outEl);
            //}
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            // ELEMENTS
            var savedUuids =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name.Equals("instance") && subNode.Attributes != null)
                    .Select(subNode => subNode.Attributes[0].Value)
                    .ToList();

            Selection =
                    savedUuids.Select(GetModelObjectFromIdentifer).Cast<T1>()
                        .Where(el => el != null).ToList();


            if (SubElementUpdate != null)
            {
                SubSelection = SubElementUpdate.Invoke(Selection);
            }

            // SUB ELEMENTS
            //var saved_subUuids =
            //    nodeElement.ChildNodes.Cast<XmlNode>()
            //        .Where(subNode => subNode.Name.Equals("instance_sub") && subNode.Attributes != null)
            //        .Select(subNode => subNode.Attributes[0].Value)
            //        .ToList();

            //SubSelection =
            //    saved_subUuids.Select(GetModelObjectFromIdentifer).Cast<T2>()
            //        .Where(el => el != null).ToList();

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
        protected virtual object GetModelObjectFromIdentifer(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get an object's unique identifier.
        /// </summary>
        /// <param name="modelObject"></param>
        /// <returns>A unique identifier or null if no unique identifier can be derived.</returns>
        protected virtual string GetIdentifierFromModelObject(object modelObject)
        {
            throw new NotImplementedException();
        }

        protected virtual object GetParentForObject(object modelObject)
        {
            throw new NotImplementedException();
        }

        protected void UpdateSubElements()
        {
            if (SubElementUpdate == null) return;
            SubSelection.Clear();
            SubSelection = SubElementUpdate.Invoke(Selection);
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
