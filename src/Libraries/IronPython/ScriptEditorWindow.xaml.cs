using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Python;
using Dynamo.ViewModels;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace DSIronPythonNode
{
    /// <summary>
    /// Interaction logic for ScriptEditorWindow.xaml
    /// </summary>
    public partial class ScriptEditorWindow : Window
    {
        private string propertyName = string.Empty;
        private Guid boundNodeId = Guid.Empty;
        private CompletionWindow completionWindow = null;
        private readonly IronPythonCompletionProvider completionProvider;
        private readonly DynamoViewModel dynamoViewModel;

        public ScriptEditorWindow(DynamoViewModel dynamoViewModel)
        {
            this.dynamoViewModel = dynamoViewModel;
            completionProvider = new IronPythonCompletionProvider();
            completionProvider.MessageLogged += dynamoViewModel.Model.Logger.Log;

            InitializeComponent();
            var view = FindUpVisualTree<DynamoView>(this);
            Owner = view;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        internal void Initialize(Guid nodeGuid, string propName, string propValue)
        {
            boundNodeId = nodeGuid;
            propertyName = propName;

            // Register auto-completion callbacks
            editText.TextArea.TextEntering += OnTextAreaTextEntering;
            editText.TextArea.TextEntered += OnTextAreaTextEntered;

            const string highlighting = "ICSharpCode.PythonBinding.Resources.Python.xshd";
            var elem = GetType().Assembly.GetManifestResourceStream(
                        "DSIronPythonNode.Resources." + highlighting);

            editText.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader(elem), HighlightingManager.Instance);

            editText.Text = propValue;
            Closed += OnScriptEditWindowClosed;
        }

        #region Autocomplete Event Handlers

        private void OnTextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text.Length > 0 && completionWindow != null)
                {
                    if (!char.IsLetterOrDigit(e.Text[0]))
                        completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            catch (Exception ex)
            {
                dynamoViewModel.Model.Logger.Log("Failed to perform python autocomplete with exception:");
                dynamoViewModel.Model.Logger.Log(ex.Message);
                dynamoViewModel.Model.Logger.Log(ex.StackTrace);
            }
        }

        private void OnTextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text == ".")
                {
                    var subString = editText.Text.Substring(0, editText.CaretOffset);
                    var completions = completionProvider.GetCompletionData(subString);

                    if (completions.Length == 0)
                        return;

                    completionWindow = new CompletionWindow(editText.TextArea);
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
            catch (Exception ex)
            {
                dynamoViewModel.Model.Logger.Log("Failed to perform python autocomplete with exception:");
                dynamoViewModel.Model.Logger.Log(ex.Message);
                dynamoViewModel.Model.Logger.Log(ex.StackTrace);
            }
        }

        #endregion

        #region Private Event Handlers

        private void OnAcceptClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnScriptEditWindowClosed(object sender, EventArgs e)
        {
            if (DialogResult.HasValue && (DialogResult.Value))
            {
                var command = new DynamoModel.UpdateModelValueCommand(
                    boundNodeId, propertyName, editText.Text);

                dynamoViewModel.ExecuteCommand(command);
            }
        }

        #endregion

        // walk up the visual tree to find object of type T, starting from initial object
        private static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
        {
            DependencyObject current = initial;

            while (current != null && current.GetType() != typeof(T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }
    }
}
