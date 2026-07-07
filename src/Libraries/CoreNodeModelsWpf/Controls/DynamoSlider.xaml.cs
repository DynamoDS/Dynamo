using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.UI;
using Dynamo.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using CoreNodeModels.Input;

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

            // DynamoSlider is shared with DoubleSlider, which still needs the decimal point,
            // so the integer-only keystroke/paste filter is only applied for integer sliders.
            if (nodeModel is IntegerSlider64Bit)
            {
                RestrictToIntegerInput(MinTb);
                RestrictToIntegerInput(MaxTb);
                RestrictToIntegerInput(StepTb);
                RestrictToIntegerInput(ValTb);
            }
        }

        private static readonly Regex IntegerInputPattern = new Regex(@"^-?\d*$", RegexOptions.Compiled);
        private void RestrictToIntegerInput(TextBox textBox)
        {
            textBox.PreviewTextInput += IntegerTextBox_PreviewTextInput;
            DataObject.AddPastingHandler(textBox, IntegerTextBox_Pasting);
        }

        private void IntegerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            e.Handled = !IsValidIntegerText(GetProposedText(textBox, e.Text));
        }

        private void IntegerTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || !e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            var pastedText = (string)e.DataObject.GetData(typeof(string));
            if (!IsValidIntegerText(GetProposedText(textBox, pastedText)))
            {
                e.CancelCommand();
            }
        }

        private static string GetProposedText(TextBox textBox, string newText)
        {
            var text = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            return text.Insert(textBox.SelectionStart, newText);
        }

        private static bool IsValidIntegerText(string text)
        {
            return IntegerInputPattern.IsMatch(text);
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