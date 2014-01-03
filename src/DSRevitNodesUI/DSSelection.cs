using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Autodesk.Revit.DB;
using DSRevitNodes.Interactivity;
using Dynamo.Controls;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    public abstract class DSSelectionBase : NodeModel 
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
                if (value != null)
                {
                    SelectButtonContent = "Change";
                }
                else
                {
                    SelectionText = "Nothing Selected";
                    SelectButtonContent = "Select";
                }

                if (dirty)
                    RequiresRecalc = true;
            }
        }

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

        public void SetupCustomUIElements(dynNodeView nodeUI)
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

            if (SelectedElement == null || !SelectionText.Any() || !SelectButtonContent.Any())
            {
                SelectionText = "Nothing Selected";
                SelectButtonContent = "Select Element";
            }

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

            var buttonTextBinding = new System.Windows.Data.Binding("SelectButtonContent")
            {
                Mode = BindingMode.TwoWay,
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
            AssociativeNode node = null;

            node = new FunctionCallNode
            {
                Function = new IdentifierNode("DSRevitNodes.Elements.ElementSelector.ByElementId"),
                FormalArguments = new List<AssociativeNode>
                {
                    new IntNode((SelectedElement as Element).Id.IntegerValue.ToString(CultureInfo.InvariantCulture))
                }
            };

            return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node)};
        }
    }

    public abstract class DSReferenceSelection : DSSelectionBase
    {
        private Reference _selected;
        protected Func<string, Reference> _selectionAction;

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public Reference SelectedElement
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

                _selected = value;
                if (value != null)
                {
                    SelectButtonContent = "Change";
                }
                else
                {
                    SelectionText = "Nothing Selected";
                    SelectButtonContent = "Select";
                }

                if (dirty)
                    RequiresRecalc = true;
            }
        }

        public override string SelectionText
        {
            get
            {
                return "Reference ID: " + (SelectedElement as Reference).ElementId;
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

        public void SetupCustomUIElements(dynNodeView nodeUI)
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

            if (SelectedElement == null || !SelectionText.Any() || !SelectButtonContent.Any())
            {
                SelectionText = "Nothing Selected";
                SelectButtonContent = "Select Element";
            }

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

            var buttonTextBinding = new System.Windows.Data.Binding("SelectButtonContent")
            {
                Mode = BindingMode.TwoWay,
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
            AssociativeNode node = null;

            var geomRef = SelectedElement as Reference;
            var geob =
                    DocumentManager.GetInstance()
                        .CurrentDBDocument.GetElement(geomRef)
                        .GetGeometryObjectFromReference(geomRef);
            var stringNode = new StringNode
            {
                value = SelectedElement.ConvertToStableRepresentation(
                    DocumentManager.GetInstance().CurrentDBDocument)
            };

            if (geob is Curve)
            {
                node = new FunctionCallNode
                {
                    Function = new IdentifierNode("DSRevitNodes.GeometryObjects.GeometryObjectSelector.ByCurve"),
                    FormalArguments = new List<AssociativeNode>
                    { 
                        stringNode
                    }
                };
            }
            else
            {
                    
                node = new FunctionCallNode
                {
                    Function = new IdentifierNode("DSRevitNodes.GeometryObjects.GeometryObjectSelector.ByReferenceId"),
                    FormalArguments = new List<AssociativeNode>
                    { 
                        stringNode
                    }
                };
            }

            return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node)};
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
                if (value != null)
                {
                    SelectButtonContent = "Change";
                }
                else
                {
                    SelectionText = "Nothing Selected";
                    SelectButtonContent = "Select";
                }

                if (dirty)
                    RequiresRecalc = true;
            }
        }

        public override string SelectionText
        {
            get
            {
                var elements = SelectedElement as List<Element>;
                var sb = new StringBuilder();
                elements.ForEach(x => sb.Append(x.Id.IntegerValue + ","));

                return "Elements:" + sb.ToString();
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

        public void SetupCustomUIElements(dynNodeView nodeUI)
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

            if (SelectedElement.Count == 0 || !SelectionText.Any() || !SelectButtonContent.Any())
            {
                SelectionText = "Nothing Selected";
                SelectButtonContent = "Select Elements";
            }

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

            var buttonTextBinding = new System.Windows.Data.Binding("SelectButtonContent")
            {
                Mode = BindingMode.TwoWay,
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
            AssociativeNode node = null;

            var els = SelectedElement as List<Element>;

            var newInputs = els.Select(el => new FunctionCallNode
            {
                Function = new IdentifierNode("DSRevitNodes.Elements.ElementSelector.ByElementId"),
                FormalArguments = new List<AssociativeNode>
                {
                    new IntNode(el.Id.IntegerValue.ToString(CultureInfo.InvariantCulture))
                }
            }).Cast<AssociativeNode>().ToList();

            node = AstFactory.BuildExprList(newInputs);

            return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node)};
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

        public DSPointOnElementSelection()
            : base(SelectionHelper.RequestReferenceXYZSelection, "Select a point on a face."){}
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
