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
    /// Auto-completion control for input parameter.
    /// </summary>
    public partial class ParameterEditor : UserControl
    {
        private NodeViewModel nodeViewModel;
        private DynamoViewModel dynamoViewModel;
        private CompletionWindow completionWindow;

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

        /// <summary>
        /// Get all types that matched with partial name
        /// </summary>
        /// <param name="partialName"></param>
        /// <returns></returns>
        private IEnumerable<ICompletionData> GetMatchedTypes(string partialName)
        {
            var engineController = this.dynamoViewModel.Model.EngineController;

            return engineController.CodeCompletionServices
                                   .SearchTypes(partialName, dynamoViewModel.CurrentSpace.ElementResolver)
                                   .Select(x => new CodeBlockCompletionData(x));
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

            rules.Add(CreateClassHighlightRule());
        }

        private HighlightingRule CreateClassHighlightRule()
        {
            Color color = (Color)ColorConverter.ConvertFromString("#2E998F");
            var classHighlightRule = new HighlightingRule();
            classHighlightRule.Color = new HighlightingColor()
            {
                Foreground = new Dynamo.Wpf.Views.CodeBlockEditorUtils.CustomizedBrush(color)
            };

            var engineController = dynamoViewModel.Model.EngineController;
            var wordList = engineController.CodeCompletionServices.GetClasses();
            String regex = String.Format(@"\b({0})({0})?\b", String.Join("|", wordList));
            classHighlightRule.Regex = new Regex(regex);

            return classHighlightRule;
        }

        #endregion

        #region Event handlers

        private void OnTextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0)
            {
                char currentChar = e.Text[0];
                if (completionWindow == null)
                {
                    string validCharacters = "[]=.:\"";
                    if (!char.IsWhiteSpace(currentChar) &&
                        !char.IsLetterOrDigit(currentChar) && 
                        !validCharacters.Contains(currentChar))
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
        }

        private void OnTextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (!InnerTextEditor.Text.Contains(':'))
                return;

            string type = InnerTextEditor.Text.Split(':').Last().Trim();
            var types = this.GetMatchedTypes(type);
            if (types == null || !types.Any())
                return;

            int index = InnerTextEditor.Text.IndexOf(':');
            if (index < 0 || InnerTextEditor.CaretOffset <= index)
                return;

            ShowCompletionWindow(types, InnerTextEditor.Text.Length - type.Length);
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

        private void ShowCompletionWindow(IEnumerable<ICompletionData> completions, int offset)
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
            completionWindow.StartOffset = offset; 
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
    }
}
