using System;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Python;
using Dynamo.ViewModels;
using Dynamo.Logging;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using PythonNodeModels;
using Dynamo.Wpf.Windows;
using System.Linq;
using System.Text.RegularExpressions;

namespace PythonNodeModelsWpf
{
    /// <summary>
    /// Interaction logic for ScriptEditorWindow.xaml
    /// </summary>
    public partial class ScriptEditorWindow : ModelessChildWindow
    {
        private string propertyName = string.Empty;
        private Guid boundNodeId = Guid.Empty;
        private CompletionWindow completionWindow = null;
        private readonly IronPythonCompletionProvider completionProvider;
        private readonly DynamoViewModel dynamoViewModel;
        public PythonNode nodeModel { get; set; }
        private bool nodeWasModified = false;
        private string originalScript;

        public ScriptEditorWindow(
            DynamoViewModel dynamoViewModel, 
            PythonNode nodeModel, 
            NodeView nodeView,
            ref ModelessChildWindow.WindowRect windowRect
            ) : base(nodeView, ref windowRect)
        {
            this.dynamoViewModel = dynamoViewModel;
            this.nodeModel = nodeModel;

            completionProvider = new IronPythonCompletionProvider(dynamoViewModel.Model.PathManager.DynamoCoreDirectory);
            completionProvider.MessageLogged += dynamoViewModel.Model.Logger.Log;

            InitializeComponent();

            Dynamo.Logging.Analytics.TrackScreenView("Python");
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
                        "PythonNodeModelsWpf.Resources." + highlighting);

            editText.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader(elem), HighlightingManager.Instance);

            editText.Text = propValue;
            originalScript = propValue;
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
                    ShowStandardCompletionWindow();
                }
                else if (completionWindow == null && (e.Text == "_" || Char.IsLetterOrDigit(e.Text, 0)))
                {
                    ShowBasicCompletionWindow();
                }
            }
            catch (Exception ex)
            {
                dynamoViewModel.Model.Logger.Log("Failed to perform python autocomplete with exception:");
                dynamoViewModel.Model.Logger.Log(ex.Message);
                dynamoViewModel.Model.Logger.Log(ex.StackTrace);
            }
        }

        private void ShowStandardCompletionWindow()
        {
            var subString = editText.Text.Substring(0, editText.CaretOffset);
            var completions = completionProvider.GetCompletionData(subString, false);

            if (completions.Length == 0)
            {
                return;
            }

            completionWindow = new CompletionWindow(editText.TextArea);
            var data = completionWindow.CompletionList.CompletionData;

            foreach (var completion in completions)
            {
                data.Add(completion);
            }

            completionWindow.Show();
            completionWindow.Closed += delegate
            {
                completionWindow = null;
            };
        }

        private void ShowBasicCompletionWindow()
        {
            string code = editText.Text.Substring(0, editText.CaretOffset);
            string lastWord = Regex.Match(code, @"\w+$").Value;

            //start autocompleting from the 2nd char and don't try to autocomplete numbers
            int temp;
            if (lastWord.Length < 2 || int.TryParse(lastWord, out temp))
            {
                return;
            }

            var completions = BasicCompletionData.PrepareAutocompletion(lastWord, code);
            if (completions.Any())
            {
                completionWindow = new CompletionWindow(editText.TextArea);
                var data = completionWindow.CompletionList.CompletionData;
                //                completionWindow.CloseAutomatically = true;
                completionWindow.StartOffset -= lastWord.Length;
                completionWindow.PreviewKeyDown += dismissAutocompletion;

                foreach (var c in completions)
                {
                    data.Add(c);
                }

                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
        }

        #endregion

        #region Private Event Handlers

        private void dismissAutocompletion(object sender, KeyEventArgs e)
        {
            string text = null;
            bool showStandardCompletionAfter = false;
            switch (e.Key)
            {
                case Key.Space:
                    text = " "; break;
                case Key.Oem4:
                    text = "["; break;
                case Key.Enter:
                    text = Environment.NewLine; break;
                case Key.D9:
                    text = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift) ? "(" : null; break;
                case Key.D0:
                    text = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift) ? ")" : null; break;
                case Key.OemComma:
                    text = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift) ? "<" : ","; break;
                case Key.OemPeriod:
                    if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        text = ">";
                    }
                    else
                    {
                        text = ".";
                        showStandardCompletionAfter = true;
                    }
                    break;
                case Key.OemPlus:
                    text = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift) ? "+" : "="; break;
                case Key.OemMinus:
                    text = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift) ? "_" : "-"; break;
            }

            if (!String.IsNullOrEmpty(text))
            {
                completionWindow.PreviewKeyDown -= dismissAutocompletion;
                completionWindow.Close();
                e.Handled = true;
                editText.Document.Insert(editText.CaretOffset, text);
                if (showStandardCompletionAfter)
                    ShowStandardCompletionWindow();
                return;
            }
            base.OnPreviewKeyDown(e);
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            UpdateScript(editText.Text);
            this.Close();
        }

        private void OnRevertClicked(object sender, RoutedEventArgs e)
        {
            if (nodeWasModified)
            {
                UpdateScript(originalScript);
            }
            
            this.Close();
        }

        private void UpdateScript(string scriptText)
        {
            var command = new DynamoModel.UpdateModelValueCommand(
                System.Guid.Empty, boundNodeId, propertyName, scriptText);

            dynamoViewModel.ExecuteCommand(command);
            this.Focus();
            nodeWasModified = true;
            nodeModel.OnNodeModified();
        }

        private void OnRunClicked(object sender, RoutedEventArgs e)
        {
            UpdateScript(editText.Text);
            if (dynamoViewModel.HomeSpace.RunSettings.RunType != RunType.Automatic)
            {
                dynamoViewModel.HomeSpace.Run();
            }
        }

        #endregion

    }
}
