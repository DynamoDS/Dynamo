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

        public void BindValidatedTextBoxes(Func<ValidationRule> ruleFactory)
        {
            BindField(ValTb, "ValueText", ruleFactory());
            BindField(MinTb, "MinText", ruleFactory());
            BindField(MaxTb, "MaxText", ruleFactory());
            BindField(StepTb, "StepText", ruleFactory());
        }

        private static ValidationRule CloneStep(ValidationRule template)
        {
            template.ValidationStep = ValidationStep.RawProposedValue;
            return template;
        }

        private static void BindField(DynamoTextBox textBox, string propertyName, ValidationRule validationRule)
        {
            var binding = new Binding(propertyName)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
                NotifyOnValidationError = false
            };

            validationRule.ValidationStep = ValidationStep.RawProposedValue;
            binding.ValidationRules.Add(validationRule);

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