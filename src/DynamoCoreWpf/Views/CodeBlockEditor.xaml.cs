using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Views;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

using ICSharpCode.AvalonEdit.Rendering;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for CodeBlockEditor.xaml
    /// </summary>
    public partial class CodeBlockEditor : UserControl
    {
        private NodeViewModel nodeViewModel;
        private DynamoViewModel dynamoViewModel;
        private CodeBlockNodeModel nodeModel = null;
        private CompletionWindow completionWindow = null;
        private CodeBlockMethodInsightWindow insightWindow = null;

        internal CodeBlockEditor(DynamoViewModel dynamoViewModel)
        {
            this.dynamoViewModel = dynamoViewModel;
            InitializeComponent();
        }

        public CodeBlockEditor()
        {
            InitializeComponent();
        }

        public CodeBlockEditor(NodeViewModel nodeViewModel)
        {
            InitializeComponent();

            this.nodeViewModel = nodeViewModel;
            this.dynamoViewModel = nodeViewModel.DynamoViewModel;
            this.DataContext = nodeViewModel.NodeModel;
            this.nodeModel = nodeViewModel.NodeModel as CodeBlockNodeModel;

            // Register text editing events
            this.InnerTextEditor.TextChanged += InnerTextEditor_TextChanged;
            this.InnerTextEditor.TextArea.LostFocus += TextArea_LostFocus;

            // the code block should not be in focus upon undo/redo actions on node
            if (this.nodeModel.ShouldFocus)
            {
                this.Loaded += (obj, args) => this.InnerTextEditor.TextArea.Focus();
            }

            // Register auto-completion callbacks
            this.InnerTextEditor.TextArea.TextEntering += OnTextAreaTextEntering;
            this.InnerTextEditor.TextArea.TextEntered += OnTextAreaTextEntered;

            InitializeSyntaxHighlighter();
        }

        private IEnumerable<ICompletionData> GetCompletionData(string code, string stringToComplete)
        {
            var engineController = this.dynamoViewModel.Model.EngineController;

            return engineController.CodeCompletionServices.GetCompletionsOnType(code, stringToComplete).
                Select(x => new CodeBlockCompletionData(x));
        }

        internal IEnumerable<ICompletionData> SearchCompletions(string stringToComplete, Guid guid)
        {
            var engineController = this.dynamoViewModel.Model.EngineController;

            return engineController.CodeCompletionServices.SearchCompletions(stringToComplete, guid).
                Select(x => new CodeBlockCompletionData(x));
        }

        internal IEnumerable<CodeBlockInsightItem> GetFunctionSignatures(string code, string functionName, string functionPrefix)
        {
            var engineController = this.dynamoViewModel.Model.EngineController;

            return engineController.CodeCompletionServices.GetFunctionSignatures(code, functionName, functionPrefix).
                Select(x => new CodeBlockInsightItem(x));
        }

        internal string GetDescription()
        {
            return "";
        }

        internal new bool Focus()
        {
            return InternalEditor.Focus();
        }

        #region Generic Properties
        internal TextEditor InternalEditor
        {
            get { return this.InnerTextEditor; }
        }

        public string Code
        {
            get
            {
                // Since this property a one way binding from source (CodeBlockNodeModel) to 
                // target (this class), the getter should never be called
                throw new NotImplementedException();

            }
            set
            {
                this.InnerTextEditor.Text = value;
            }
        }
        #endregion

        #region Dependency Property
        public static readonly DependencyProperty CodeProperty = DependencyProperty.Register("Code", typeof(string),
            typeof(CodeBlockEditor), new PropertyMetadata((obj, args) =>
            {
                var target = (CodeBlockEditor)obj;
                target.Code = (string)args.NewValue;
            })
        );
        #endregion

        #region Syntax highlighting helper methods

        private void InitializeSyntaxHighlighter()
        {
            var stream = GetType().Assembly.GetManifestResourceStream(
                "Dynamo.Wpf.UI.Resources." + Configurations.HighlightingFile);

            this.InnerTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader(stream), HighlightingManager.Instance);

            // Highlighting Digits
            var rules = this.InnerTextEditor.SyntaxHighlighting.MainRuleSet.Rules;

            rules.Add(CodeBlockEditorUtils.CreateDigitRule());
            rules.Add(CreateClassHighlightRule());
            rules.Add(CreateMethodHighlightRule());
        }

        private HighlightingRule CreateClassHighlightRule()
        {
            Color color = (Color)ColorConverter.ConvertFromString("#2E998F");
            var classHighlightRule = new HighlightingRule();
            classHighlightRule.Color = new HighlightingColor()
            {
                Foreground = new CodeBlockEditorUtils.CustomizedBrush(color)
            };

            var engineController = this.dynamoViewModel.Model.EngineController;

            var wordList = engineController.CodeCompletionServices.GetClasses();
            String regex = String.Format(@"\b({0})({0})?\b", String.Join("|", wordList));
            classHighlightRule.Regex = new Regex(regex);

            return classHighlightRule;
        }

        private HighlightingRule CreateMethodHighlightRule()
        {
            Color color = (Color)ColorConverter.ConvertFromString("#417693");
            var methodHighlightRule = new HighlightingRule();
            methodHighlightRule.Color = new HighlightingColor()
            {
                Foreground = new CodeBlockEditorUtils.CustomizedBrush(color)
            };

            var engineController = this.dynamoViewModel.Model.EngineController;

            var wordList = engineController.CodeCompletionServices.GetGlobals();
            String regex = String.Format(@"\b({0})({0})?\b", String.Join("|", wordList));
            methodHighlightRule.Regex = new Regex(regex);

            return methodHighlightRule;
        }

        #endregion

        #region Auto-complete event handlers

        private void OnTextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text.Length > 0 && completionWindow != null)
                {
                    char currentChar = e.Text[0];
                    // If a completion item is highlighted and the user types
                    // any of the following characters, only then is it selected and inserted
                    // and the code completion window closed
                    if (currentChar == '\t' || currentChar == '.' || currentChar == '\n' || currentChar == '\r')
                    {
                        completionWindow.CompletionList.RequestInsertion(e);
                    }
                    else if (!char.IsLetterOrDigit(currentChar) && currentChar != '_')
                    {
                        // In all other cases where what is being typed is not alpha-numeric 
                        // we want to get rid of the completion window 
                        completionWindow.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                this.dynamoViewModel.Model.Logger.Log("Failed to perform code block autocomplete with exception:");
                this.dynamoViewModel.Model.Logger.Log(ex.Message);
                this.dynamoViewModel.Model.Logger.Log(ex.StackTrace);
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
                    if (startPos > 1 && char.IsLetterOrDigit(InternalEditor.Document.GetCharAt(startPos - 2)))
                        return;

                    if (CodeCompletionParser.IsInsideCommentOrString(code, startPos))
                        return;

                    // Autocomplete as you type
                    // complete global methods (builtins), all classes, symbols local to codeblock node
                    string stringToComplete = CodeCompletionParser.GetStringToComplete(code);

                    var completions = this.SearchCompletions(stringToComplete, nodeModel.GUID);

                    if (!completions.Any())
                        return;

                    ShowCompletionWindow(completions, completeWhenTyping: true);
                }
            }
            catch (System.Exception ex)
            {
                this.dynamoViewModel.Model.Logger.Log("Failed to perform code block autocomplete with exception:");
                this.dynamoViewModel.Model.Logger.Log(ex.Message);
                this.dynamoViewModel.Model.Logger.Log(ex.StackTrace);
            }
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
            completionWindow = new CompletionWindow(this.InnerTextEditor.TextArea);
            completionWindow.AllowsTransparency = true;
            completionWindow.SizeToContent = SizeToContent.WidthAndHeight;

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

        private void ShowInsightWindow(IEnumerable<CodeBlockInsightItem> items)
        {
            if (items == null)
                return;

            if (insightWindow != null)
            {
                insightWindow.Close();
            }
            insightWindow = new CodeBlockMethodInsightWindow(this.InnerTextEditor.TextArea);
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

        #endregion

        #region Generic Event Handlers
        /// <summary>
        /// Called when the CBN is committed and the underlying source data 
        /// needs to be updated with the text typed in the CBN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextArea_LostFocus(object sender, RoutedEventArgs e)
        {
            this.InnerTextEditor.TextArea.ClearSelection();

            this.nodeViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.UpdateModelValueCommand(
                    this.nodeViewModel.NodeModel.GUID, "Code", this.InnerTextEditor.Text));
        }

        void InnerTextEditor_TextChanged(object sender, EventArgs e)
        {
            if (WatermarkLabel.Visibility == Visibility.Visible)
                WatermarkLabel.Visibility = Visibility.Collapsed;

        }
        #endregion

        #region Private Helper Methods

        private void OnRequestReturnFocusToSearch()
        {
            dynamoViewModel.ReturnFocusToSearch();
        }

        private void HandleEscape()
        {
            if (completionWindow != null)
            {
                completionWindow.Close();
                return;
            }

            var text = this.InnerTextEditor.Text;
            var cb = DataContext as CodeBlockNodeModel;

            if (cb == null || cb.Code != null && text.Equals(cb.Code))
                OnRequestReturnFocusToSearch();
            else
                this.InnerTextEditor.Text = (DataContext as CodeBlockNodeModel).Code;
        }
        #endregion

        #region Key Press Event Handlers
        /// <summary>
        /// To allow users to remove focus by pressing Shift Enter. Uses two bools (shift / enter)
        /// and sets them when pressed/released
        /// </summary>        
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {

            if (e.KeyboardDevice.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    OnRequestReturnFocusToSearch();
                }
            }
            else if (e.Key == Key.Escape)
            {
                HandleEscape();
            }
        }

        #endregion
    }


}
