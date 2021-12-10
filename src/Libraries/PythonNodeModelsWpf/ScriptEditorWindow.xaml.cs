using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Python;
using Dynamo.ViewModels;
using Dynamo.Wpf.Windows;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using PythonNodeModels;
using System.Linq;
using Dynamo.PythonServices;

namespace PythonNodeModelsWpf
{
    /// <summary>
    /// Interaction logic for ScriptEditorWindow.xaml
    /// </summary>
    public partial class ScriptEditorWindow : ModelessChildWindow
    {
        private string propertyName = string.Empty;
        private Guid boundNodeId = Guid.Empty;
        private Guid boundWorkspaceId = Guid.Empty;
        private CompletionWindow completionWindow = null;
        private readonly SharedCompletionProvider completionProvider;
        private readonly DynamoViewModel dynamoViewModel;
        public PythonNode nodeModel { get; set; }
        private bool nodeWasModified = false;
        private string originalScript;

        public string CachedEngineName { get; set; }

        [Obsolete()]
        public PythonEngineVersion CachedEngined { get; set; }

        /// <summary>
        /// Available Python engines.
        /// </summary>
        [Obsolete()]
        public ObservableCollection<PythonEngineVersion> AvailableEnginesd
        {
            get; private set;
        }

        /// <summary>
        /// Available Python engines.
        /// </summary>
        public ObservableCollection<string> AvailableEngineNames
        {
            get; private set;
        }

        public ScriptEditorWindow(
            DynamoViewModel dynamoViewModel,
            PythonNode nodeModel,
            NodeView nodeView,
            ref ModelessChildWindow.WindowRect windowRect
            ) : base(nodeView, ref windowRect)
        {
            this.Closed += OnScriptEditorWindowClosed;
            this.dynamoViewModel = dynamoViewModel;
            this.nodeModel = nodeModel;

            completionProvider = new SharedCompletionProvider(nodeModel.EngineName, dynamoViewModel.Model.PathManager.DynamoCoreDirectory);
            completionProvider.MessageLogged += dynamoViewModel.Model.Logger.Log;
            nodeModel.CodeMigrated += OnNodeModelCodeMigrated;

            InitializeComponent();
            this.DataContext = this;

            EngineSelectorComboBox.Visibility = Visibility.Visible;

            Analytics.TrackScreenView("Python");
        }

        internal void Initialize(Guid workspaceGuid, Guid nodeGuid, string propName, string propValue)
        {
            boundWorkspaceId = workspaceGuid;
            boundNodeId = nodeGuid;
            propertyName = propName;

            // Register auto-completion callbacks
            editText.TextArea.TextEntering += OnTextAreaTextEntering;
            editText.TextArea.TextEntered += OnTextAreaTextEntered;

            // Initialize editor with global settings for show/hide tabs and spaces
            editText.Options = dynamoViewModel.PythonScriptEditorTextOptions.GetTextOptions();

            // Set options to reflect global settings when python script editor in initialized for the first time.
            editText.Options.ShowSpaces = dynamoViewModel.ShowTabsAndSpacesInScriptEditor;
            editText.Options.ShowTabs = dynamoViewModel.ShowTabsAndSpacesInScriptEditor;

            const string highlighting = "ICSharpCode.PythonBinding.Resources.Python.xshd";
            var elem = GetType().Assembly.GetManifestResourceStream(
                        "PythonNodeModelsWpf.Resources." + highlighting);

            editText.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader(elem), HighlightingManager.Instance);

            AvailableEngineNames = new ObservableCollection<string>(PythonEngineManager.Instance.AvailableEngines.Select(x => x.Name));
            // Add the serialized Python Engine even if it is missing (so that the user does not see an empty slot)
            if (!AvailableEngineNames.Contains(nodeModel.EngineName))
            {
                AvailableEngineNames.Add(nodeModel.EngineName);
            }

            PythonEngineManager.Instance.AvailableEngines.CollectionChanged += UpdateAvailableEngines;

            editText.Text = propValue;
            originalScript = propValue;
            CachedEngineName = nodeModel.EngineName;
            EngineSelectorComboBox.SelectedItem = CachedEngineName;
        }
        #region Autocomplete Event Handlers

        private void UpdateAvailableEngines(object sender = null, NotifyCollectionChangedEventArgs e = null)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if (!AvailableEngineNames.Contains((string)item))
                    {
                        AvailableEngineNames.Add((string)item);
                    }
                }
            }
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

        private void OnTextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text == ".")
                {
                    var subString = editText.Text.Substring(0, editText.CaretOffset);
                    var completions = completionProvider.GetCompletionData(subString, false);

                    if (completions == null || completions.Length == 0)
                    {
                        return;
                    }

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

        private void OnNodeModelCodeMigrated(object sender, PythonCodeMigrationEventArgs e)
        {
            originalScript = e.OldCode;
            editText.Text = e.NewCode;
            if (CachedEngineName != PythonEngineManager.CPython3EngineName)
            {
                CachedEngineName = PythonEngineManager.CPython3EngineName;
                EngineSelectorComboBox.SelectedItem = PythonEngineManager.CPython3EngineName;
            }
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            originalScript = editText.Text;
            nodeModel.EngineName = CachedEngineName;
            UpdateScript(editText.Text);
            Analytics.TrackEvent(
                Dynamo.Logging.Actions.Save,
                Dynamo.Logging.Categories.PythonOperations);
        }

        private void OnRevertClicked(object sender, RoutedEventArgs e)
        {
            if (nodeWasModified)
            {
                editText.Text = originalScript;
                CachedEngineName = nodeModel.EngineName;
                EngineSelectorComboBox.SelectedItem = CachedEngineName;
                UpdateScript(originalScript);
            }
        }

        private void UpdateScript(string scriptText)
        {
            var command = new DynamoModel.UpdateModelValueCommand(
                boundWorkspaceId, boundNodeId, propertyName, scriptText);

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
            Analytics.TrackEvent(
                Dynamo.Logging.Actions.Run,
                Dynamo.Logging.Categories.PythonOperations);
        }

        private void OnMigrationAssistantClicked(object sender, RoutedEventArgs e)
        {
            if (nodeModel == null)
                throw new NullReferenceException(nameof(nodeModel));

            UpdateScript(editText.Text);
            Analytics.TrackEvent(
                Dynamo.Logging.Actions.Migration,
                Dynamo.Logging.Categories.PythonOperations);
            nodeModel.RequestCodeMigration(e);
        }

        private void OnMoreInfoClicked(object sender, RoutedEventArgs e)
        {
            dynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(PythonNodeModels.Properties.Resources.PythonMigrationWarningUriString, UriKind.Relative)));
        }

        private void OnEngineChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CachedEngineName != nodeModel.EngineName)
            {
                nodeWasModified = true;
                // Cover what switch did user make. Only track when the new engine option is different with the previous one.
                Analytics.TrackEvent(
                    Dynamo.Logging.Actions.Switch,
                    Dynamo.Logging.Categories.PythonOperations,
                    CachedEngineName.ToString());
            }
            editText.Options.ConvertTabsToSpaces = CachedEngineName != PythonEngineManager.IronPython2EngineName;
        }

        private void OnScriptEditorWindowClosed(object sender, EventArgs e)
        {
            completionProvider?.Dispose();
            nodeModel.CodeMigrated -= OnNodeModelCodeMigrated;
            this.Closed -= OnScriptEditorWindowClosed;
            PythonEngineManager.Instance.AvailableEngines.CollectionChanged -= UpdateAvailableEngines;

            Analytics.TrackEvent(
                Dynamo.Logging.Actions.Close,
                Dynamo.Logging.Categories.PythonOperations);
        }

        #endregion
    }
}
