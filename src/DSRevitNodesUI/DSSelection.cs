using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Autodesk.Revit.DB;
using DSRevitNodes.Interactivity;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;
using Binding = System.Windows.Data.Binding;

namespace DSRevitNodes.Elements
{
    public abstract class DSSelectionBase<T> : NodeModel 
    {
        private T _selected;
        private bool _canSelect;
        private string _selectionText;
        
        /// <summary>
        /// The message to display in the interface during selection.
        /// </summary>
        protected string selectionMessage;

        /// <summary>
        /// The selection action.
        /// </summary>
        protected Func<string, T> selectionAction;

        protected Action unwrapAction;
 
        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public T SelectedElement
        {
            get { return _selected; }
            set
            {
                _selected = value;
                RaisePropertyChanged("SelectedElement");
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
        /// The text that describes this selection.
        /// </summary>
        public string SelectionText
        {
            get
            {
                if (SelectedElement == null)
                {
                    return "Nothing selected.";
                }

                if (SelectedElement is Element)
                {
                    return (SelectedElement as Element).Name;
                }
                else if (SelectedElement is Reference)
                {
                    return "Reference ID: " + (SelectedElement as Reference).ElementId;
                }
                else if (SelectedElement is List<Element>)
                {
                    var elements = SelectedElement as List<Element>;
                    var sb = new StringBuilder();
                    elements.ForEach(x=>sb.Append(x.Id.IntegerValue+","));

                    return "Elements:" + sb.ToString();
                }

                return _selectionText;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }
        
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var selectButton = new DynamoNodeButton()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            selectButton.Click += selectButton_Click;

            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.Children.Add(selectButton);
            System.Windows.Controls.Grid.SetRow(selectButton, 0);

            selectButton.DataContext = this;

            var buttonEnabledBinding = new Binding("CanSelect")
            {
                Mode = BindingMode.TwoWay
            };
            selectButton.SetBinding(UIElement.IsEnabledProperty, buttonEnabledBinding);
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
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

        #region public methods

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node = null;

            if (SelectedElement is Element)
            {
                node = new FunctionCallNode
                {
                    Function = new IdentifierNode("DSRevitNodes.Elements.ElementSelector.ByElementId"),
                    FormalArguments = new List<AssociativeNode>
                    {
                        new IntNode((SelectedElement as Element).Id.IntegerValue.ToString(CultureInfo.InvariantCulture))
                    }
                };
            }
            else if (SelectedElement is Reference)
            {
                var geomRef = SelectedElement as Reference;
                var geob =
                        DocumentManager.GetInstance()
                            .CurrentDBDocument.GetElement(geomRef)
                            .GetGeometryObjectFromReference(geomRef);
                var stringNode = new StringNode
                {
                    value = (SelectedElement as Reference).ConvertToStableRepresentation(
                        DocumentManager.GetInstance().CurrentDBDocument)
                };

                if (geob is Curve)
                {
                    node = new FunctionCallNode
                    {
                        Function = new IdentifierNode("DSRevitNodes.GeoemtryObjects.GeometryObjectSelector.ByCurve"),
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
                        Function = new IdentifierNode("DSRevitNodes.GeoemtryObjects.GeometryObjectSelector.ByReferenceId"),
                        FormalArguments = new List<AssociativeNode>
                        { 
                            stringNode
                        }
                    };
                }
            }
            else if (SelectedElement is List<Element>)
            {
                var els = SelectedElement as List<Element>;

                var newInputs = els.Select(el => new FunctionCallNode
                {
                    Function = new IdentifierNode("DSRevitNodes.Elements.ElementSelector.ByElementIds"), 
                    FormalArguments = new List<AssociativeNode>
                    {
                        new IntNode(el.Id.IntegerValue.ToString(CultureInfo.InvariantCulture))
                    }
                }).Cast<AssociativeNode>().ToList();

                node = AstFactory.BuildExprList(newInputs);
            }

            return new [] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        #endregion
    }
    
    //TODO: DSSelection needs to respond to document modification events
    public class DSSelection<T> : DSSelectionBase<T>
    {
        #region internal constructors

        internal DSSelection(Func<string, T> action, string message)
        {
            selectionAction = action;
            selectionMessage = message;
        }
        
        #endregion

        #region public static constructors

        public static DSSelection<Element> SelectAnalysisResults()
        {
            return new DSSelection<Element>(SelectionHelper.RequestAnalysisResultInstanceSelection, "Select an analysis result.");
        }

        public static DSSelection<Element> SelectElement()
        {
            return new DSSelection<Element>(SelectionHelper.RequestModelElementSelection, "Select Model Element");
        }

        public static DSSelection<FamilyInstance> SelectFamilyInstance()
        {
            return new DSSelection<FamilyInstance>(SelectionHelper.RequestFamilyInstanceSelection, "Select a family instance.");
        }

        public static DSSelection<List<FamilyInstance>> SelectDividedSurfaceFamilies()
        {
            return new DSSelection<List<FamilyInstance>>(SelectionHelper.RequestDividedSurfaceFamilyInstancesSelection, "Select a divided surface.");
        }

        public static DSSelection<Reference> SelectFace()
        {
            return new DSSelection<Reference>(SelectionHelper.RequestFaceReferenceSelection, "Select a face.");
        }

        public static DSSelection<Reference> SelectEdge()
        {
            return new DSSelection<Reference>(SelectionHelper.RequestEdgeReferenceSelection, "Select an edge.");
        }

        public static DSSelection<CurveElement> SelectCurve()
        {
            return new DSSelection<CurveElement>(SelectionHelper.RequestCurveElementSelection, "Select a curve.");
        }

        public static DSSelection<ReferencePoint> SelectReferencePoint()
        {
            return new DSSelection<ReferencePoint>(SelectionHelper.RequestReferencePointSelection,
                "Select a reference point.");
        }

        public static DSSelection<Level> SelectLevel()
        {
            return new DSSelection<Level>(SelectionHelper.RequestLevelSelection,
                "Select a level.");
        }

        public static DSSelection<Reference> SelectPointOnElement()
        {
            return new DSSelection<Reference>(SelectionHelper.RequestReferenceXYZSelection, "Select a point on a face.");
        }

        public static DSSelection<List<Element>> SelectMultipleElements()
        {
            return new DSSelection<List<Element>>(SelectionHelper.RequestMultipleCurveElementsSelection, "Select elements.");
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
                SelectedElement =  selectionAction(selectionMessage);
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
    }
}
