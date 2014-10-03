using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ProtoCore.Mirror;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
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

        private CompletionWindow completionWindow = null;
        private CodeBlockNodeModel nodeModel = null;
        private CodeCompletionParser codeParser = null;
        

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
            this.Loaded += (obj, args) => this.InnerTextEditor.TextArea.Focus();

            // Register auto-completion callbacks
            this.InnerTextEditor.TextArea.TextEntering += OnTextAreaTextEntering;
            this.InnerTextEditor.TextArea.TextEntered += OnTextAreaTextEntered;

            InitializeSyntaxHighlighter();
        }

        private IEnumerable<ICompletionData> GetCompletionData(string code, string stringToComplete, Guid codeBlockGuid)
        {
            IEnumerable<CodeBlockCompletionData> completions = null;
            var engineController = this.dynamoViewModel.Model.EngineController;

            // Determine if the string to be completed is a class
            var type = engineController.GetClassType(stringToComplete);
            if (type != null)
            {
                var members = type.GetMembers();
                completions = members.Select<StaticMirror, CodeBlockCompletionData>(
                    x => CodeBlockCompletionData.ConvertMirrorToCompletionData(x, this));
            }
            // If not of class type
            else
            {
                // Check if the string to be completed is a declared variable
                string typeName = CodeCompletionParser.GetVariableType(code, stringToComplete);
                if (typeName != null)
                    type = engineController.GetClassType(typeName);

                if (type != null)
                {
                    var members = type.GetInstanceMembers();
                    completions = members.Select<StaticMirror, CodeBlockCompletionData>(
                        x => CodeBlockCompletionData.ConvertMirrorToCompletionData(x, this));
                }
            }
            
            return completions;
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

        internal bool Focus()
        {
            return InternalEditor.Focus();
        }

        #region Dependency Property
        public static readonly DependencyProperty CodeProperty = DependencyProperty.Register("Code", typeof(string),
            typeof(CodeBlockEditor), new PropertyMetadata((obj, args) =>
            {
                var target = (CodeBlockEditor)obj;
                target.Code = (string)args.NewValue;
            })
        );
        #endregion

        #region Auto-complete event handlers
        private void OnTextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text.Length > 0 && completionWindow != null)
                {
                    // If a completion item is highlighted and the user types
                    // a special character or function key, select the item and insert it
                    if (!char.IsLetterOrDigit(e.Text[0]))
                        completionWindow.CompletionList.RequestInsertion(e);
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
                if (e.Text == ".")
                {
                    var code = this.InnerTextEditor.Text.Substring(0, this.InnerTextEditor.CaretOffset);
                    
                    string stringToComplete = CodeCompletionParser.GetStringToComplete(code).Trim('.');
                    
                    var completions = this.GetCompletionData(code, stringToComplete, nodeModel.GUID);

                    if (completions.Count() == 0)
                        return;

                    // TODO: Need to make this more efficient by instantiating 'completionWindow'
                    // just once and updating its contents each time

                    // This implementation has been referenced from
                    // http://www.codeproject.com/Articles/42490/Using-AvalonEdit-WPF-Text-Editor
                    completionWindow = new CompletionWindow(this.InnerTextEditor.TextArea);
                    var data = completionWindow.CompletionList.CompletionData;

                    foreach (var completion in completions)
                        data.Add(completion);

                    completionWindow.Show();
                    completionWindow.Closed += delegate
                    {
                        completionWindow = null;
                    };
                }
            }
            catch (System.Exception ex)
            {
                this.dynamoViewModel.Model.Logger.Log("Failed to perform code block autocomplete with exception:");
                this.dynamoViewModel.Model.Logger.Log(ex.Message);
                this.dynamoViewModel.Model.Logger.Log(ex.StackTrace);
            }
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
                WatermarkLabel.Visibility = System.Windows.Visibility.Collapsed;

        }
        #endregion

        #region Private Helper Methods
        private void InitializeSyntaxHighlighter()
        {
            var stream = GetType().Assembly.GetManifestResourceStream(
                "Dynamo.UI.Resources." + Configurations.HighlightingFile);

            this.InnerTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader(stream), HighlightingManager.Instance);

            // Highlighting Digits
            var rules = this.InnerTextEditor.SyntaxHighlighting.MainRuleSet.Rules;
            rules.Add(CodeBlockUtils.CreateDigitRule());
        }

        private void OnRequestReturnFocusToSearch()
        {
            dynamoViewModel.ReturnFocusToSearch();
        }

        private void HandleEscape()
        {
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
