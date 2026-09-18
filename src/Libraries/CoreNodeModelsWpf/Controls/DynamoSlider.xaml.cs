using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Nodes;
using Dynamo.UI;
using Dynamo.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for DynamoSlider.xaml
    /// </summary>
    public partial class DynamoSlider : UserControl
    {
        readonly NodeModel nodeModel;
        private IViewModelView<NodeViewModel> ui;

        public DynamoSlider(NodeModel model, IViewModelView<NodeViewModel> nodeUI)
        {
            InitializeComponent();
            this.slider.IsMoveToPointEnabled = true;            
            nodeModel = model;
            ui = nodeUI;

            slider.PreviewMouseUp += delegate
            {
                nodeUI.ViewModel.DynamoViewModel.OnRequestReturnFocusToView();
            };

        }

        /// <summary>
        /// Installs the Text bindings for the value, min, max and step text boxes.
        /// </summary>
        /// <param name="ruleFactory">
        /// Optional factory, invoked once per text box. It must return a <b>new</b> rule instance on
        /// every call - a shared instance would let one field's validation state leak into another.
        /// Pass <c>null</c> to bind without validation.
        /// </param>
        public void BindValidatedTextBoxes(Func<ValidationRule> ruleFactory = null)
        {
            BindField(ValTb, "ValueText", ruleFactory?.Invoke());
            BindField(MinTb, "MinText", ruleFactory?.Invoke());
            BindField(MaxTb, "MaxText", ruleFactory?.Invoke());
            BindField(StepTb, "StepText", ruleFactory?.Invoke());
        }

        private static void BindField(DynamoTextBox textBox, string propertyName, ValidationRule validationRule)
        {
            var binding = new Binding(propertyName)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
                NotifyOnValidationError = false
            };

            if (validationRule != null)
            {
                validationRule.ValidationStep = ValidationStep.RawProposedValue;
                binding.ValidationRules.Add(validationRule);
            }

            textBox.BindToProperty(binding);
            Validation.SetErrorTemplate(textBox, null);
        }

        #region Event Handlers

        private void Slider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            var undoRecorder = ui.ViewModel.WorkspaceViewModel.Model.UndoRecorder;
            WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void Slider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            nodeModel.MarkNodeAsModified(true);           
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var undoRecorder = ui.ViewModel.WorkspaceViewModel.Model.UndoRecorder;
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.OriginalSource is Rectangle)
                WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void Slider_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ui.ViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            }
        }

        #endregion
    }
}