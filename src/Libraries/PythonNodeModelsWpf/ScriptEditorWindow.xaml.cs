using System;
using System.Collections.Generic;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Dynamo.PythonServices;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Indentation;

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
        private FoldingManager foldingManager;
        private TabFoldingStrategy foldingStrategy;
        private bool IsSaved { get; set; }

        // Reasonable max and min font size values for zooming limits
        private const double FONT_MAX_SIZE = 60d;
        private const double FONT_MIN_SIZE = 5d;

        public string CachedEngine { get; set; }

        /// <summary>
        /// Available Python engines.
        /// </summary>
        public ObservableCollection<string> AvailableEngines { get; private set; }

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

            completionProvider = new SharedCompletionProvider(nodeModel.EngineName,
                dynamoViewModel.Model.PathManager.DynamoCoreDirectory);
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

            // Hyperlink color
            editText.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Color.FromArgb(255, 106, 192, 231));

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

            AvailableEngines =
                new ObservableCollection<string>(PythonEngineManager.Instance.AvailableEngines.Select(x => x.Name));
            // Add the serialized Python Engine even if it is missing (so that the user does not see an empty slot)
            if (!AvailableEngines.Contains(nodeModel.EngineName))
            {
                AvailableEngines.Add(nodeModel.EngineName);
            }

            PythonEngineManager.Instance.AvailableEngines.CollectionChanged += UpdateAvailableEngines;

            editText.Text = propValue;
            originalScript = propValue;
            CachedEngine = nodeModel.EngineName;
            EngineSelectorComboBox.SelectedItem = CachedEngine;

            InstallFoldingManager();

        }

        private void InstallFoldingManager()
        {
            editText.TextArea.IndentationStrategy = new PythonIndentationStrategy(editText);

            foldingManager = FoldingManager.Install(editText.TextArea);
            foldingStrategy = new TabFoldingStrategy();
            foldingStrategy.UpdateFoldings(foldingManager, editText.Document);

            editText.TextChanged += EditTextOnTextChanged;

            UpdateFoldings();
        }

        private void UpdateFoldings()
        {
            // Mark the script for saving
            if (IsSaved) IsSaved = false;
            //if (!ShouldUpdate()) return;

            if (foldingManager == null)
                throw new ArgumentNullException("foldingManager");

            var margins = editText.TextArea.LeftMargins.OfType<FoldingMargin>();
            foreach (var margin in margins)
            {
                var test = margin;
                test.FoldingMarkerBrush = new SolidColorBrush(Color.FromArgb(255, 120, 0, 200));
                test.FoldingMarkerBackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                test.SelectedFoldingMarkerBackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
                test.SelectedFoldingMarkerBrush = new SolidColorBrush(Color.FromArgb(255, 0, 125, 255));
            }

            foldingStrategy.UpdateFoldings(foldingManager, editText.Document);

            if (foldingManager.AllFoldings.Any())
            {
                foreach (var section in foldingManager.AllFoldings)
                {
                    // only set the title of the section once
                    if (section.Title != null) continue;

                    var title = section.TextContent.Split(new[] { '\r', '\n' }).FirstOrDefault() + "...";
                    section.Title = title;
                }
            }
        }

        private void EditTextOnTextChanged(object sender, EventArgs e)
        {
            UpdateFoldings();
        }

        // Keeps track of Enter key use
        private bool IsEnterHit { get; set; }
        private bool ShouldUpdate()
        {
            if (!IsEnterHit) return false;
            var offset = editText.CaretOffset;
            var line = editText.Document.GetLineByNumber(editText.Document.GetLineByOffset(offset).LineNumber - 1);
            var text = editText.Document.GetText(line.Offset, line.Length);
            return text.EndsWith(":");
        }


        #region Text Zoom in Python Editor

        /// <summary>
        /// PreviewMouseWheel event handler to zoom in and out
        /// Additional check to make sure reacting to ctrl + mouse wheel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool ctrl = Keyboard.Modifiers == ModifierKeys.Control;
            if (ctrl)
            {
                this.UpdateFontSize(e.Delta > 0);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Function to increases/decreases font size in avalon editor by a specific increment
        /// </summary>
        /// <param name="increase"></param>
        private void UpdateFontSize(bool increase)
        {
            double currentSize = editText.FontSize;

            if (increase)
            {
                if (currentSize < FONT_MAX_SIZE)
                {
                    double newSize = Math.Min(FONT_MAX_SIZE, currentSize + 1);
                    editText.FontSize = newSize;
                }
            }
            else
            {
                if (currentSize > FONT_MIN_SIZE)
                {
                    double newSize = Math.Max(FONT_MIN_SIZE, currentSize - 1);
                    editText.FontSize = newSize;
                }
            }
        }

        #endregion

        #region Autocomplete Event Handlers

        private void UpdateAvailableEngines(object sender = null, NotifyCollectionChangedEventArgs e = null)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if (!AvailableEngines.Contains((string) item))
                    {
                        AvailableEngines.Add((string) item);
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
                    completionWindow.Closed += delegate { completionWindow = null; };
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
            if (CachedEngine != PythonEngineManager.CPython3EngineName)
            {
                CachedEngine = PythonEngineManager.CPython3EngineName;
                EngineSelectorComboBox.SelectedItem = PythonEngineManager.CPython3EngineName;
            }
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            SaveScript();
        }

        private void SaveScript()
        {
            originalScript = editText.Text;
            nodeModel.EngineName = CachedEngine;
            UpdateScript(editText.Text);
            Analytics.TrackEvent(
                Dynamo.Logging.Actions.Save,
                Dynamo.Logging.Categories.PythonOperations);
            IsSaved = true;
        }

        private void OnRevertClicked(object sender, RoutedEventArgs e)
        {
            if (nodeWasModified)
            {
                editText.Text = originalScript;
                CachedEngine = nodeModel.EngineName;
                EngineSelectorComboBox.SelectedItem = CachedEngine;
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
            dynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(
                new Uri(PythonNodeModels.Properties.Resources.PythonMigrationWarningUriString, UriKind.Relative)));
        }

        private void OnEngineChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CachedEngine != nodeModel.EngineName)
            {
                nodeWasModified = true;
                // Cover what switch did user make. Only track when the new engine option is different with the previous one.
                Analytics.TrackEvent(
                    Actions.Switch,
                    Categories.PythonOperations,
                    CachedEngine);
            }

            editText.Options.ConvertTabsToSpaces = CachedEngine != PythonEngineManager.IronPython2EngineName;
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
            
            editText.TextChanged -= EditTextOnTextChanged;
            FoldingManager.Uninstall(foldingManager);
            foldingManager = null;
        }

        private void OnUndoClicked(object sender, RoutedEventArgs e)
        {
            if (!editText.CanUndo) return;

            editText.Undo();
            e.Handled = true;
        }

        private void OnRedoClicked(object sender, RoutedEventArgs e)
        {
            if (!editText.CanRedo) return;

            editText.Redo();
            e.Handled = true;
        }

        private void OnZoomInClicked(object sender, RoutedEventArgs e)
        {
            UpdateFontSize(true);
            e.Handled = true;
        }

        private void OnZoomOutClicked(object sender, RoutedEventArgs e)
        {
            UpdateFontSize(false);
            e.Handled = true;
        }

        

        #endregion

        #region Navigation Controls

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.Button).Name.Equals("MaximizeButton"))
            {
                this.WindowState = WindowState.Maximized;
                ToggleButtons(true);
            }
            else
            {
                this.WindowState = WindowState.Normal;
                ToggleButtons(false);
            }
        }


        /// <summary>
        /// Toggles between the Maximize and Normalize buttons on the window
        /// </summary>
        /// <param name="toggle"></param>
        private void ToggleButtons(bool toggle)
        {
            if (toggle)
            {
                this.MaximizeButton.Visibility = Visibility.Collapsed;
                this.NormalizeButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.MaximizeButton.Visibility = Visibility.Visible;
                this.NormalizeButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Lets the user drag this window around with their left mouse button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            DragMove();
        }

        // ESC Button pressed triggers Window close        
        private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!IsSaved) SaveScript();
            this.Close();
        }

        // Updates the IsEnterHit value
        private void EditText_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsEnterHit = true;
            }
            else
            {
                IsEnterHit = false;
            }
        }

        #endregion

    }

}
