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

using Dynamo.Applications.Models;
using Dynamo.Interfaces;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using ProtoCore.AST.AssociativeAST;

using Revit.Elements;
using Revit.Interactivity;
using RevitServices.Persistence;
using Element = Revit.Elements.Element;

namespace Dynamo.Nodes
{
    public delegate List<string> ElementsSelectionUpdateDelegate(string message, out object target, SelectionType selectionType, ILogger logger);

    public abstract class SelectionBase : RevitNodeModel, IWpfNode
    {
        protected bool _canSelect = true;
        protected string _selectionText ="";
        protected string _selectionMessage;
        protected string _selectButtonContent;
        protected object _selectionTarget;

        protected SelectionBase(WorkspaceModel workspaceModel) : base(workspaceModel) { }

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

    //public abstract class ElementSelection : SelectionBase 
    //{
    //    protected Func<string, ILogger, string> SelectionAction;

    //    private string selectedUniqueId;
    //    private Document selectionOwner;

    //    /// <summary>
    //    /// The Element which is selected.
    //    /// </summary>
    //    public string SelectedUniqueId
    //    {
    //        get { return selectedUniqueId; }
    //        set
    //        {
    //            bool dirty;
    //            if (selectedUniqueId != null)
    //            {
    //                if (value != null && value.Equals(selectedUniqueId))
    //                    return;

    //                dirty = true;
    //            }
    //            else
    //                dirty = value != null;
                
    //            selectedUniqueId = value;
    //            selectionOwner = selectedUniqueId == null ? null : DocumentManager.Instance.CurrentDBDocument;

    //            if (dirty)
    //            {

    //                RequiresRecalc = true;
    //            }

    //            RaisePropertyChanged("SelectedUniqueId");
    //        }
    //    }

    //    public override string SelectionText
    //    {
    //        get
    //        {
    //            return SelectedUniqueId == null
    //                ? "Nothing Selected"
    //                : string.Format("Element ID: {0}", DocumentManager.Instance.CurrentDBDocument.GetElement(selectedUniqueId).Id);
    //        }
    //        set
    //        {
    //            _selectionText = value;
    //            RaisePropertyChanged("SelectionText");
    //        }
    //    }

    //    public override bool ForceReExecuteOfNode
    //    {
    //        get { return true; }
    //    }

    //    #region protected constructors

    //    protected ElementSelection(WorkspaceModel workspaceModel, Func<string, ILogger, string> action, string message) : base(workspaceModel)
    //    {
    //        SelectionAction = action;
    //        _selectionMessage = message;

    //        OutPortData.Add(new PortData("Element", "The selected element."));
    //        RegisterAllPorts();

    //        var revMod = workspaceModel.DynamoModel as RevitDynamoModel;
    //        if (revMod == null) return;

    //        revMod.RevitServicesUpdater.ElementsModified += Updater_ElementsModified;
    //        revMod.RevitServicesUpdater.ElementsDeleted += Updater_ElementsDeleted;
    //        revMod.RevitDocumentChanged += Controller_RevitDocumentChanged;
    //    }

    //    void Controller_RevitDocumentChanged(object sender, EventArgs e)
    //    {
    //        SelectedUniqueId = null;
    //        RaisePropertyChanged("SelectedUniqueId");
    //        RaisePropertyChanged("SelectionText");
    //    }

    //    public override void Destroy()
    //    {
    //        base.Destroy();

    //        RevitDynamoModel.RevitServicesUpdater.ElementsModified -= Updater_ElementsModified;
    //        RevitDynamoModel.RevitServicesUpdater.ElementsDeleted -= Updater_ElementsDeleted;
    //    }

    //    #endregion

    //    #region public methods

    //    public override void SetupCustomUIElements(dynNodeView nodeUI)
    //    {
    //        //add a button to the inputGrid on the dynElement
    //        var selectButton = new DynamoNodeButton()
    //        {
    //            HorizontalAlignment = HorizontalAlignment.Stretch,
    //            VerticalAlignment = VerticalAlignment.Top,
    //            Height = Configurations.PortHeightInPixels,
    //        };
    //        selectButton.Click += selectButton_Click;

    //        var tb = new TextBox
    //        {
    //            HorizontalAlignment = HorizontalAlignment.Stretch,
    //            VerticalAlignment = VerticalAlignment.Center,
    //            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0)),
    //            BorderThickness = new Thickness(0),
    //            IsReadOnly = true,
    //            IsReadOnlyCaretVisible = false
    //        };

    //        nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
    //        nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

    //        nodeUI.inputGrid.Children.Add(tb);
    //        nodeUI.inputGrid.Children.Add(selectButton);

    //        System.Windows.Controls.Grid.SetRow(selectButton, 0);
    //        System.Windows.Controls.Grid.SetRow(tb, 1);

    //        tb.DataContext = this;
    //        selectButton.DataContext = this;

    //        var selectTextBinding = new System.Windows.Data.Binding("SelectionText")
    //        {
    //            Mode = BindingMode.TwoWay,
    //        };
    //        tb.SetBinding(TextBox.TextProperty, selectTextBinding);

    //        var buttonTextBinding = new System.Windows.Data.Binding("SelectedUniqueId")
    //        {
    //            Mode = BindingMode.TwoWay,
    //            Converter = new SelectionButtonContentConverter(),
    //        };
    //        selectButton.SetBinding(ContentControl.ContentProperty, buttonTextBinding);

    //        var buttonEnabledBinding = new System.Windows.Data.Binding("CanSelect")
    //        {
    //            Mode = BindingMode.TwoWay,
    //        };
    //        selectButton.SetBinding(Button.IsEnabledProperty, buttonEnabledBinding);
    //    }

    //    #endregion

    //    #region ElementSync

    //    void Updater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted)
    //    {
    //        var uuids =
    //            deleted.Select(document.GetElement)
    //                .Where(el => el != null)
    //                .Select(el => el.UniqueId);

    //        if (!string.IsNullOrEmpty(SelectedUniqueId) && document.Equals(selectionOwner) && uuids.Contains(SelectedUniqueId))
    //        {
    //            SelectedUniqueId = null;

    //            RaisePropertyChanged("SelectedUniqueId");
    //            RaisePropertyChanged("SelectionText");
    //            RequiresRecalc = true;
    //        }
    //    }

    //    void Updater_ElementsModified(IEnumerable<string> updated)
    //    {
    //        if (SelectedUniqueId != null && updated.Contains(selectedUniqueId))
    //        {
    //            RequiresRecalc = true;
    //        }
    //    }

    //    #endregion

    //    /// <summary>
    //    /// Callback when selection button is clicked. 
    //    /// Calls the selection action, and stores the ElementId(s) of the selected objects.
    //    /// </summary>
    //    protected override void OnSelectClick()
    //    {
    //        try
    //        {
    //            //call the delegate associated with a selection type
    //            SelectedUniqueId = SelectionAction(_selectionMessage, RevitDynamoModel.Logger);
    //            RaisePropertyChanged("SelectionText");
    //            RequiresRecalc = true;
    //        }
    //        catch (OperationCanceledException)
    //        {
    //            CanSelect = true;
    //        }
    //        catch (Exception e)
    //        {
    //            RevitDynamoModel.Logger.Log(e);
    //        }
    //    }

    //    public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
    //    {
    //        // When there's no selection, this returns an invalid ID.
    //        var selectedElementId = selectedUniqueId ?? "";

    //        AssociativeNode node;
    //        if (string.IsNullOrEmpty(selectedElementId))
    //        {
    //            node = AstFactory.BuildNullNode();
    //        }
    //        else
    //        {
    //            node = AstFactory.BuildFunctionCall(
    //            new Func<string, bool, Element>(ElementSelector.ByUniqueId),
    //            new List<AssociativeNode>
    //            {
    //                AstFactory.BuildStringNode(selectedElementId),
    //                AstFactory.BuildBooleanNode(true)
    //            });
    //        }

    //        return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node)};
    //    }

    //    protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
    //    {
    //        if (SelectedUniqueId != null)
    //        {
    //            XmlElement outEl = xmlDoc.CreateElement("instance");
    //            outEl.SetAttribute("id", selectedUniqueId);
    //            nodeElement.AppendChild(outEl);
    //        }
    //    }

    //    protected override void LoadNode(XmlNode nodeElement)
    //    {
    //        string id = (from XmlNode subNode in nodeElement.ChildNodes
    //                     where subNode.Name.Equals("instance")
    //                     where subNode.Attributes != null
    //                     select subNode.Attributes[0].Value).LastOrDefault();

    //        if (id != null && DocumentManager.Instance.ElementExistsInDocument(new ElementUUID(id)))
    //        {
    //            SelectedUniqueId = DocumentManager.Instance.CurrentDBDocument.GetElement(id).UniqueId;                
    //        }
    //    }
    //}

    public abstract class ElementSelection : SelectionBase
    {
        /// <summary>
        /// The function to invoke during selection.
        /// </summary>
        protected ElementsSelectionUpdateDelegate SelectionAction;

        /// <summary>
        /// The function to invoke during update.
        /// </summary>
        protected Func<object, IEnumerable<string>> Update;

        private List<string> selection = new List<string>();
        private Document selectionOwner;
        private SelectionType selectionType;

        #region public properties

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public List<string> Selection
        {
            get { return selection; }
            set
            {
                bool dirty = value != null;

                selection = value;

                if (selection != null)
                {
                    selectionOwner = DocumentManager.Instance.CurrentDBDocument;
                }

                if (dirty)
                {
                    RequiresRecalc = true;
                }

                RaisePropertyChanged("Selection");
            }
        }

        public override string SelectionText
        {
            get
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                var ids = Selection.Select(doc.GetElement).Select(el => el.Id).ToArray();

                var sb = new StringBuilder();
                int count = 0;
                while (count < Math.Min(ids.Count(), 10))
                {
                    sb.Append(ids[count] + ",");
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

        #endregion

        #region protected constructors

        protected ElementSelection(WorkspaceModel workspaceModel, ElementsSelectionUpdateDelegate action, SelectionType selectionType, string message) 
            : base(workspaceModel)
        {
            SelectionAction = action;
            _selectionMessage = message;
            this.selectionType = selectionType;

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

        public override void Destroy()
        {
            base.Destroy();

            RevitDynamoModel.RevitServicesUpdater.ElementsModified -= Updater_ElementsModified;
            RevitDynamoModel.RevitServicesUpdater.ElementsDeleted -= Updater_ElementsDeleted;
        }

        #endregion

        #region ElementSync

        void Controller_RevitDocumentChanged(object sender, EventArgs e)
        {
            Selection.Clear();
            RaisePropertyChanged("Selection");
            RaisePropertyChanged("SelectionText");
        }

        void Updater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted)
        {
            if (Selection.Any() && document.Equals(selectionOwner))
            {
                var uuids =
                deleted.Select(document.GetElement)
                    .Where(el => el != null)
                    .Select(el => el.UniqueId);

                Selection = Selection.Where(x => !uuids.Contains(x)).ToList();

                RaisePropertyChanged("Selection");
                RaisePropertyChanged("SelectionText");
                RequiresRecalc = true;
            }
        }

        protected virtual void Updater_ElementsModified(IEnumerable<string> updated)
        {
            if (Selection != null && Selection.Any(updated.Contains))
            {
                RequiresRecalc = true;

                if (Update != null && _selectionTarget != null)
                {
                    Selection.Clear();
                    Selection = Update.Invoke(_selectionTarget).ToList();
                }
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

            var buttonTextBinding = new System.Windows.Data.Binding("selection")
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

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (Selection == null || !Selection.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var els = selection;

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

        #endregion

        #region protected methods

        /// <summary>
        /// Callback when selection button is clicked. 
        /// Calls the selection action, and stores the ElementId(s) of the selected objects.
        /// </summary>
        protected override void OnSelectClick()
        {
            try
            {
                //call the delegate associated with a selection type
                Selection = SelectionAction(_selectionMessage, out _selectionTarget, selectionType, RevitDynamoModel.Logger);

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

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
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
            selectionOwner = DocumentManager.Instance.CurrentDBDocument;

            selection =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name.Equals("instance") && subNode.Attributes != null)
                    .Select(subNode => subNode.Attributes[0].Value)
                    .ToList();

            RequiresRecalc = true;
            RaisePropertyChanged("Selection");
        }
        
        #endregion
    }

    //public abstract class ReferenceSelection : SelectionBase
    //{
    //    protected string stableReference;
    //    protected Func<string, ILogger, SelectionType, string> SelectionAction;

    //    private SelectionType selectionType;

    //    #region public properties

    //    /// <summary>
    //    /// The Element which is selected.
    //    /// </summary>
    //    public virtual string StableReference
    //    {
    //        get { return stableReference; }
    //        set
    //        {
    //            bool dirty = value != null;

    //            stableReference = value;

    //            if (dirty)
    //            {
    //                RequiresRecalc = true;

    //            }

    //            RaisePropertyChanged("StableReference");
    //        }
    //    }

    //    public override string SelectionText
    //    {
    //        get
    //        {
    //            var doc = DocumentManager.Instance.CurrentDBDocument;
    //            var el = doc.GetElement(Reference.ParseFromStableRepresentation(doc, stableReference));
    //            return stableReference == null
    //                                    ? "Nothing Selected"
    //                                    : string.Format("Reference Id: {0}", el.Id);
    //        }
    //        set
    //        {
    //            _selectionText = value;
    //            RaisePropertyChanged("SelectionText");
    //        }
    //    }

    //    public override bool ForceReExecuteOfNode
    //    {
    //        get { return true; }
    //    }

    //    #endregion

    //    #region protected constructors

    //    protected ReferenceSelection(WorkspaceModel workspaceModel, Func<string, ILogger, SelectionType, string> action, SelectionType selectionType, string message)
    //        : base(workspaceModel)
    //    {
    //        this.selectionType = selectionType;

    //        SelectionAction = action;
    //        _selectionMessage = message;

    //        OutPortData.Add(new PortData("Reference", "The geometry reference."));
    //        RegisterAllPorts();

    //        // we need to obtain the dynamo model directly from the workspace model 
    //        // here, as it is not yet initialized on the base class
    //        var revMod = workspaceModel.DynamoModel as RevitDynamoModel;
    //        if (revMod == null) return;

    //        var u = revMod.RevitServicesUpdater;
    //        u.ElementsModified += u_ElementsModified;

    //        revMod.RevitDocumentChanged += Controller_RevitDocumentChanged;
    //    }

    //    void Controller_RevitDocumentChanged(object sender, EventArgs e)
    //    {
    //        StableReference = null;
    //        RaisePropertyChanged("StableReference");
    //        RaisePropertyChanged("SelectionText");
    //    }

    //    void u_ElementsModified(IEnumerable<string> updated)
    //    {
    //        var enumerable = updated as string[] ?? updated.ToArray();

    //        if (string.IsNullOrEmpty(stableReference) || !enumerable.Any()) return;

    //        var doc = DocumentManager.Instance.CurrentDBDocument;
    //        if (enumerable.Contains(doc.GetElement(Reference.ParseFromStableRepresentation(doc, stableReference)).UniqueId))
    //        {
    //            RequiresRecalc = true;
    //        }
    //    }

    //    public override void Destroy()
    //    {
    //        base.Destroy();

    //        var u = RevitDynamoModel.RevitServicesUpdater;
    //        u.ElementsModified -= u_ElementsModified;
    //    }

    //    #endregion

    //    #region public methods

    //    public override void SetupCustomUIElements(dynNodeView nodeUI)
    //    {
    //        //add a button to the inputGrid on the dynElement
    //        var selectButton = new DynamoNodeButton
    //        {
    //            HorizontalAlignment = HorizontalAlignment.Stretch,
    //            VerticalAlignment = VerticalAlignment.Top,
    //            Height = Configurations.PortHeightInPixels,
    //        };
    //        selectButton.Click += selectButton_Click;

    //        var tb = new TextBox
    //        {
    //            HorizontalAlignment = HorizontalAlignment.Stretch,
    //            VerticalAlignment = VerticalAlignment.Center,
    //            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0)),
    //            BorderThickness = new Thickness(0),
    //            IsReadOnly = true,
    //            IsReadOnlyCaretVisible = false
    //        };

    //        nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
    //        nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

    //        nodeUI.inputGrid.Children.Add(tb);
    //        nodeUI.inputGrid.Children.Add(selectButton);

    //        System.Windows.Controls.Grid.SetRow(selectButton, 0);
    //        System.Windows.Controls.Grid.SetRow(tb, 1);

    //        tb.DataContext = this;
    //        selectButton.DataContext = this;

    //        var selectTextBinding = new System.Windows.Data.Binding("SelectionText")
    //        {
    //            Mode = BindingMode.TwoWay,
    //        };
    //        tb.SetBinding(TextBox.TextProperty, selectTextBinding);

    //        var buttonTextBinding = new System.Windows.Data.Binding("StableReference")
    //        {
    //            Mode = BindingMode.OneWay,
    //            Converter = new SelectionButtonContentConverter(),
    //        };
    //        selectButton.SetBinding(ContentControl.ContentProperty, buttonTextBinding);

    //        var buttonEnabledBinding = new System.Windows.Data.Binding("CanSelect")
    //        {
    //            Mode = BindingMode.TwoWay,
    //        };
    //        selectButton.SetBinding(Button.IsEnabledProperty, buttonEnabledBinding);
    //    }

    //    public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
    //    {
    //        AssociativeNode node;

    //        if (StableReference != null)
    //        {
    //            var args = new List<AssociativeNode>
    //            {
    //                AstFactory.BuildStringNode(stableReference)
    //            };

    //            node = AstFactory.BuildFunctionCall(new Func<string, object>(GeometryObjectSelector.ByReferenceStableRepresentation), args);
    //        }
    //        else
    //        {
    //            node = AstFactory.BuildNullNode();
    //        }

    //        return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
    //    }

    //    #endregion

    //    #region protected methods

    //    /// <summary>
    //    /// Callback when selection button is clicked. 
    //    /// Calls the selection action, and stores the ElementId(s) of the selected objects.
    //    /// </summary>
    //    protected override void OnSelectClick()
    //    {
    //        try
    //        {
    //            //call the delegate associated with a selection type
    //            StableReference = SelectionAction(_selectionMessage, this.RevitDynamoModel.Logger, selectionType);
    //            RaisePropertyChanged("SelectionText");

    //            RequiresRecalc = true;
    //        }
    //        catch (OperationCanceledException)
    //        {
    //            CanSelect = true;
    //        }
    //        catch (Exception e)
    //        {
    //            Workspace.DynamoModel.Logger.Log(e);
    //        }
    //    }

    //    protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
    //    {
    //        if (StableReference != null)
    //        {
    //            XmlElement outEl = xmlDoc.CreateElement("instance");
    //            outEl.SetAttribute("id", StableReference);
    //            nodeElement.AppendChild(outEl);
    //        }
    //    }

    //    protected override void LoadNode(XmlNode nodeElement)
    //    {
    //        foreach (XmlNode subNode in nodeElement.ChildNodes)
    //        {
    //            if (subNode.Name.Equals("instance"))
    //            {
    //                Reference saved = null;
    //                var id = subNode.Attributes[0].Value;
    //                try
    //                {
    //                    saved = Reference.ParseFromStableRepresentation(
    //                        DocumentManager.Instance.CurrentDBDocument, id);
    //                }
    //                catch
    //                {
    //                    RevitDynamoModel.Logger.Log(
    //                        "Unable to find reference with stable id: " + id);
    //                }
    //                StableReference = saved != null ? id : string.Empty;
    //            }
    //        }
    //    }

    //    #endregion
    //}

    [NodeName("Select Analysis Results")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select analysis results from the document.")]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
    public class DSAnalysisResultSelection : ElementSelection
    {
        public DSAnalysisResultSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestSelection, SelectionType.Element, "Select an analysis result.")
        { }
    }

    [NodeName("Select Model Element")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a model element from the document.")]
    [IsDesignScriptCompatible]
    public class DSModelElementSelection : ElementSelection
    {
        public DSModelElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestSelection, SelectionType.Element, "Select Model Element")
        { }
    }
    
    [NodeName("Select Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a face.")]
    [IsDesignScriptCompatible]
    public class DSFaceSelection : ElementSelection
    {
        public override string SelectionText
        {
            get
            {
                return _selectionText = StableReference == null
                                            ? "Nothing Selected"
                                            : "Face of Element ID: " + Reference.ParseFromStableRepresentation(DocumentManager.Instance.CurrentDBDocument, StableReference).ElementId;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSFaceSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestSelection, SelectionType.Face, "Select a face.") { }
    }

    [NodeName("Select Edge")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select an edge.")]
    [IsDesignScriptCompatible]
    public class DSEdgeSelection : ElementSelection
    {
        public override string SelectionText
        {
            get
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;

                return _selectionText = StableReference == null
                                            ? "Nothing Selected"
                                            : "Edge of Element ID: " + Reference.ParseFromStableRepresentation(doc, stableReference).ElementId;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSEdgeSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestSelection, SelectionType.Edge, "Select an edge.")
        { }
    }

    [NodeName("Select Point on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a point on a face.")]
    [IsDesignScriptCompatible]
    public class DSPointOnElementSelection : ElementSelection
    {
        public override string SelectionText
        {
            get
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                return _selectionText = string.IsNullOrEmpty(stableReference)
                                            ? "Nothing Selected"
                                            : "Point on element" + " (" + Reference.ParseFromStableRepresentation(doc,stableReference) + ")";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public override string StableReference
        {
            get { return stableReference; }
            set
            {
                bool dirty = value != null;

                stableReference = value;

                if (dirty)
                {
                    RequiresRecalc = true;
                }

                RaisePropertyChanged("SelectedUniqueIdElement");
            }
        }

        public DSPointOnElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestSelection, SelectionType.PointOnFace, "Select a point on a face.")
        { }

    //    public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
    //    {
    //        var dbDocument = DocumentManager.Instance.CurrentDBDocument;

    //        if (string.IsNullOrEmpty(stableReference) || dbDocument == null)
    //        {
    //            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
    //        }

    //        var pointRef =
    //        Reference.ParseFromStableRepresentation(dbDocument, stableReference);

    //        if (pointRef.GlobalPoint == null)
    //        {
    //            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
    //        }

    //        var pt = pointRef.GlobalPoint;

    //        //this is a selected point on a face
    //        var ptArgs = new List<AssociativeNode>()
    //        {
    //            AstFactory.BuildDoubleNode(pt.X),
    //            AstFactory.BuildDoubleNode(pt.Y),
    //            AstFactory.BuildDoubleNode(pt.Z)
    //        };

    //        AssociativeNode node = AstFactory.BuildFunctionCall
    //        (
    //            "Autodesk.DesignScript.Geometry.Point",
    //            "ByCoordinates",
    //            ptArgs
    //        );
    //        return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
    //    }
    }

    [NodeName("Select UV on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a UV on a face.")]
    [IsDesignScriptCompatible]
    public class DSUVOnElementSelection : ElementSelection
    {
        public override string SelectionText
        {
            get
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                return _selectionText = StableReference == null
                                            ? "Nothing Selected"
                                            : "UV on element" + " (" + Reference.ParseFromStableRepresentation(doc, stableReference) + ")";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSUVOnElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestSelection, SelectionType.PointOnFace, "Select a point on a face.")
        { }

        //public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        //{
        //    GeometryObject geob = null;
        //    string stableRep = string.Empty;
        //    var dbDocument = DocumentManager.Instance.CurrentDBDocument;

        //    if (StableReference == null || dbDocument == null)
        //    {
        //        return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
        //    }

        //    var pointRef = Reference.ParseFromStableRepresentation(dbDocument, stableReference);
        //    if (pointRef.UVPoint == null)
        //    {
        //        return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
        //    }

        //    var pt = pointRef.UVPoint;

        //    //this is a selected point on a face
        //    var ptArgs = new List<AssociativeNode>()
        //    {
        //        AstFactory.BuildDoubleNode(pt.U),
        //        AstFactory.BuildDoubleNode(pt.V)
        //    };

        //    AssociativeNode node = AstFactory.BuildFunctionCall
        //    (
        //        "Autodesk.DesignScript.Geometry.UV",
        //        "ByCoordinates",
        //        ptArgs
        //    );
        //    return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        //}
    }

    [NodeName("Select Divided Surface Families")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a divided surface and get its family instances.")]
    [IsDesignScriptCompatible]
    public class DSDividedSurfaceFamiliesSelection : ElementSelection
    {
        public DSDividedSurfaceFamiliesSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, 
            SelectionHelper.RequestDividedSurfaceFamilyInstancesSelection, 
            "Select a divided surface.") { }
    }

    [NodeName("Select Model Elements")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select multiple elements from the Revit document.")]
    [IsDesignScriptCompatible]
    public class DSModelElementsSelection : ElementSelection
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
                    (Selection != null && Selection.Count > 0)
                        ? "Element IDs:" + formatSelectionText(Selection.Where(x => x != null).Select(x => x.ToString()))
                        : "Nothing Selected";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSModelElementsSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.RequestSelection, SelectionType.MultipleElements, "Select elements.") { }
    }

    internal class SelectionButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "Select";

            if (value.GetType() == typeof(List<Element>))
            {
                var els = (List<Element>)value;
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
}
