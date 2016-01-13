using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CoreNodeModels.Properties;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;

namespace CoreNodeModels
{
    public delegate List<string> ElementsSelectionDelegate(string message,
    SelectionType selectionType, SelectionObjectType objectType);

    /// <summary>
    /// The base class for all selection nodes.
    /// </summary>
    /// <typeparam name="TSelection">The type which is used to constrain the selection.</typeparam>
    /// <typeparam name="TResult">The type which is returned from the selection or subselection.</typeparam>
    public abstract class SelectionBase<TSelection, TResult> : NodeModel
    {
        private bool canSelect = true;
        private readonly string selectionMessage;
        private List<TResult> selectionResults = new List<TResult>();
        private List<TSelection> selection = new List<TSelection>();
        private readonly SelectionType selectionType;
        private readonly SelectionObjectType selectionObjectType;
        
        #region public properties

        public IEnumerable<TSelection> Selection { get { return selection; } } 

        /// <summary>
        /// A list of selected model objects
        /// </summary>
        public IEnumerable<TResult> SelectionResults
        {
            get { return selectionResults; }
            private set
            {
                bool dirty = value != null;

                if (dirty)
                {
                    selectionResults = value.ToList();
                    
                    OnNodeModified(forceExecute:true);
                }
                else
                    selectionResults = null;

                SetSelectionNodeState();

                RaisePropertyChanged("SelectionResults");
                RaisePropertyChanged("Text");
            }
        }

        public string Prefix { get; set; }

        /// <summary>
        /// Whether or not the Select button is enabled in the UI.
        /// </summary>
        public virtual bool CanSelect
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

        public abstract IModelSelectionHelper<TSelection> SelectionHelper { get; }

        /// <summary>
        /// A string which can be used to convey information about the
        /// selection state to the user. This is different than the SelectionMessage
        /// which is used to prompt the user.
        /// </summary>
        public virtual string SelectionSuggestion
        {
            get { return Resources.SelectionNodeSugestion; }
        }

        #endregion

        #region protected constructors

        protected SelectionBase(
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix)
        {
            selectionMessage = message;

            this.selectionType = selectionType;
            this.selectionObjectType = selectionObjectType;

            string portName = GetOutputPortName();
            OutPortData.Add(new PortData(portName, Resources.SelectionPortDataResultToolTip));

            RegisterAllPorts();

            Prefix = prefix;

            State = ElementState.Warning; 
            
            ShouldDisplayPreviewCore = false;
        }

        #endregion

        #region public methods

        public override string ToString()
        {
            return SelectionResults.Any()
                ? string.Format("{0} : {1}", Prefix, FormatSelectionText(SelectionResults))
                : Resources.SelectionNodeNothingSelected;
        }

        public void ClearSelections()
        {
            SelectionResults = new List<TResult>();
        }

        public override void ClearRuntimeError()
        {
            //do nothing as the errors for the selection nodes
            //are not created when running the graph
        }

        #endregion

        #region private methods

        private void SetSelectionNodeState()
        {
            if (null == selectionResults || selectionResults.Count == 0)
                State = ElementState.Warning;
            else if (State == ElementState.Warning)
                State = ElementState.Active;
        }

        public bool CanBeginSelect()
        {
            return CanSelect;
        }

        protected virtual string FormatSelectionText<T>(IEnumerable<T> elements)
        {
            return elements.Any()
                ? System.String.Join(" ", SelectionResults.Take(20).Select(x=>x.ToString()))
                : "";
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Callback when selection button is clicked. 
        /// Calls the selection action, and stores the ElementId(s) of the selected objects.
        /// </summary>
        public virtual void Select(object parameter)
        {
            try
            {
                CanSelect = false;

                // Call the delegate associated with a selection type
                var newSelection =
                    SelectionHelper.RequestSelectionOfType(
                        selectionMessage,
                        selectionType,
                        selectionObjectType);

                // If there is a sub element selector, then run it
                // using the selection as an input. If not, attempt
                // to cast the temp selection type to the 
                // stored collection type.
                UpdateSelection(newSelection);
                
                CanSelect = true;
            }
            catch (Exception e)
            {
                CanSelect = true;
                Log(LogMessage.Error(e));
            }
        }

        protected abstract IEnumerable<TResult> ExtractSelectionResults(TSelection selections);

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            if (!SelectionResults.Any()) return;

            var uuidsSelection =
                selection.Select(GetIdentifierFromModelObject).Where(x => x != null);

            foreach (var id in uuidsSelection)
            {
                XmlElement outEl = nodeElement.OwnerDocument.CreateElement("instance");
                outEl.SetAttribute("id", id);
                nodeElement.AppendChild(outEl);
            }
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            var savedUuids =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name.Equals("instance") && subNode.Attributes != null)
                    .Select(subNode => subNode.Attributes[0].Value)
                    .ToList();

            var loadedSelection =
                    savedUuids.Select(GetModelObjectFromIdentifer)
                        // ReSharper disable once CompareNonConstrainedGenericWithNull
                        .Where(el => el != null);

            UpdateSelection(loadedSelection);

            OnNodeModified();
            RaisePropertyChanged("SelectionResults");
        }

        /// <summary>
        /// Get an object in the model from a string identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The object or null if the object cannot be found.</returns>
        protected abstract TSelection GetModelObjectFromIdentifer(string id);

        /// <summary>
        /// Get an object's unique identifier.
        /// </summary>
        /// <param name="modelObject"></param>
        /// <returns>A unique identifier or null if no unique identifier can be derived.</returns>
        protected abstract string GetIdentifierFromModelObject(TSelection modelObject);

        public virtual void UpdateSelection(IEnumerable<TSelection> newSelection)
        {
            selection = newSelection.ToList();
            SelectionResults = selection.SelectMany(ExtractSelectionResults);
        }

        protected virtual string GetOutputPortName()
        {
            switch (selectionObjectType)
            {
                case SelectionObjectType.Edge:
                    if (selectionType == SelectionType.One)
                    {
                        return Resources.SelectionEdgeOutputPortName;
                    }
                    else
                    {
                        return Resources.SelectionEdgesOutputPortName;
                    }
                case SelectionObjectType.Face:
                    if (selectionType == SelectionType.One)
                    {
                        return Resources.SelectionFaceOutputPortName;
                    }
                    else
                    {
                        return Resources.SelectionFacesOutputPortName;
                    }
                case SelectionObjectType.PointOnFace:
                    if (selectionType == SelectionType.One)
                    {
                        return Resources.SelectionPointOutputPortName;
                    }
                    else
                    {
                        return Resources.SelectionPointsOutputPortName;
                    }
                default:
                    if (selectionType == SelectionType.One)
                    {
                        return Resources.SelectionElementOutputPortName;
                    }
                    else
                    {
                        return Resources.SelectionElementsOutputPortName;
                    }
            }
        }

        #endregion
    }
}
