using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Autodesk.Revit.UI.Selection;
using DSCoreNodes;
using Dynamo.Controls;
using Dynamo.Utilities;
using GraphToDSCompiler;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Threading;
using Autodesk.Revit.DB;
using Binding = System.Windows.Data.Binding;
using Grid = System.Windows.Controls.Grid;
using Node = ProtoCore.AST.Node;
using RevThread = RevitServices.Threading;

namespace Dynamo.Nodes
{
    public class SelectElement : NodeWithUI
    {
        private ElementId _selected;
        private bool _canSelect;

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public virtual ElementId SelectedElement
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
            Grid.SetRow(selectButton, 0);

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
            RevThread.IdlePromise.ExecuteOnIdleAsync(
                delegate
                {
                    OnSelectClick();
                    CanSelect = true; //...and re-enable it once selection has finished.
                });
        }

        /// <summary>
        /// Override this to perform custom selection logic.
        /// </summary>
        protected virtual void OnSelectClick()
        {
            var doc = dynRevitSettings.Doc;

            var choices = doc.Selection;
            choices.Elements.Clear();

            var eleRef = doc.Selection.PickObject(ObjectType.Element);
            if (eleRef != null)
            {
                SelectedElement = eleRef.ElementId;
            }
        }

        public override Node BuildAst()
        {
            return new FunctionCallNode
            {
                Function = new IdentifierNode("DSRevitNodes.Elements.DSElementFactory.ByElementId"),
                FormalArguments = new List<AssociativeNode>
                {
                    new IntNode(SelectedElement.IntegerValue.ToString())
                }
            };
        }
    }
}
