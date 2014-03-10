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
using Autodesk.Revit.DB;
using ProtoCore.AST.ImperativeAST;
using Revit.Interactivity;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    public abstract class DSSelectionBase : NodeModel, IWpfNode
    {
        protected bool _canSelect = true;
        protected string _selectionText ="";
        protected string _selectionMessage;
        protected string _selectButtonContent;

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
    }

    public abstract class DSElementSelection : DSSelectionBase 
    {
        private Element _selected;
        protected Func<string, Element> _selectionAction;

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public Element SelectedElement
        {
            get { return _selected; }
            set
            {
                bool dirty;
                if (_selected != null)
                {
                    if (value != null && value.Id.Equals(_selected.Id))
                        return;

                    dirty = true;
                }
                else
                    dirty = value != null;

                _selected = value;

                if (dirty)
                    RequiresRecalc = true;

                RaisePropertyChanged("SelectedElement");
            }
        }

        public override string SelectionText
        {
            get
            {
                return _selected == null
                                       ? "Nothing Selected"
                                       : string.Format("Element Id:{0}",_selected.Id);
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        #region protected constructors

        protected DSElementSelection(Func<string, Element> action, string message)
        {
            _selectionAction = action;
            _selectionMessage = message;

            OutPortData.Add(new PortData("Element", "The selected element.", typeof(object)));
            RegisterAllPorts();
        }
        
        #endregion

        #region public methods

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var selectButton = new DynamoNodeButton()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
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
    
        /// <summary>
        /// Callback when selection button is clicked. 
        /// Calls the selection action, and stores the ElementId(s) of the selected objects.
        /// </summary>
        protected override void OnSelectClick()
        {
            try
            {
                //call the delegate associated with a selection type
                SelectedElement =  _selectionAction(_selectionMessage);
                RaisePropertyChanged("SelectionText");
                RequiresRecalc = true;
            }
            catch (OperationCanceledException)
            {
                CanSelect = true;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log(e);
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // When there's no selection, this returns an invalid ID.
            ElementId selectedElementId = ElementId.InvalidElementId;
            if (SelectedElement != null)
                selectedElementId = SelectedElement.Id;

            var node = AstFactory.BuildFunctionCall(
                "Revit.Elements.ElementSelector",
                "ByElementId",
                new List<AssociativeNode>
                {
                    AstFactory.BuildIntNode(selectedElementId.IntegerValue)
                });

            return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node)};
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            if (SelectedElement != null)
            {
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", SelectedElement.UniqueId);
                nodeElement.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("instance"))
                {
                    Element saved = null;
                    var id = subNode.Attributes[0].Value;
                    try
                    {
                        saved = DocumentManager.Instance.CurrentUIDocument.Document.GetElement(id); // FamilyInstance;
                    }
                    catch
                    {
                        DynamoLogger.Instance.Log(
                            "Unable to find element with ID: " + id);
                    }
                    SelectedElement = saved;
                }
            }
        }
    }

    public abstract class DSReferenceSelection : DSSelectionBase
    {
        protected Reference _selected;
        protected Func<string, Reference> _selectionAction;

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public virtual Reference SelectedElement
        {
            get { return _selected; }
            set
            {
                bool dirty;

                if (_selected != null)
                {
                    if (value != null && value.ElementId.Equals(_selected.ElementId))
                        return;

                    dirty = true;
                }
                else
                    dirty = value != null;

                dirty = value != null;

                _selected = value;

                if (dirty)
                    RequiresRecalc = true;

                RaisePropertyChanged("SelectedElement");
            }
        }

        public override string SelectionText
        {
            get
            {
                return _selected == null
                                        ? "Nothing Selected"
                                        : string.Format("Reference Id: {0}", _selected.ElementId);
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        #region protected constructors

        protected DSReferenceSelection(Func<string, Reference> action, string message)
        {
            _selectionAction = action;
            _selectionMessage = message;

            OutPortData.Add(new PortData("Reference", "The geometry reference.", typeof(object)));
            RegisterAllPorts();
        }

        #endregion

        #region public methods

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var selectButton = new DynamoNodeButton()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
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
                SelectedElement = _selectionAction(_selectionMessage);
                RaisePropertyChanged("SelectionText");
                RequiresRecalc = true;
            }
            catch (OperationCanceledException)
            {
                CanSelect = true;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log(e);
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            GeometryObject geob = null;
            string stableRep = string.Empty;

            AssociativeNode node = null;

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
            }

            
            var args = new List<AssociativeNode>
            {
                AstFactory.BuildStringNode(stableRep)
            };

            if (geob is Curve)
            {
                node = AstFactory.BuildFunctionCall(
                    "GeometryObjectSelector",
                    "ByCurve", 
                    args);
            }
            else
            {
                node = AstFactory.BuildFunctionCall(
                    "GeometryObjectSelector",
                    "ByReferenceStableRepresentation",
                    args);
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
                        DynamoLogger.Instance.Log(
                            "Unable to find reference with stable id: " + id);
                    }
                    SelectedElement = saved;
                }
            }
        }
    }

    public abstract class DSElementsSelection : DSSelectionBase
    {
        private List<Element> _selected;
        protected Func<string, List<Element>> _selectionAction;

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public List<Element> SelectedElement
        {
            get { return _selected; }
            set
            {
                bool dirty = value != null;

                _selected = value;

                if (dirty)
                    RequiresRecalc = true;

                RaisePropertyChanged("SelectedElement");
            }
        }

        public override string SelectionText
        {
            get
            {
                var sb = new StringBuilder();
                int count = 0;
                while (count < Math.Min(_selected.Count, 10))
                {
                    sb.Append(_selected[count].Id.IntegerValue + ",");
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

        protected DSElementsSelection(Func<string, List<Element>> action, string message)
        {
            _selected = new List<Element>();
            _selectionAction = action;
            _selectionMessage = message;

            OutPortData.Add(new PortData("Elements", "The selected elements.", typeof(object)));
            RegisterAllPorts();
        }

        #endregion

        #region public methods

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var selectButton = new DynamoNodeButton()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
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
                SelectedElement = _selectionAction(_selectionMessage);
                RaisePropertyChanged("SelectionText");
                RequiresRecalc = true;
            }
            catch (OperationCanceledException)
            {
                CanSelect = true;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log(e);
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (SelectedElement == null)
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var els = SelectedElement;

                var newInputs = els.Select(el =>
                    AstFactory.BuildFunctionCall(
                    "ElementSelector",
                    "ByElementId",
                    new List<AssociativeNode>
                {
                    AstFactory.BuildIntNode(el.Id.IntegerValue),
                }
                    )).ToList();

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node)};
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            if (SelectedElement != null)
            {
                foreach (Element selectedElement in _selected.Where(x => x != null))
                {
                    XmlElement outEl = xmlDoc.CreateElement("instance");
                    outEl.SetAttribute("id", selectedElement.UniqueId);
                    nodeElement.AppendChild(outEl);
                }
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("instance"))
                {
                    Element saved = null;
                    var id = subNode.Attributes[0].Value;
                    try
                    {
                        saved = DocumentManager.Instance.CurrentUIDocument.Document.GetElement(id);
                    }
                    catch
                    {
                        DynamoLogger.Instance.Log(
                            "Unable to find element with ID: " + id);
                    }
                    if (SelectedElement == null)
                        SelectedElement = new List<Element>();
                    SelectedElement.Add(saved);
                }
            }
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

            return value == null ? "Select" : "Change";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [NodeName("Select Analysis Results")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Select analysis results from the document.")]
    [IsDesignScriptCompatible]
    public class DSAnalysisResultSelection : DSElementSelection
    {
        public DSAnalysisResultSelection()
            : base(SelectionHelper.RequestAnalysisResultInstanceSelection, "Select an analysis result."){}
    }

    [NodeName("Select Model Element")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Select a model element from the document.")]
    [IsDesignScriptCompatible]
    public class DSModelElementSelection : DSElementSelection
    {
        public DSModelElementSelection()
            : base(SelectionHelper.RequestModelElementSelection, "Select Model Element"){}
    }

    [NodeName("Select Family Instance")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Select a family instance from the document.")]
    [IsDesignScriptCompatible]
    public class DSFamilyInstanceSelection : DSElementSelection
    {
        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : SelectedElement.Name;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSFamilyInstanceSelection()
            :base (SelectionHelper.RequestFamilyInstanceSelection, "Select a family instance."){}
    }

    [NodeName("Select Level")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Select a level from the document.")]
    [IsDesignScriptCompatible]
    public class DSLevelSelection : DSElementSelection
    {
        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : SelectedElement.Name + " ("
                                              + SelectedElement.Id + ")";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSLevelSelection()
            :base(SelectionHelper.RequestLevelSelection,"Select a level."){}
    }

    [NodeName("Select Curve Element")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Select a curve element from the document.")]
    [IsDesignScriptCompatible]
    public class DSCurveElementSelection : DSElementSelection
    {
        public override string SelectionText
        {
            get
            {
                return _selectionText = SelectedElement == null
                                            ? "Nothing Selected"
                                            : "Curve ID: " + SelectedElement.Id;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSCurveElementSelection()
            :base(SelectionHelper.RequestCurveElementSelection, "Select a model or reference curve."){}
    }

    [NodeName("Select Reference Point")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Select a reference point from the document.")]
    [IsDesignScriptCompatible]
    public class DSReferencePointSelection : DSElementSelection
    {
        public DSReferencePointSelection()
            :base(SelectionHelper.RequestReferencePointSelection,"Select a reference point."){}
    }

    [NodeName("Select Face")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
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
                                            : "Face ID: " + SelectedElement.ElementId;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSFaceSelection()
            : base(SelectionHelper.RequestFaceReferenceSelection, "Select a face."){}
    }

    [NodeName("Select Edge")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
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
                                            : "Element of Edge  ID: " + SelectedElement.ElementId;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSEdgeSelection()
            : base(SelectionHelper.RequestEdgeReferenceSelection, "Select an edge."){}
    }

    [NodeName("Select Point on Face")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
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
            get { return _selected; }
            set
            {
                bool dirty = value != null;

                _selected = value;

                if (dirty)
                    RequiresRecalc = true;

                RaisePropertyChanged("SelectedElement");
            }
        }
        public DSPointOnElementSelection()
            : base(SelectionHelper.RequestReferenceXYZSelection, "Select a point on a face."){}

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            GeometryObject geob = null;
            string stableRep = string.Empty;
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
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Select a UV on a face.")]
    [IsDesignScriptCompatible]
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

        public DSUVOnElementSelection()
            : base(SelectionHelper.RequestReferenceXYZSelection, "Select a point on a face.") { }

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
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Select a divided surface and get its family instances.")]
    [IsDesignScriptCompatible]
    public class DSDividedSurfaceFamiliesSelection : DSElementsSelection
    {
        public DSDividedSurfaceFamiliesSelection()
            :base(SelectionHelper.RequestDividedSurfaceFamilyInstancesSelection, "Select a divided surface."){}
    }

    [NodeName("Select Multiple Elements")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Select multiple elements from the Revit document.")]
    [IsDesignScriptCompatible]
    public class DSModelElementsSelection : DSElementsSelection
    {
        private static string formatSelectionText(IEnumerable<Element> elements)
        {
            return elements.Any()
                ? System.String.Join(" ", elements.Select(x => x.Id.ToString()))
                : "Nothing Selected";
        }

        public override string SelectionText
        {
            get
            {
                return _selectionText = (SelectedElement != null && SelectedElement.Count > 0)
                                            ? "Element IDs:" + formatSelectionText(SelectedElement.Where(x => x != null && x.Id != null))
                                            : "Nothing Selected";
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }

        public DSModelElementsSelection()
            :base(SelectionHelper.RequestMultipleCurveElementsSelection, "Select elements."){}
    }
}
