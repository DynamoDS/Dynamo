using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Dynamo.Configuration;
using DynCmd = Dynamo.Models.DynamoModel;
using Dynamo.Wpf.Views;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Auto-completion control for input parameter.
    /// </summary>
    public partial class ParameterEditor : UserControl
    {
        private NodeViewModel nodeViewModel;
        private DynamoViewModel dynamoViewModel;
        private CompletionWindow completionWindow;
        private CodeCompletionMethodInsightWindow insightWindow;

        public ParameterEditor(NodeViewModel nodeViewModel)
        {
            InitializeComponent();

            this.nodeViewModel = nodeViewModel;
            this.dynamoViewModel = nodeViewModel.DynamoViewModel;
            this.DataContext = nodeViewModel.NodeModel;
            this.InnerTextEditor.TextArea.LostFocus += OnTextAreaLostFocus;
            this.InnerTextEditor.TextArea.TextEntering += OnTextAreaTextEntering;
            this.InnerTextEditor.TextArea.TextEntered += OnTextAreaTextEntered;

            InitializeSyntaxHighlighter();
        }

        private IEnumerable<ICompletionData> GetCompletionData(string code, string stringToComplete)
        {
            var engineController =
                dynamoViewModel.EngineController;

            return engineController.CodeCompletionServices.GetCompletionsOnType(
                code, stringToComplete, dynamoViewModel.CurrentSpace.ElementResolver).
                Select(x => new CodeCompletionData(x));
        }

        internal IEnumerable<ICompletionData> SearchCompletions(string stringToComplete, Guid guid)
        {
            var engineController = dynamoViewModel.EngineController;

            return engineController.CodeCompletionServices.SearchCompletions(stringToComplete, guid,
                dynamoViewModel.CurrentSpace.ElementResolver).Select(x => new CodeCompletionData(x));
        }

        internal IEnumerable<CodeCompletionInsightItem> GetFunctionSignatures(string code, string functionName, string functionPrefix)
        {
            var engineController = dynamoViewModel.EngineController;

            return engineController.CodeCompletionServices.GetFunctionSignatures(
                code, functionName, functionPrefix, dynamoViewModel.CurrentSpace.ElementResolver).
                Select(x => new CodeCompletionInsightItem(x));
        }

        #region Properties

        public string Parameter
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

        #region Dependency Property
        public static readonly DependencyProperty ParameterProperty = 
            DependencyProperty.Register(
                "Parameter", 
                typeof(string),
                typeof(ParameterEditor), 
                new PropertyMetadata((obj, args) =>
                {
                    var target = (ParameterEditor)obj;
                    target.Parameter = (string)args.NewValue;
                })
            );
        #endregion

        #region Syntax highlighting helper methods

        private void InitializeSyntaxHighlighter()
        {
            var stream = GetType().Assembly.GetManifestResourceStream(
                "Dynamo.Wpf.UI.Resources." + Configurations.HighlightingFile);

            InnerTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader(stream), HighlightingManager.Instance);

            // Highlighting Digits
            var rules = InnerTextEditor.SyntaxHighlighting.MainRuleSet.Rules;

            rules.Add(CodeHighlightingRuleFactory.CreateNumberHighlightingRule());
            rules.Add(CodeHighlightingRuleFactory.CreateClassHighlightRule(dynamoViewModel.EngineController));
            rules.Add(CodeHighlightingRuleFactory.CreateMethodHighlightRule(dynamoViewModel.EngineController));
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

            var lastInput = (nodeViewModel.NodeModel as Symbol).InputSymbol;
            if (lastInput.Equals(InnerTextEditor.Text))
                return;

            nodeViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.UpdateModelValueCommand(
                    nodeViewModel.WorkspaceViewModel.Model.Guid,
                    nodeViewModel.NodeModel.GUID, "InputSymbol",
                    InnerTextEditor.Text));
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

                var text = this.InnerTextEditor.Text;
                var input = DataContext as Symbol;
                if (input == null || input.InputSymbol != null && text.Equals(input.InputSymbol))
                {
                    dynamoViewModel.ReturnFocusToSearch();
                }
                else
                {
                    this.InnerTextEditor.Text = (DataContext as Symbol).InputSymbol;
                }
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
            };nodeModel

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
