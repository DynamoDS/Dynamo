using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Autodesk.Revit.DB;
using DSCoreNodes;
using DSRevitNodes.Interactivity;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Nodes;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using Binding = System.Windows.Data.Binding;

namespace DSRevitNodes.Elements
{
    public abstract class DSElementSelectionBase<T> : NodeWithUI 
    {
        private T _selected;
        private bool _canSelect;
        private string _selectionText;
         
        /// <summary>
        /// The message to display in the interface during selection.
        /// </summary>
        protected string _selectionMessage;

        /// <summary>
        /// The selection action.
        /// </summary>
        protected Func<string, T> _selectionAction;

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

                return _selectionText;
            }
            set
            {
                _selectionText = value;
                RaisePropertyChanged("SelectionText");
            }
        }
        
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var selectButton = new dynNodeButton
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
        
        public override Node BuildAst()
        {
            FunctionCallNode node = null;

            if (SelectedElement is Element)
            {
                node = new FunctionCallNode
                {
                    Function = new IdentifierNode("DSRevitNodes.Elements.DSElementFactory.ByElementId"),
                    FormalArguments = new List<AssociativeNode>
                    {
                        new IntNode((SelectedElement as Element).Id.IntegerValue.ToString(CultureInfo.InvariantCulture))
                    }
                };
            }
            else if (SelectedElement is Reference)
            {
                node = new FunctionCallNode
                {
                    Function = new IdentifierNode("DSRevitNodes.Elements.DSElementFactory.ByElementId"),
                    FormalArguments = new List<AssociativeNode>
                    {
                        new IntNode(
                            (SelectedElement as Reference).ElementId.IntegerValue.ToString(CultureInfo.InvariantCulture))
                    }
                };
            }

            return node;
        }

        #endregion
    }

    public class DSElementSelection<T> : DSElementSelectionBase<T>
    {

        #region internal constructors

        internal DSElementSelection(Func<string, T> action, string message)
        {
            _selectionAction = action;
            _selectionMessage = message;
        }
        
        #endregion

        #region public static constructors

        public static DSElementSelection<Element> SelectAnalysisResults()
        {
            return new DSElementSelection<Element>(SelectionHelper.RequestAnalysisResultInstanceSelection, "Select an analysis result.");
        }

        public static DSElementSelection<Element> SelectElement()
        {
            return new DSElementSelection<Element>(SelectionHelper.RequestModelElementSelection, "Select Model Element");
        }

        public static DSElementSelection<FamilyInstance> SelectFamilyInstance()
        {
            return new DSElementSelection<FamilyInstance>(SelectionHelper.RequestFamilyInstanceSelection, "Select a family instance.");
        }

        public static DSElementSelection<Form> SelectDividedSurfaceFamilies()
        {
            return new DSElementSelection<Form>(SelectionHelper.RequestFormSelection, "Select a divided surface.");
        }

        public static DSElementSelection<Reference> SelectFace()
        {
            return new DSElementSelection<Reference>(SelectionHelper.RequestFaceReferenceSelection, "Select a face.");
        }

        public static DSElementSelection<Reference> SelectEdge()
        {
            return new DSElementSelection<Reference>(SelectionHelper.RequestEdgeReferenceSelection, "Select an edge.");
        }

        public static DSElementSelection<CurveElement> SelectCurve()
        {
            return new DSElementSelection<CurveElement>(SelectionHelper.RequestCurveElementSelection, "Select a curve.");
        }

        public static DSElementSelection<ReferencePoint> SelectReferencePoint()
        {
            return new DSElementSelection<ReferencePoint>(SelectionHelper.RequestReferencePointSelection,
                "Select a reference point.");
        }

        public static DSElementSelection<Level> SelectLevel()
        {
            return new DSElementSelection<Level>(SelectionHelper.RequestLevelSelection,
                "Select a level.");
        }

        public static DSElementSelection<Reference> SelectPointOnElement()
        {
            return new DSElementSelection<Reference>(SelectionHelper.RequestReferenceXYZSelection, "Select a point on a face.");
        }

        #endregion

        /// <summary>
        /// Callback when selection button is clicked. 
        /// Calls the selection action, and wraps the result in an AbstractElement
        /// </summary>
        protected override void OnSelectClick()
        {
            try
            {
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
    }

    public class DSMultiSelect : NodeWithUI
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            throw new System.NotImplementedException();
        }

        public override Node BuildAst()
        {
            throw new System.NotImplementedException();
        }
    }
}
