using Dynamo.Graph.Nodes;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Views;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.UI.Controls
{
    // TODO: Merge CodeCompletionEditor with CodeBlockEditor

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
        protected DynamoViewModel dynamoViewModel;
        private CompletionWindow completionWindow;
        private CodeCompletionMethodInsightWindow insightWindow;

        /// <summary>
        /// Create CodeCOmpletionEditor with NodeViewModel
        /// </summary>
        /// <param name="nodeViewModel"></param>
        public CodeCompletionEditor(NodeViewModel nodeViewModel)
        {
            InitializeComponent();

            this.nodeViewModel = nodeViewModel;
            this.dynamoViewModel = nodeViewModel.DynamoViewModel;
            this.DataContext = nodeViewModel.NodeModel;
            this.InnerTextEditor.TextArea.LostFocus += OnTextAreaLostFocus;
            this.InnerTextEditor.TextArea.TextEntering += OnTextAreaTextEntering;
            this.InnerTextEditor.TextArea.TextEntered += OnTextAreaTextEntered;

            CodeHighlightingRuleFactory.CreateHighlightingRules(InnerTextEditor, dynamoViewModel.EngineController);
        }

        /// <summary>
        /// Derived class overrides this function to handle escape event.
        /// </summary>
        protected virtual void OnEscape()
        {
        }

        /// <summary>
        /// Derived class overrides this function to handle the commit of code.
        /// </summary>
        /// <param name="code"></param>
        protected virtual void OnCommitChange(string code)
        {
        }


        private IEnumerable<ICompletionData> GetCompletionData(string code, string stringToComplete)
        {
            var engineController =
                dynamoViewModel.EngineController;

            return engineController.CodeCompletionServices.GetCompletionsOnType(
                code, stringToComplete, dynamoViewModel.CurrentSpace.ElementResolver).
                Select(x => new CodeCompletionData(x));
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

        #region Properties
        public string Code
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                InnerTextEditor.Text = value;
            }
        }

        #endregion

        #region Event handlers

        private void OnTextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0)
            {
                char currentChar = e.Text[0];
                if (completionWindow != null)
                {
                    if (currentChar == '\n' || currentChar == '\r')
                    {
                        completionWindow.CompletionList.RequestInsertion(e);
                    }
                    else if (!char.IsLetterOrDigit(currentChar))
                    {
                        completionWindow.Close();
                    }
                }
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

                    if (!completions.Any())
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
            catch (System.Exception ex)
            {
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextAreaLostFocus(object sender, RoutedEventArgs e)
        {
            InnerTextEditor.TextArea.ClearSelection();

            OnCommitChange(InnerTextEditor.Text);
        }

        /// <summary>
        /// To allow users to remove focus by pressing Shift Enter. Uses two bools (shift / enter)
        /// and sets them when pressed/released
        /// </summary>        
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    dynamoViewModel.ReturnFocusToSearch();
                }
            }
            else if (e.Key == Key.Escape)
            {
                if (completionWindow != null)
                {
                    completionWindow.Close();
                    return;
                }

                OnEscape();
            }
        }
        #endregion

        private void ShowInsightWindow(IEnumerable<CodeCompletionInsightItem> items)
        {
            if (items == null)
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
            if (insightWindow.Items.Count > 0)
            {
                insightWindow.SelectedItem = insightWindow.Items[0];
            }
            else
            {
                // don't open insight window when there are no items
                return;
            }
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
    }
}