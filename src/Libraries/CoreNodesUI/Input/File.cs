using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using Autodesk.DesignScript.Runtime;
using Binding = System.Windows.Data.Binding;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBox = System.Windows.Controls.TextBox;
using System.IO;

namespace DSCore.File
{
    [SupressImportIntoVM]
    public abstract class FileSystemBrowser : DSCoreNodesUI.String
    {
        protected FileSystemBrowser(WorkspaceModel workspace, string tip)
            : base(workspace)
        {
            OutPortData[0].ToolTipString = tip;
            RegisterAllPorts();

            Value = "";
        }

        private FileSystemWatcher watcher = null;
        private const string defaultValue = "No file selected.";

        protected bool? isFileWatcherAttached;

        protected void CreateFileWatcher()
        {
            // Do not attach file watcher if no file is selected
            if (string.IsNullOrEmpty(Value) || Value.Equals(defaultValue))
            {
                watcher = null;
                return;
            }

            if (watcher != null)
            {
                watcher.Changed += watcher_Changed;
                return;
            }

            // Create new file watcher and register its "file changed" event
            var dir = Path.GetDirectoryName(Value);

            if (string.IsNullOrEmpty(dir))
                dir = ".";

            var name = Path.GetFileName(Value);

            watcher = new FileSystemWatcher(dir, name)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            watcher.Changed += watcher_Changed;
        }

        protected void RemoveFileWatcher()
        {
            ForceReExecuteOfNode = false;
            if (watcher != null)
                watcher.Changed -= watcher_Changed;
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            ForceReExecuteOfNode = true;
        }

        public override void SetupCustomUIElements(dynNodeView view)
        {
            //add a button to the inputGrid on the dynElement
            var readFileButton = new DynamoNodeButton
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Height = Configurations.PortHeightInPixels
            };

            readFileButton.Click += readFileButton_Click;
            readFileButton.Content = "Browse...";
            readFileButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            readFileButton.VerticalAlignment = VerticalAlignment.Center;

            var tb = new TextBox();
            if (string.IsNullOrEmpty(Value))
                Value = defaultValue;

            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.VerticalAlignment = VerticalAlignment.Center;
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;
            tb.TextChanged += delegate
            {
                tb.ScrollToHorizontalOffset(double.PositiveInfinity);
                view.ViewModel.DynamoViewModel.ReturnFocusToSearch();
            };
            tb.Margin = new Thickness(0, 5, 0, 5);

            // Attach FileWatcher Checkbox
            var checkBox = new System.Windows.Controls.CheckBox
            {
                Content = "Attach FileWatcher",
                Margin = new Thickness(5, 0, 0, 0)
            };
            checkBox.Checked += (obj, args) => { CreateFileWatcher(); isFileWatcherAttached = true; };
            checkBox.Unchecked += (obj, args) => { RemoveFileWatcher(); isFileWatcherAttached = false; };

            var sp = new StackPanel();
            sp.Children.Add(readFileButton);
            sp.Children.Add(tb);
            sp.Children.Add(checkBox);
            view.inputGrid.Children.Add(sp);

            tb.DataContext = this;
            var bindingVal = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new FilePathDisplayConverter()
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
        }

        protected abstract void readFileButton_Click(object sender, RoutedEventArgs e);

        public void Dispose()
        {
            RemoveFileWatcher();
            watcher.Dispose();
        }
    }

    [NodeName("File Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Allows you to select a file on the system to get its filename.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class Filename : FileSystemBrowser
    {
        public Filename(WorkspaceModel workspace) : base(workspace, "Filename") { }

        protected override void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                CheckFileExists = false
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Value = openDialog.FileName;
                if (isFileWatcherAttached == true)
                    CreateFileWatcher();
            }
        }

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }
    }


    [NodeName("Directory Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Allows you to select a directory on the system to get its path.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    //MAGN -3382 [IsVisibleInDynamoLibrary(false)]
    public class Directory : FileSystemBrowser
    {
        public Directory(WorkspaceModel workspace) : base(workspace, "Directory") { }

        protected override void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Value = openDialog.SelectedPath;
                if (isFileWatcherAttached == true)
                    CreateFileWatcher();
            }
        }
    }
}
