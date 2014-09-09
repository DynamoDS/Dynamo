using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

using Dynamo.Applications;
using Dynamo.Applications.Models;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using Revit.Elements;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using ProtoCore.AST.AssociativeAST;

using Revit.GeometryObjects;
using Revit.Interactivity;

using RevitServices.Elements;
using RevitServices.Persistence;
using Element = Revit.Elements.Element;

namespace Dynamo.Nodes
{
    public abstract class DSSelectionBase : RevitNodeModel, IWpfNode
    {
        protected bool _canSelect = true;
        protected string _selectionText ="";
        protected string _selectionMessage;
        protected string _selectButtonContent;

        protected DSSelectionBase(WorkspaceModel workspaceModel) : base(workspaceModel) { }

        /// <summary>
        /// The text that describes this selection.
        /// </summary>
        public virtual string SelectionText
        {
            get
            {
                return _selectionText;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        /// <summary>
        /// Whether or not the Select button is enabled in the UI.
        /// </summary>
        public bool CanSelect
        {
            get { return _canSelect; }
            set
            {
                _canSelect = value;
                RaisePropertyChanged("CanSelect");
            }
        }

        /// <summary>
        /// The content of the selection button.
        /// </summary>
        public string SelectButtonContent
        {
            get { return _selectButtonContent; }
            set
            {
                _selectButtonContent = value;
                RaisePropertyChanged("SelectButtonContent");
            }
        }

        /// <summary>
        /// Handler for the selection button's Click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void selectButton_Click(object sender, RoutedEventArgs e)
        {
            //Disable the button once it's been clicked...
            CanSelect = false;
            RevitServices.Threading.IdlePromise.ExecuteOnIdleAsync(
                delegate
                {
                    OnSelectClick();
                    CanSelect = true; //...and re-enable it once selection has finished.
                });
        }

        /// <summary>
        /// Override this to perform custom selection logic.
        /// </summary>
        protected abstract void OnSelectClick();

        public abstract void SetupCustomUIElements(dynNodeView view);

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }
    }

    public abstract class DSElementSelection : DSSelectionBase 
    {
        protected Func<string, ILogger, ElementId> SelectionAction;

        private string selectedUniqueId;
        private ElementId selectedElement;
        private Document selectionOwner;

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public ElementId SelectedElement
        {
            get { return selectedElement; }
            set
            {
                bool dirty;
                if (selectedElement != null)
                {
                    if (value != null && value.Equals(selectedElement))
                        return;

                    dirty = true;
                }
                else
                    dirty = value != null;
                
                selectedElement = value;
                if (selectedElement == null)
                {
                    selectionOwner = null;
                    selectedUniqueId = null;
                }
                else
                {
                    selectionOwner = DocumentManager.Instance.CurrentDBDocument;
                    var el = selectionOwner.GetElement(selectedElement);
                    if (el != null)
                    {
                        selectedUniqueId = el.UniqueId;
                    }
                    else
                    {
                        selectedUniqueId = string.Empty;
                    }
                }

                if (dirty)
                {

                    RequiresRecalc = true;
                }

                RaisePropertyChanged("SelectedElement");
            }
        }

        public override string SelectionText
        {
            get
            {
                return SelectedElement == null
                    ? "Nothing Selected"
                    : string.Format("Element ID: {0}", SelectedElement);
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public override bool ForceReExecuteOfNode
        {
            get { return true; }
        }

        #region protected constructors

        protected DSElementSelection(WorkspaceModel workspaceModel, Func<string, ILogger, ElementId> action, string message) : base(workspaceModel)
        {
            SelectionAction = action;
            _selectionMessage = message;

            OutPortData.Add(new PortData("Element", "The selected element."));
            RegisterAllPorts();

            var revMod = workspaceModel.DynamoModel as RevitDynamoModel;
            if (revMod == null) return;

            revMod.RevitServicesUpdater.ElementsModified += Updater_ElementsModified;
            revMod.RevitServicesUpdater.ElementsDeleted += Updater_ElementsDeleted;
            revMod.RevitDocumentChanged += Controller_RevitDocumentChanged;
        }

        void Controller_RevitDocumentChanged(object sender, EventArgs e)
        {
            SelectedElement = null;
            RaisePropertyChanged("SelectedElement");
            RaisePropertyChanged("SelectionText");
        }

        public override void Destroy()
        {
            base.Destroy();

            RevitDynamoModel.RevitServicesUpdater.ElementsModified -= Updater_ElementsModified;
            RevitDynamoModel.RevitServicesUpdater.ElementsDeleted -= Updater_ElementsDeleted;
        }

        #endregion

        #region public methods

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var selectButton = new DynamoNodeButton()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Height = Configurations.PortHeightInPixels,
            };
            selectButton.Click += selectButton_Click;

            var tb = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0)),
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                IsReadOnlyCaretVisible = false
            };

            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            nodeUI.inputGrid.Children.Add(tb);
            nodeUI.inputGrid.Children.Add(selectButton);

            System.Windows.Controls.Grid.SetRow(selectButton, 0);
            System.Windows.Controls.Grid.SetRow(tb, 1);

            tb.DataContext = this;
            selectButton.DataContext = this;

            var selectTextBinding = new System.Windows.Data.Binding("SelectionText")
            {
                Mode = BindingMode.TwoWay,
            };
            tb.SetBinding(TextBox.TextProperty, selectTextBinding);

            var buttonTextBinding = new System.Windows.Data.Binding("SelectedElement")
            {
                Mode = BindingMode.TwoWay,
                Converter = new SelectionButtonContentConverter(),
            };
            selectButton.SetBinding(ContentControl.ContentProperty, buttonTextBinding);

            var buttonEnabledBinding = new System.Windows.Data.Binding("CanSelect")
            {
                Mode = BindingMode.TwoWay,
            };
            selectButton.SetBinding(Button.IsEnabledProperty, buttonEnabledBinding);
        }

        #endregion

        #region ElementSync

        void Updater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted)
        {
            if (SelectedElement != null && document.Equals(selectionOwner) && deleted.Contains(SelectedElement))
            {
                SelectedElement = null;

                RaisePropertyChanged("SelectedElement");
                RaisePropertyChanged("SelectionText");
                RequiresRecalc = true;
            }
        }

        void Updater_ElementsModified(IEnumerable<string> updated)
        {
            if (SelectedElement != null && updated.Contains(selectedUniqueId))
            {
                RequiresRecalc = true;
            }
        }

        #endregion

        /// <summary>
        /// Callback when selection button is clicked. 
        /// Calls the selection action, and stores the ElementId(s) of the selected objects.
        /// </summary>
        protected override void OnSelectClick()
        {
            try
            {
                //call the delegate associated with a selection type
                SelectedElement = SelectionAction(_selectionMessage, this.RevitDynamoModel.Logger);
                RaisePropertyChanged("SelectionText");
                RequiresRecalc = true;
            }
            catch (OperationCanceledException)
            {
                CanSelect = true;
            }
            catch (Exception e)
            {
                RevitDynamoModel.Logger.Log(e);
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // When there's no selection, this returns an invalid ID.
            var selectedElementId = selectedUniqueId ?? "";

            AssociativeNode node;
            if (string.IsNullOrEmpty(selectedElementId))
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                node = AstFactory.BuildFunctionCall(
                new Func<string, bool, Element>(ElementSelector.ByUniqueId),
                new List<AssociativeNode>
                {
                    AstFactory.BuildStringNode(selectedElementId),
                    AstFactory.BuildBooleanNode(true)
                });
            }

            return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node)};
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            if (SelectedElement != null)
            {
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", selectedUniqueId);
                nodeElement.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            string id = (from XmlNode subNode in nodeElement.ChildNodes
                         where subNode.Name.Equals("instance")
                         where subNode.Attributes != null
                         select subNode.Attributes[0].Value).LastOrDefault();

            if (id != null && DocumentManager.Instance.ElementExistsInDocument(new ElementUUID(id)))
            {
                SelectedElement = DocumentManager.Instance.CurrentDBDocument.GetElement(id).Id;                
            }
        }
    }

    public abstract class DSReferenceSelection : DSSelectionBase
    {
        protected Reference Selected;
        protected Func<string, ILogger, Reference> SelectionAction;

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public virtual Reference SelectedElement
        {
            get { return Selected; }
            set
            {
                bool dirty = value != null;

                Selected = value;

                if (dirty)
                {
                    RequiresRecalc = true;

                }

                RaisePropertyChanged("SelectedElement");
            }
        }

        public override string SelectionText
        {
            get
            {
                return Selected == null
                                        ? "Nothing Selected"
                                        : string.Format("Reference Id: {0}", Selected.ElementId);
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public override bool ForceReExecuteOfNode
        {
            get { return true; }
        }

        #region protected constructors

        protected DSReferenceSelection(WorkspaceModel workspaceModel, Func<string, ILogger, Reference> action, string message) : base(workspaceModel)
        {
            SelectionAction = action;
            _selectionMessage = message;

            OutPortData.Add(new PortData("Reference", "The geometry reference."));
            RegisterAllPorts();

            // we need to obtain the dynamo model directly from the workspace model 
            // here, as it is not yet initialized on the base class
            var revMod = workspaceModel.DynamoModel as RevitDynamoModel;
            if (revMod == null) return;

            var u = revMod.RevitServicesUpdater;
            u.ElementsModified += u_ElementsModified;

            revMod.RevitDocumentChanged += Controller_RevitDocumentChanged;
        }

        void Controller_RevitDocumentChanged(object sender, EventArgs e)
        {
            SelectedElement = null;
            RaisePropertyChanged("SelectedElement");
            RaisePropertyChanged("SelectionText");
        }

        void u_ElementsModified(IEnumerable<string> updated)
        {
            var enumerable = updated as string[] ?? updated.ToArray();

            if (Selected == null || !enumerable.Any()) return;
 
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if(enumerable.Contains(doc.GetElement(Selected).UniqueId))
            {
                RequiresRecalc = true;
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            var u = RevitDynamoModel.RevitServicesUpdater;
            u.ElementsModified -= u_ElementsModified;
        }

        #endregion

        #region public methods

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var selectButton = new DynamoNodeButton
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Height = Configurations.PortHeightInPixels,
            };
            selectButton.Click += selectButton_Click;

            var tb = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0)),
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                IsReadOnlyCaretVisible = false
            };

            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            nodeUI.inputGrid.Children.Add(tb);
            nodeUI.inputGrid.Children.Add(selectButton);

            System.Windows.Controls.Grid.SetRow(selectButton, 0);
            System.Windows.Controls.Grid.SetRow(tb, 1);

            tb.DataContext = this;
            selectButton.DataContext = this;

            var selectTextBinding = new System.Windows.Data.Binding("SelectionText")
            {
                Mode = BindingMode.TwoWay,
            };
            tb.SetBinding(TextBox.TextProperty, selectTextBinding);

            var buttonTextBinding = new System.Windows.Data.Binding("SelectedElement")
            {
                Mode = BindingMode.OneWay,
                Converter = new SelectionButtonContentConverter(),
            };
            selectButton.SetBinding(ContentControl.ContentProperty, buttonTextBinding);

            var buttonEnabledBinding = new System.Windows.Data.Binding("CanSelect")
            {
                Mode = BindingMode.TwoWay,
            };
            selectButton.SetBinding(Button.IsEnabledProperty, buttonEnabledBinding);
        }

        #endregion

        /// <summary>
        /// Callback when selection button is clicked. 
        /// Calls the selection action, and stores the ElementId(s) of the selected objects.
        /// </summary>
        protected override void OnSelectClick()
        {
            try
            {
                //call the delegate associated with a selection type
                SelectedElement = SelectionAction(_selectionMessage, this.RevitDynamoModel.Logger);
                RaisePropertyChanged("SelectionText");

                RequiresRecalc = true;
            }
            catch (OperationCanceledException)
            {
                CanSelect = true;
            }
            catch (Exception e)
            {
                Workspace.DynamoModel.Logger.Log(e);
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            GeometryObject geob = null;
            string stableRep = string.Empty;

            AssociativeNode node;

            if (SelectedElement != null)
            {
                var dbDocument = DocumentManager.Instance.CurrentDBDocument;
                if (dbDocument != null)
                {
                    var element = dbDocument.GetElement(SelectedElement);
                    if (element != null)
                        geob = element.GetGeometryObjectFromReference(SelectedElement);
                }

                stableRep = SelectedElement.ConvertToStableRepresentation(dbDocument);

                var args = new List<AssociativeNode>
                {
                    AstFactory.BuildStringNode(stableRep)
                };

                node = AstFactory.BuildFunctionCall(new Func<string, object>(GeometryObjectSelector.ByReferenceStableRepresentation),args);
            }
            else
            {
                node = AstFactory.BuildNullNode();
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            if (SelectedElement != null)
            {
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", SelectedElement.ConvertToStableRepresentation(DocumentManager.Instance.CurrentDBDocument));
                nodeElement.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("instance"))
                {
                    Reference saved = null;
                    var id = subNode.Attributes[0].Value;
                    try
                    {
                        saved = Reference.ParseFromStableRepresentation(
                            DocumentManager.Instance.CurrentDBDocument, id);
                    }
                    catch
                    {
                        RevitDynamoModel.Logger.Log(
                            "Unable to find reference with stable id: " + id);
                    }
                    SelectedElement = saved;
                }
            }
        }
    }

    public abstract class DSElementsSelection : DSSelectionBase
    {
        protected Func<string, ILogger, List<ElementId>> SelectionAction;

        private List<string> selectedUniqueIds = new List<string>();
        private List<ElementId> selectedElements = new List<ElementId>();
        private Document selectionOwner;

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public List<ElementId> SelectedElement
        {
            get { return selectedElements; }
            set
            {
                bool dirty = value != null;

                selectedElements = value;

                if (selectedElements != null)
                {
                    selectionOwner = DocumentManager.Instance.CurrentDBDocument;
                    
                    selectedUniqueIds.Clear();

                    var elements = selectedElements.Select(id => selectionOwner.GetElement(id))
                        .Where(el => el != null).Select(el=>el.UniqueId);
                    selectedUniqueIds.AddRange(elements);
                }

                if (dirty)
                {
                    RequiresRecalc = true;
                }

                RaisePropertyChanged("SelectedElement");
            }
        }

        public override string SelectionText
        {
            get
            {
                var sb = new StringBuilder();
                int count = 0;
                while (count < Math.Min(SelectedElement.Count, 10))
                {
                    sb.Append(SelectedElement[count] + ",");
                    count++;
                }
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1).Append("...");
                }

                return "Elements:" + sb;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        #region protected constructors

        protected DSElementsSelection(WorkspaceModel workspaceModel, Func<string, ILogger, List<ElementId>> action, string message) 
            : base(workspaceModel)
        {
            SelectionAction = action;
            _selectionMessage = message;

            OutPortData.Add(new PortData("Elements", "The selected elements."));
            RegisterAllPorts();

            // we need to obtain the dynamo model directly from the workspace model 
            // here, as it is not yet initialized on the base constructor
            var revMod = workspaceModel.DynamoModel as RevitDynamoModel;
            if (revMod == null) return;

            revMod.RevitServicesUpdater.ElementsModified += Updater_ElementsModified;
            revMod.RevitServicesUpdater.ElementsDeleted += Updater_ElementsDeleted;
            revMod.RevitDocumentChanged += Controller_RevitDocumentChanged;
        }

        void Controller_RevitDocumentChanged(object sender, EventArgs e)
        {
            SelectedElement.Clear();
            RaisePropertyChanged("SelectedElement");
            RaisePropertyChanged("SelectionText");
        }

        public override void Destroy()
        {
            base.Destroy();

            RevitDynamoModel.RevitServicesUpdater.ElementsModified -= Updater_ElementsModified;
            RevitDynamoModel.RevitServicesUpdater.ElementsDeleted -= Updater_ElementsDeleted;
        }

        #endregion

        #region ElementSync

        void Updater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted)
        {
            if (SelectedElement != null && document.Equals(selectionOwner))
            {
                SelectedElement = SelectedElement.Where(x => !deleted.Contains(x)).ToList();

                RaisePropertyChanged("SelectedElement");
                RaisePropertyChanged("SelectionText");
                RequiresRecalc = true;
            }
        }

        void Updater_ElementsModified(IEnumerable<string> updated)
        {
            if (SelectedElement != null && selectedUniqueIds.Any(updated.Contains))
            {

                RequiresRecalc = true;
            }
        }

        #endregion

        #region public methods

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var selectButton = new DynamoNodeButton
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Height = Configurations.PortHeightInPixels,
            };
            selectButton.Click += selectButton_Click;

            var tb = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0)),
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                IsReadOnlyCaretVisible = false,
                MaxWidth = 200,
                TextWrapping = TextWrapping.Wrap
            };

            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            nodeUI.inputGrid.Children.Add(tb);
            nodeUI.inputGrid.Children.Add(selectButton);

            System.Windows.Controls.Grid.SetRow(selectButton, 0);
            System.Windows.Controls.Grid.SetRow(tb, 1);

            tb.DataContext = this;
            selectButton.DataContext = this;

            var selectTextBinding = new System.Windows.Data.Binding("SelectionText")
            {
                Mode = BindingMode.TwoWay,
            };
            tb.SetBinding(TextBox.TextProperty, selectTextBinding);

            var buttonTextBinding = new System.Windows.Data.Binding("SelectedElement")
            {
                Mode = BindingMode.OneWay,
                Converter = new SelectionButtonContentConverter(),
            };
            selectButton.SetBinding(ContentControl.ContentProperty, buttonTextBinding);

            var buttonEnabledBinding = new System.Windows.Data.Binding("CanSelect")
            {
                Mode = BindingMode.TwoWay,
            };
            selectButton.SetBinding(Button.IsEnabledProperty, buttonEnabledBinding);
        }

        #endregion

        /// <summary>
        /// Callback when selection button is clicked. 
        /// Calls the selection action, and stores the ElementId(s) of the selected objects.
        /// </summary>
        protected override void OnSelectClick()
        {
            try
            {
                //call the delegate associated with a selection type
                SelectedElement = SelectionAction(_selectionMessage, this.RevitDynamoModel.Logger);
                RaisePropertyChanged("SelectionText");

                RequiresRecalc = true;
            }
            catch (OperationCanceledException)
            {
                CanSelect = true;
            }
            catch (Exception e)
            {
                this.RevitDynamoModel.Logger.Log(e);
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (SelectedElement == null || !SelectedElement.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var els = selectedUniqueIds;

                var newInputs = els.Select(el =>
                    AstFactory.BuildFunctionCall(
                    new Func<string, bool, Element>(ElementSelector.ByUniqueId),
                    new List<AssociativeNode>
                    {
                        AstFactory.BuildStringNode(el),
                        AstFactory.BuildBooleanNode(true)
                    }
                    )).ToList();

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            if (SelectedElement != null)
            {
                foreach (string selectedElement in selectedUniqueIds.Where(x => x != null))
                {
                    XmlElement outEl = xmlDoc.CreateElement("instance");
                    outEl.SetAttribute("id", selectedElement);
                    nodeElement.AppendChild(outEl);
                }
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            selectionOwner = DocumentManager.Instance.CurrentDBDocument;

            selectedUniqueIds =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name.Equals("instance") && subNode.Attributes != null)
                    .Select(subNode => subNode.Attributes[0].Value)
                    .ToList();

            selectedElements =
                selectedUniqueIds
                    .Where(id => DocumentManager.Instance.ElementExistsInDocument(new ElementUUID(id)))
                    .Select(selectionOwner.GetElement)
                    .Select(x => x.Id)
                    .ToList();

            RequiresRecalc = true;
            RaisePropertyChanged("SelectedElement");
        }
    }

    internal class SelectionButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "Select";

            if (value.GetType() == typeof (List<Element>))
            {
                var els = (List<Element>) value;
                if (!els.Any())
                    return "Select";
            }

            return "Change";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [NodeName("Select Analysis Results")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select analysis results from the document.")]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
    public class DSAnalysisResultSelection : DSElementSelection
    {
        public DSAnalysisResultSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestAnalysisResultInstanceSelection, "Select an analysis result.")
        { }
    }

    [NodeName("Select Model Element")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a model element from the document.")]
    [IsDesignScriptCompatible]
    public class DSModelElementSelection : DSElementSelection
    {
        public DSModelElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestModelElementSelection, "Select Model Element")
        { }
    }
    
    [NodeName("Select Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a face.")]
    [IsDesignScriptCompatible]
    public class DSFaceSelection : DSReferenceSelection
    {
        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : "Face of Element ID: " + SelectedElement.ElementId;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSFaceSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestFaceReferenceSelection, "Select a face.") { }
    }

    [NodeName("Select Edge")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select an edge.")]
    [IsDesignScriptCompatible]
    public class DSEdgeSelection : DSReferenceSelection
    {
        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : "Edge of Element ID: " + SelectedElement.ElementId;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSEdgeSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestEdgeReferenceSelection, "Select an edge.")
        { }
    }

    [NodeName("Select Point on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a point on a face.")]
    [IsDesignScriptCompatible]
    public class DSPointOnElementSelection : DSReferenceSelection
    {
        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : "Point on element" + " (" + SelectedElement.ElementId + ")";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public override Reference SelectedElement
        {
            get { return Selected; }
            set
            {
                bool dirty = value != null;

                Selected = value;

                if (dirty)
                {
                    RequiresRecalc = true;
                }

                RaisePropertyChanged("SelectedElement");
            }
        }

        public DSPointOnElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestReferenceXYZSelection, "Select a point on a face.")
        { }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var dbDocument = DocumentManager.Instance.CurrentDBDocument;

            if (SelectedElement == null || dbDocument == null)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            if (SelectedElement.GlobalPoint == null)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var pt = SelectedElement.GlobalPoint;

            //this is a selected point on a face
            var ptArgs = new List<AssociativeNode>()
            {
                AstFactory.BuildDoubleNode(pt.X),
                AstFactory.BuildDoubleNode(pt.Y),
                AstFactory.BuildDoubleNode(pt.Z)
            };

            AssociativeNode node = AstFactory.BuildFunctionCall
            (
                "Autodesk.DesignScript.Geometry.Point",
                "ByCoordinates",
                ptArgs
            );
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Select UV on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a UV on a face.")]
    [IsDesignScriptCompatible]
    //MAGN-3382 [IsVisibleInDynamoLibrary(false)]
    public class DSUVOnElementSelection : DSReferenceSelection
    {
        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : "UV on element" + " (" + SelectedElement.ElementId + ")";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSUVOnElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestReferenceXYZSelection, "Select a point on a face.")
        { }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            GeometryObject geob = null;
            string stableRep = string.Empty;
            var dbDocument = DocumentManager.Instance.CurrentDBDocument;

            if (SelectedElement == null || dbDocument == null)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            if (SelectedElement.UVPoint == null)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var pt = SelectedElement.UVPoint;

            //this is a selected point on a face
            var ptArgs = new List<AssociativeNode>()
            {
                AstFactory.BuildDoubleNode(pt.U),
                AstFactory.BuildDoubleNode(pt.V)
            };

            AssociativeNode node = AstFactory.BuildFunctionCall
            (
                "Autodesk.DesignScript.Geometry.UV",
                "ByCoordinates",
                ptArgs
            );
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Select Divided Surface Families")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a divided surface and get its family instances.")]
    [IsDesignScriptCompatible]
    public class DSDividedSurfaceFamiliesSelection : DSElementsSelection
    {
        public DSDividedSurfaceFamiliesSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestDividedSurfaceFamilyInstancesSelection, "Select a divided surface.") { }
    }

    [NodeName("Select Model Elements")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select multiple elements from the Revit document.")]
    [IsDesignScriptCompatible]
    public class DSModelElementsSelection : DSElementsSelection
    {
        private static string formatSelectionText(IEnumerable<string> elements)
        {
            return elements.Any()
                ? String.Join(" ", elements.Take(20)) + "..."
                : "Nothing Selected";
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = 
                    (SelectedElement != null && SelectedElement.Count > 0)
                        ? "Element IDs:" + formatSelectionText(SelectedElement.Where(x => x != null).Select(x => x.ToString()))
                        : "Nothing Selected";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSModelElementsSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestMultipleCurveElementsSelection, "Select elements.") { }
    }
}
