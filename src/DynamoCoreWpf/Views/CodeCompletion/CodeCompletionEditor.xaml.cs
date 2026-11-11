using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Views;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for CodeCompletionEditor.xaml
    /// </summary>
    public partial class CodeCompletionEditor : UserControl
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CodeCompletionEditor()
        {
            InitializeComponent();
        }

        protected NodeViewModel nodeViewModel;

        /// <summary>
        /// If the editor has been disposed. 
        /// </summary>
        protected bool IsDisposed
        {
            get; private set;
        }

        private DynamoViewModel dynamoViewModel;
        private CompletionWindow completionWindow;
        private CodeCompletionMethodInsightWindow insightWindow;
        /// <summary>
        /// True when Escape on KeyDown closed the completion window. OnEscape is
        /// deferred to KeyUp so a held Escape cannot ReturnFocus and then start
        /// geometry-preview navigation on DynamoView KeyDown repeats.
        /// </summary>
        private bool escapeClosedCompletion;

        /// <summary>
        /// Create CodeCOmpletionEditor with NodeViewModel
        /// </summary>
        /// <param name="nodeView"></param>
        public CodeCompletionEditor(NodeView nodeView)
        {
            InitializeComponent();

            nodeView.Unloaded += (obj, args) => IsDisposed = true;
            this.nodeViewModel = nodeView.ViewModel;
            this.DataContext = nodeViewModel.NodeModel;
            this.dynamoViewModel = nodeViewModel.DynamoViewModel;
            this.dynamoViewModel.PreferencesViewModel.PropertyChanged += OnPreferencesViewModelPropertyChanged;
            this.InnerTextEditor.TextChanged += OnTextChanged;
            this.InnerTextEditor.TextArea.GotFocus+= OnTextAreaGotFocus; 
            this.InnerTextEditor.TextArea.LostFocus += OnTextAreaLostFocus;
            this.InnerTextEditor.TextArea.TextEntering += OnTextAreaTextEntering;
            this.InnerTextEditor.TextArea.TextEntered += OnTextAreaTextEntered;
            // Handle Escape on the focused TextArea itself (not only UserControl.OnPreviewKeyDown)
            // so DynamoView_KeyDown cannot start geometry-preview navigation on hold.
            this.InnerTextEditor.TextArea.PreviewKeyDown += OnTextAreaPreviewKeyDown;
            this.InnerTextEditor.TextArea.PreviewKeyUp += OnTextAreaPreviewKeyUp;

            CodeHighlightingRuleFactory.CreateHighlightingRules(InnerTextEditor, dynamoViewModel.EngineController);
        }

        /// <summary>
        /// Set focus to the editor.
        /// </summary>
        public void SetFocus()
        {
            InnerTextEditor.Focus();
        }

        #region Dependency Property
        /// <summary>
        /// Dependency property.
        /// </summary>
        public static readonly DependencyProperty CodeProperty =
            DependencyProperty.Register(
                "Code",
                typeof(string),
                typeof(CodeCompletionEditor),
                new PropertyMetadata((obj, args) =>
                {
                    var target = (CodeCompletionEditor)obj;
                    target.Code = (string)args.NewValue;
                })
            );
        #endregion

        #region Properties

        /// <summary>
        /// Set the content of the editor.
        /// </summary>
        public string Code
        {
            get
            {
                // Since this property a one way binding from source (for example, CodeBlockNodeModel)
                // to target (this class), the getter should never be called.
                throw new NotImplementedException();
            }
            set
            {
                InnerTextEditor.Text = value;
            }
        }

        #endregion

        /// <summary>
        /// Derived class overrides this function to handle escape event.
        /// </summary>
        protected virtual void OnEscape()
        {
        }

        /// <summary>
        /// Derived class overrides this function to handle the commit of code.
        /// </summary>
        protected virtual void OnCommitChange()
        {
        }
        
        /// <summary>
        /// Derived class overrides this function to handle the event of getting focus. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnTextAreaGotFocus(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Update the value of specified property of node to the input code.
        /// </summary>
        /// <param name="property"></param>
        protected void UpdateNodeValue(string property)
        {
            dynamoViewModel.ExecuteCommand(
                new Models.DynamoModel.UpdateModelValueCommand(
                    nodeViewModel.WorkspaceViewModel.Model.Guid,
                    nodeViewModel.NodeModel.GUID, 
                    property,
                    InnerTextEditor.Text));
        }

        /// <summary>
        /// Return focus to the Dynamo.
        /// </summary>
        protected void ReturnFocus()
        {
            dynamoViewModel.OnRequestReturnFocusToView();
        }

        private IEnumerable<ICompletionData> GetCompletionData(string code, string stringToComplete)
        {
            var engineController =
                dynamoViewModel.EngineController;

            var completions = engineController.CodeCompletionServices.GetCompletionsOnType(
                code, stringToComplete, dynamoViewModel.CurrentSpace.ElementResolver);
            
            return completions?.Select(x => new CodeCompletionData(x));
        }

        private IEnumerable<ICompletionData> SearchCompletions(string stringToComplete, Guid guid)
        {
            var engineController = dynamoViewModel.EngineController;

            return engineController.CodeCompletionServices.SearchCompletions(stringToComplete, guid,
                dynamoViewModel.CurrentSpace.ElementResolver).Select(x => new CodeCompletionData(x));
        }

        private IEnumerable<CodeCompletionInsightItem> GetFunctionSignatures(string code, string functionName, string functionPrefix)
        {
            var engineController = dynamoViewModel.EngineController;

            return engineController.CodeCompletionServices.GetFunctionSignatures(
                code, functionName, functionPrefix, dynamoViewModel.CurrentSpace.ElementResolver).
                Select(x => new CodeCompletionInsightItem(x));
        }

        #region Event handlers

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (WatermarkLabel.Visibility == Visibility.Visible)
            {
                WatermarkLabel.Visibility = Visibility.Collapsed;
                this.InnerTextEditor.ShowLineNumbers = dynamoViewModel.PreferencesViewModel.ShowCodeBlockLineNumber;
            }
        }

        private void OnPreferencesViewModelPropertyChanged(object sender, EventArgs e)
        {
            if ((e as PropertyChangedEventArgs).PropertyName == nameof(dynamoViewModel.PreferencesViewModel.ShowCodeBlockLineNumber))
            {
                this.InnerTextEditor.ShowLineNumbers = dynamoViewModel.PreferencesViewModel.ShowCodeBlockLineNumber;
            }
        }

        private void OnTextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length == 0 || completionWindow == null)
                return;

            try
            {
                char currentChar = e.Text[0];
                if (currentChar == '\t' || currentChar == '.' || currentChar == '\n' || currentChar == '\r')
                {
                    completionWindow.CompletionList.RequestInsertion(e);
                }
                else if (!char.IsLetterOrDigit(currentChar))
                {
                    completionWindow.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        private void OnTextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            try
            {
                int startPos = this.InnerTextEditor.CaretOffset;
                var code = this.InnerTextEditor.Text.Substring(0, startPos);

                if (e.Text == ".")
                {
                    if (CodeCompletionParser.IsInsideCommentOrString(code, startPos))
                        return;

                    string stringToComplete = CodeCompletionParser.GetStringToComplete(code).Trim('.');

                    var completions = this.GetCompletionData(code, stringToComplete);

                    if (completions == null || !completions.Any())
                        return;

                    ShowCompletionWindow(completions);
                }
                // Complete function signatures
                else if (e.Text == "(")
                {
                    if (CodeCompletionParser.IsInsideCommentOrString(code, startPos))
                        return;

                    string functionName;
                    string functionPrefix;
                    CodeCompletionParser.GetFunctionToComplete(code, out functionName, out functionPrefix);

                    var insightItems = this.GetFunctionSignatures(code, functionName, functionPrefix);

                    ShowInsightWindow(insightItems);
                }
                else if (e.Text == ")")
                {
                    if (insightWindow != null)
                        insightWindow.Close();
                }
                else if (completionWindow == null && (char.IsLetterOrDigit(e.Text[0]) || e.Text[0] == '_'))
                {

                    // Begin completion while typing only if the previous character already typed in
                    // is a white space or non-alphanumeric character
                    if (startPos > 1 && char.IsLetterOrDigit(InnerTextEditor.Document.GetCharAt(startPos - 2)))
                        return;

                    if (CodeCompletionParser.IsInsideCommentOrString(code, startPos))
                        return;

                    // Autocomplete as you type
                    // complete global methods (builtins), all classes, symbols local to codeblock node
                    string stringToComplete = CodeCompletionParser.GetStringToComplete(code);

                    var completions = this.SearchCompletions(stringToComplete, nodeViewModel.NodeModel.GUID);

                    if (!completions.Any())
                        return;

                    ShowCompletionWindow(completions, completeWhenTyping: true);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextAreaLostFocus(object sender, RoutedEventArgs e)
        {
            if (IsDisposed)
                return;

            InnerTextEditor.TextArea.ClearSelection();

            OnCommitChange();
        }

        /// <summary>
        /// To allow users to remove focus by pressing Shift Enter. Uses two bools (shift / enter)
        /// and sets them when pressed/released.
        /// </summary>        
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    ReturnFocus();
                }
            }
        }

        /// <summary>
        /// When text is selected, Left/Right should place the cursor at the
        /// selection start/end like in a normal text editor. AvalonEdit clears the selection
        /// then moves one character from the old cursor, which feels stuck.
        /// Returns the cursor offset to use, or null when the key should pass through.
        /// </summary>
        internal static int? GetSelectionCollapseCaretOffset(
            int selectionStart, int selectionLength, Key key, System.Windows.Input.ModifierKeys modifiers)
        {
            if (modifiers != System.Windows.Input.ModifierKeys.None || selectionLength <= 0)
                return null;

            if (key == Key.Left)
                return selectionStart;
            if (key == Key.Right)
                return selectionStart + selectionLength;

            return null;
        }

        private void OnTextAreaPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var collapseOffset = GetSelectionCollapseCaretOffset(
                InnerTextEditor.SelectionStart,
                InnerTextEditor.SelectionLength,
                e.Key,
                e.KeyboardDevice.Modifiers);
            if (collapseOffset.HasValue)
            {
                InnerTextEditor.TextArea.ClearSelection();
                InnerTextEditor.CaretOffset = collapseOffset.Value;
                InnerTextEditor.TextArea.Caret.BringCaretToView();
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Escape)
                return;

            // Do not call OnEscape on KeyDown: CodeBlockEditor.OnEscape may
            // ReturnFocus immediately, after which Escape KeyDown repeats hit
            // DynamoView and start 3D background navigation.
            if (!e.IsRepeat)
            {
                if (completionWindow != null)
                {
                    completionWindow.Close();
                    escapeClosedCompletion = true;
                }
                else
                {
                    escapeClosedCompletion = false;
                }
            }

            e.Handled = true;
        }

        private void OnTextAreaPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            // Leave the editor on a short press (not while the key is held).
            if (!escapeClosedCompletion)
            {
                OnEscape();
            }

            escapeClosedCompletion = false;
            e.Handled = true;
        }
        #endregion

        private void ShowInsightWindow(IEnumerable<CodeCompletionInsightItem> items)
        {
            if (items == null || !items.Any())
                return;

            if (insightWindow != null)
            {
                insightWindow.Close();
            }

            insightWindow = new CodeCompletionMethodInsightWindow(this.InnerTextEditor.TextArea);
            foreach (var item in items)
            {
                insightWindow.Items.Add(item);
            }

            if (insightWindow.Items.Count <= 0)
                return;

            insightWindow.SelectedItem = insightWindow.Items[0];
            insightWindow.Closed += delegate
            {
                insightWindow = null;
            };
            insightWindow.Show();
        }

        private void ShowCompletionWindow(IEnumerable<ICompletionData> completions, bool completeWhenTyping = false)
        {
            // TODO: Need to make this more efficient by instantiating 'completionWindow'
            // just once and updating its contents each time

            // This implementation has been referenced from
            // http://www.codeproject.com/Articles/42490/Using-AvalonEdit-WPF-Text-Editor
            if (completionWindow != null)
            {
                completionWindow.Close();
            }

            completionWindow = new CompletionWindow(this.InnerTextEditor.TextArea)
            {
                AllowsTransparency = true,
                SizeToContent = SizeToContent.WidthAndHeight
            };

            if (completeWhenTyping)
            {
                // As opposed to complete on '.', in complete while typing mode 
                // the first character typed should also be considered for matches
                // while generating options in completion window
                completionWindow.StartOffset--;

                // As opposed to complete on '.', in complete while typing mode 
                // erasing the first character of the string being completed
                // should close the completion window
                completionWindow.CloseWhenCaretAtBeginning = true;
            }

            var data = completionWindow.CompletionList.CompletionData;

            foreach (var completion in completions)
                data.Add(completion);

            completionWindow.Closed += delegate
            {
                completionWindow = null;
            };

            completionWindow.Show();
        }

        internal void Dispose()
        {
            this.dynamoViewModel.PreferencesViewModel.PropertyChanged -= OnPreferencesViewModelPropertyChanged;
        }
    }
}
