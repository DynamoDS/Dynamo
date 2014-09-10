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

using DSRevitNodesUI;

using Dynamo.Applications.Models;
using Dynamo.Interfaces;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.UI.Commands;

using ProtoCore.AST.AssociativeAST;

using Revit.Elements;
using Revit.GeometryObjects;
using Revit.Interactivity;
using RevitServices.Persistence;
using Element = Autodesk.Revit.DB.Element;

namespace Dynamo.Nodes
{
    public delegate List<string> ElementsSelectionUpdateDelegate(string message, out object target, SelectionType selectionType, ILogger logger);

    public abstract class ElementSelection : RevitNodeModel, IWpfNode
    {
        protected bool canSelect = true;
        protected string selectionText = "";
        protected string selectionMessage;
        protected object selectionTarget;
        protected ElementsSelectionUpdateDelegate SelectionAction;
        protected Func<object, IEnumerable<string>> Update;
        protected List<string> selection = new List<string>();

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

        public virtual string SelectionText
        {
            get
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                var ids = Selection.Select(doc.GetElement).Where(el=>el != null).Select(el => el.Id).ToArray();

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
                selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DelegateCommand SelectCommand { get; set; }

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
                SelectCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region protected constructors

        protected ElementSelection(WorkspaceModel workspaceModel, ElementsSelectionUpdateDelegate action, SelectionType selectionType, string message) 
            : base(workspaceModel)
        {
            SelectionAction = action;
            selectionMessage = message;
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

            SelectCommand = new DelegateCommand(Select, CanBeginSelect);
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

                if (Update != null && selectionTarget != null)
                {
                    Selection.Clear();
                    Selection = Update.Invoke(selectionTarget).ToList();
                }
            }
        }

        #endregion

        #region public methods

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            ////add a button to the inputGrid on the dynElement
            //var selectButton = new DynamoNodeButton
            //{
            //    HorizontalAlignment = HorizontalAlignment.Stretch,
            //    VerticalAlignment = VerticalAlignment.Top,
            //    Height = Configurations.PortHeightInPixels,
            //};
            //selectButton.Click += selectButton_Click;

            //var tb = new TextBox
            //{
            //    HorizontalAlignment = HorizontalAlignment.Stretch,
            //    VerticalAlignment = VerticalAlignment.Center,
            //    Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0)),
            //    BorderThickness = new Thickness(0),
            //    IsReadOnly = true,
            //    IsReadOnlyCaretVisible = false,
            //    MaxWidth = 200,
            //    TextWrapping = TextWrapping.Wrap
            //};

            //nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            //nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            //nodeUI.inputGrid.Children.Add(tb);
            //nodeUI.inputGrid.Children.Add(selectButton);

            //System.Windows.Controls.Grid.SetRow(selectButton, 0);
            //System.Windows.Controls.Grid.SetRow(tb, 1);

            //tb.DataContext = this;
            //selectButton.DataContext = this;

            //var selectTextBinding = new System.Windows.Data.Binding("SelectionText")
            //{
            //    Mode = BindingMode.TwoWay,
            //};
            //tb.SetBinding(TextBox.TextProperty, selectTextBinding);

            //var buttonTextBinding = new System.Windows.Data.Binding("Selection")
            //{
            //    Mode = BindingMode.OneWay,
            //    Converter = new SelectionButtonContentConverter(),
            //};
            //selectButton.SetBinding(ContentControl.ContentProperty, buttonTextBinding);

            //var buttonEnabledBinding = new System.Windows.Data.Binding("CanSelect")
            //{
            //    Mode = BindingMode.TwoWay,
            //};
            //selectButton.SetBinding(Button.IsEnabledProperty, buttonEnabledBinding);
            var selectionControl = new ElementSelectionControl { DataContext = this };
            nodeUI.inputGrid.Children.Add(selectionControl);
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
                    new Func<string, bool, Revit.Elements.Element>(ElementSelector.ByUniqueId),
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

        #region private methods

        private bool CanBeginSelect(object parameter)
        {
            return CanSelect;
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
                //call the delegate associated with a selection type
                Selection = SelectionAction(selectionMessage, out selectionTarget, selectionType, RevitDynamoModel.Logger);
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

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }

        #endregion
    }

    public abstract class ReferenceSelection : ElementSelection
    {
        public override string SelectionText
        {
            get
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                var refs = Selection.Select(r => Reference.ParseFromStableRepresentation(doc, r));
                var ids = refs.Select(doc.GetElement).Where(el=>el != null).Select(el => el.Id).ToArray();

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

                return "Objects:" + sb;
            }
            set
            {
                selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        protected ReferenceSelection(WorkspaceModel workspaceModel, 
            ElementsSelectionUpdateDelegate action, 
            SelectionType selectionType, 
            string message) :base(workspaceModel, action, selectionType, message){}

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var newInputs = new List<AssociativeNode>();
            
            AssociativeNode node;

            if (Selection == null || !Selection.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;

                foreach (var stableRef in Selection)
                {
                    var elRef =
                    Reference.ParseFromStableRepresentation(doc, stableRef);

                    // POINT ON FACE
                    var pt = elRef.GlobalPoint;
                    if (pt != null)
                    {
                        //this is a selected point on a face
                        var ptArgs = new List<AssociativeNode>()
                        {
                            AstFactory.BuildDoubleNode(pt.X),
                            AstFactory.BuildDoubleNode(pt.Y),
                            AstFactory.BuildDoubleNode(pt.Z)
                        };

                        var ptNode = AstFactory.BuildFunctionCall
                        (
                            "Autodesk.DesignScript.Geometry.Point",
                            "ByCoordinates",
                            ptArgs
                        );

                        newInputs.Add(ptNode);
                        continue;
                    }

                    // UV POINT ON FACE
                    var uv = elRef.UVPoint;
                    if (uv != null)
                    {
                        //this is a selected point on a face
                        var ptArgs = new List<AssociativeNode>()
                        {
                            AstFactory.BuildDoubleNode(uv.U),
                            AstFactory.BuildDoubleNode(uv.V)
                        };

                        var uvNode = AstFactory.BuildFunctionCall
                        (
                            "Autodesk.DesignScript.Geometry.UV",
                            "ByCoordinates",
                            ptArgs
                        );

                        newInputs.Add(uvNode);
                        continue;
                    }


                    var refNode = AstFactory.BuildFunctionCall(
                        new Func<string, object>(
                            GeometryObjectSelector.ByReferenceStableRepresentation),
                        new List<AssociativeNode>
                        {
                            AstFactory.BuildStringNode(stableRef),
                        }
                        );
                    newInputs.Add(refNode);

                }

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Select Analysis Results")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select analysis results from the document.")]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
    public class DSAnalysisResultSelection : ElementSelection
    {
        public DSAnalysisResultSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.Instance.RequestElementSelection<Element>, SelectionType.Element, "Select an analysis result.")
        { }
    }

    [NodeName("Select Model Element")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a model element from the document.")]
    [IsDesignScriptCompatible]
    public class DSModelElementSelection : ElementSelection
    {
        public DSModelElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.Instance.RequestElementSelection<Element>, SelectionType.Element, "Select Model Element")
        { }
    }
    
    [NodeName("Select Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a face.")]
    [IsDesignScriptCompatible]
    public class DSFaceSelection : ReferenceSelection
    {
        public override string SelectionText
        {
            get
            {
                return selectionText = !Selection.Any()
                                            ? "Nothing Selected"
                                            : "Face of Element ID: " + Reference.ParseFromStableRepresentation(DocumentManager.Instance.CurrentDBDocument, Selection.First()).ElementId;
            }
            set
            {
                selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSFaceSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.Instance.RequestReferenceSelection, SelectionType.Face, "Select a face.") { }
    }

    [NodeName("Select Edge")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select an edge.")]
    [IsDesignScriptCompatible]
    public class DSEdgeSelection : ReferenceSelection
    {
        public override string SelectionText
        {
            get
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;

                return selectionText = !Selection.Any()
                                            ? "Nothing Selected"
                                            : "Edge of Element ID: " + Reference.ParseFromStableRepresentation(doc, Selection.First()).ElementId;
            }
            set
            {
                selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSEdgeSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.Instance.RequestReferenceSelection, SelectionType.Edge, "Select an edge.")
        { }
    }

    [NodeName("Select Point on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a point on a face.")]
    [IsDesignScriptCompatible]
    public class DSPointOnElementSelection : ReferenceSelection
    {
        public override string SelectionText
        {
            get
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                return selectionText = !Selection.Any()
                                            ? "Nothing Selected"
                                            : "Point on element" + " (" + Reference.ParseFromStableRepresentation(doc,Selection.First()) + ")";
            }
            set
            {
                selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        //public override string StableReference
        //{
        //    get { return stableReference; }
        //    set
        //    {
        //        bool dirty = value != null;

        //        stableReference = value;

        //        if (dirty)
        //        {
        //            RequiresRecalc = true;
        //        }

        //        RaisePropertyChanged("SelectedUniqueIdElement");
        //    }
        //}

        public DSPointOnElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.Instance.RequestReferenceSelection, SelectionType.PointOnFace, "Select a point on a face.")
        { }
    }

    [NodeName("Select UV on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a UV on a face.")]
    [IsDesignScriptCompatible]
    public class DSUVOnElementSelection : ReferenceSelection
    {
        public override string SelectionText
        {
            get
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                return selectionText = !Selection.Any()
                                            ? "Nothing Selected"
                                            : "UV on element" + " (" + Reference.ParseFromStableRepresentation(doc, Selection.First()) + ")";
            }
            set
            {
                selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSUVOnElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.Instance.RequestReferenceSelection, SelectionType.PointOnFace, "Select a point on a face.")
        { }
    }

    [NodeName("Select Divided Surface Families")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a divided surface and get its family instances.")]
    [IsDesignScriptCompatible]
    public class DSDividedSurfaceFamiliesSelection : ElementSelection
    {
        public DSDividedSurfaceFamiliesSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, 
            SelectionHelper.Instance.RequestElementSelection<Autodesk.Revit.DB.DividedSurface>, 
            SelectionType.Element, 
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
                return selectionText = 
                    (Selection != null && Selection.Count > 0)
                        ? "Element IDs:" + formatSelectionText(Selection.Where(x => x != null).Select(x => x.ToString()))
                        : "Nothing Selected";
            }
            set
            {
                selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSModelElementsSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, SelectionHelper.Instance.RequestElementSelection<Element>, SelectionType.MultipleElements, "Select elements.") { }
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
