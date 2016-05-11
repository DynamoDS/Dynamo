using System;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Dynamo.Python;
using System.Windows.Input;
using System.Xml;
using Dynamo.PluginManager;
using PythonNodeModelsWpf;
using System.IO;
using Dynamo.ViewModels;

namespace PluginManager
{
    /// <summary>
    /// Interaction logic for PluginScriptEditor.xaml
    /// </summary>
    public partial class PluginScriptEditor : Window
    {
        private string propertyName = string.Empty;
        private Guid boundNodeId = Guid.Empty;
        private CompletionWindow completionWindow = null;
        private readonly IronPythonCompletionProvider completionProvider;
        private string filePath;
        private PluginManagerExtension pluginManagerContext;
        private DynamoViewModel dynamoViewModel;
        public PluginScriptEditor(string filePath, PluginManagerExtension pluginManagerContext)
        {
            this.filePath = filePath;
            this.dynamoViewModel = pluginManagerContext.DynamoViewModel;
            completionProvider = new IronPythonCompletionProvider();
            //completionProvider.MessageLogged += dynamoViewModel.Model.Logger.Log;
            this.pluginManagerContext = pluginManagerContext;
            InitializeComponent();
            //var view = FindUpVisualTree<DynamoView>(this);
            //Owner = view;
           // WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        internal void Initialize(/*Guid nodeGuid, string propName, string propValue*/)
        {
           // boundNodeId = nodeGuid;
           // propertyName = propName;
          string propValue = File.ReadAllText(filePath);
            // Register auto-completion callbacks
            editText.TextArea.TextEntering += OnTextAreaTextEntering;
            editText.TextArea.TextEntered += OnTextAreaTextEntered;

            const string highlighting = "ICSharpCode.PythonBinding.Resources.Python.xshd";
            var elem = GetType().Assembly.GetManifestResourceStream(
                        "PluginManager.Resources." + highlighting);

            var ele = this.GetType().Assembly.GetManifestResourceNames();
            //var el = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            editText.SyntaxHighlighting = HighlightingLoader.Load(
                new System.Xml.XmlTextReader(elem), HighlightingManager.Instance);

            editText.Text = propValue;
            //Closed += OnScriptEditWindowClosed;
        }
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
        private void OnRunClicked(object sender, RoutedEventArgs e)
        {
            PluginManagerIronPythonEvaluator.EvaluatePythonString(editText.Text,pluginManagerContext);
            this.WindowState = WindowState.Normal;
        }
        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            System.IO.File.WriteAllText(filePath, string.Empty);
            System.IO.File.WriteAllText(filePath, editText.Text);
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
    }
}
