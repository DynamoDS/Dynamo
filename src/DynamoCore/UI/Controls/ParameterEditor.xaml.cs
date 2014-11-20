using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
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
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// </summary>
    public partial class ParameterEditor : UserControl
    {
        private NodeViewModel nodeViewModel;
        private DynamoViewModel dynamoViewModel;
        private Symbol nodeModel = null;
        private CompletionWindow completionWindow = null;

        internal ParameterEditor(DynamoViewModel dynamoViewModel)
        {
            this.dynamoViewModel = dynamoViewModel;
            InitializeComponent();
        }

        public ParameterEditor()
        {
            InitializeComponent();
        }

        public ParameterEditor(NodeViewModel nodeViewModel)
        {
            InitializeComponent();

            this.nodeViewModel = nodeViewModel;
            this.dynamoViewModel = nodeViewModel.DynamoViewModel;
            this.DataContext = nodeViewModel.NodeModel;
            this.nodeModel = nodeViewModel.NodeModel as Symbol;

            // Register text editing events
            this.InnerTextEditor.TextArea.LostFocus += TextArea_LostFocus;

            // Register auto-completion callbacks
            this.InnerTextEditor.TextArea.TextEntering += OnTextAreaTextEntering;
            this.InnerTextEditor.TextArea.TextEntered += OnTextAreaTextEntered;

            InitializeSyntaxHighlighter();
        }

        internal IEnumerable<ICompletionData> SearchAllTypes(string stringToComplete, Guid guid)
        {
            var engineController = this.dynamoViewModel.Model.EngineController;

            return engineController.CodeCompletionServices.SearchTypes(stringToComplete, guid).
                Select(x => new CodeBlockCompletionData(x));
        }

        internal string GetDescription()
        {
            return "";
        }

        #region Generic Properties
        internal TextEditor InternalEditor
        {
            get { return this.InnerTextEditor; }
        }

        public string Parameter
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                this.InnerTextEditor.Text = value;
            }
        }
        #endregion

        #region Dependency Property
        public static readonly DependencyProperty ParameterProperty = DependencyProperty.Register("Parameter", typeof(string),
            typeof(ParameterEditor), new PropertyMetadata((obj, args) =>
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
                "Dynamo.UI.Resources." + Configurations.HighlightingFile);

            this.InnerTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader(stream), HighlightingManager.Instance);

            // Highlighting Digits
            var rules = this.InnerTextEditor.SyntaxHighlighting.MainRuleSet.Rules;

            rules.Add(CreateClassHighlightRule());
        }

        private HighlightingRule CreateClassHighlightRule()
        {
            Color color = (Color)ColorConverter.ConvertFromString("#2E998F");
            var classHighlightRule = new HighlightingRule();
            classHighlightRule.Color = new HighlightingColor()
            {
                Foreground = new CustomizedBrush(color)
            };

            var engineController = this.dynamoViewModel.Model.EngineController;

            var wordList = engineController.CodeCompletionServices.GetClasses();
            String regex = String.Format(@"\b({0})({0})?\b", String.Join("|", wordList));
            classHighlightRule.Regex = new Regex(regex);

            return classHighlightRule;
        }

        #endregion

        #region Auto-complete event handlers

        private void OnTextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text.Length == 0)
                {
                    return;
                }

                char currentChar = e.Text[0];
                if (completionWindow == null)
                {
                    if (currentChar == '\n' || currentChar == '\r')
                    {
                        e.Handled = true;
                    }
                }
                else
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
                string input = this.InnerTextEditor.Text;
                bool hasInputType = input.Contains(':');

                string type = input.Split(':').Last();
                type = type.Trim();
                
                if (hasInputType)
                {
                    var types = this.SearchAllTypes(type, nodeModel.GUID);
                    if (types != null && types.Any())
                    {
                        ShowCompletionWindow(types, type.Length);
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

        private void ShowCompletionWindow(IEnumerable<ICompletionData> completions, int replaceLength)
        {
            // This implementation has been referenced from
            // http://www.codeproject.com/Articles/42490/Using-AvalonEdit-WPF-Text-Editor
            if (completionWindow != null)
            {
                completionWindow.Close();
            }

            completionWindow = new CompletionWindow(this.InnerTextEditor.TextArea);
            completionWindow.AllowsTransparency = true;
            completionWindow.SizeToContent = SizeToContent.WidthAndHeight;
            completionWindow.StartOffset -= replaceLength;
            completionWindow.CloseWhenCaretAtBeginning = true;

            var data = completionWindow.CompletionList.CompletionData;
            foreach (var completion in completions)
            {
                data.Add(completion);
            }

            completionWindow.Closed += delegate
            {
                completionWindow = null;
            };

            completionWindow.Show();
        }

        #endregion

        #region Generic Event Handlers
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextArea_LostFocus(object sender, RoutedEventArgs e)
        {
            this.InnerTextEditor.TextArea.ClearSelection();
            this.nodeViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.UpdateModelValueCommand(
                    this.nodeViewModel.NodeModel.GUID, "InputSymbol", this.InnerTextEditor.Text));
            
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
            var input = DataContext as Symbol;
            if (input == null || input.InputSymbol != null && text.Equals(input.InputSymbol))
            {
                OnRequestReturnFocusToSearch();
            }
            else
            {
                this.InnerTextEditor.Text = (DataContext as Symbol).InputSymbol;
            }
        }
        #endregion

        #region Key Press Event Handlers
        /// <summary>
        /// To allow users to remove focus by pressing Shift Enter. Uses two bools (shift / enter)
        /// and sets them when pressed/released
        /// </summary>        
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
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
