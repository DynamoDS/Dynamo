//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Nodes.Prompts;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Search;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoCommands = Dynamo.UI.Commands.DynamoCommands;
using String = System.String;

namespace Dynamo.Controls
{
    /// <summary>
    ///     Interaction logic for DynamoForm.xaml
    /// </summary>
    public partial class DynamoView : Window
    {
        public const int CANVAS_OFFSET_Y = 0;
        public const int CANVAS_OFFSET_X = 0;

        private Point dragOffset;
#pragma warning disable 649
        private dynNodeView draggedNode;
#pragma warning restore 649
        private DynamoViewModel _vm;
        private Stopwatch _timer;

        public bool ConsoleShowing
        {
            get { return LogScroller.Height > 0; }
        }

        public static Application MakeSandboxAndRun()
        {
            var controller = DynamoController.MakeSandbox();
            var app = new Application();

            //create the view
            var ui = new DynamoView();
            ui.DataContext = controller.DynamoViewModel;
            controller.UIDispatcher = ui.Dispatcher;

            app.Run(ui);

            return app;
        }

        public DynamoView()
        {
            _timer = new Stopwatch();
            _timer.Start();

            InitializeComponent();

            this.Loaded += dynBench_Activated;
        }


        void vm_RequestLayoutUpdate(object sender, EventArgs e)
        {
            UpdateLayout();
        }

        private void dynBench_Activated(object sender, EventArgs e)
        {
            this.WorkspaceTabs.SelectedIndex = 0;
            _vm = (DataContext as DynamoViewModel);
            _vm.Model.RequestLayoutUpdate += vm_RequestLayoutUpdate;
            _vm.PostUiActivationCommand.Execute(null);

            _timer.Stop();
            DynamoLogger.Instance.Log(String.Format("{0} elapsed for loading Dynamo main window.",
                                                                     _timer.Elapsed));
            LoadSamplesMenu();

            #region Search initialization

            var search = new SearchView {DataContext = dynSettings.Controller.SearchViewModel};
            sidebarGrid.Children.Add(search);
            dynSettings.Controller.SearchViewModel.Visible = true;

            #endregion

            //PACKAGE MANAGER
            _vm.RequestPackagePublishDialog += _vm_RequestRequestPackageManagerPublish;
            _vm.RequestManagePackagesDialog += new EventHandler(_vm_RequestShowInstalledPackages);
            _vm.RequestPackageManagerSearchDialog += new EventHandler(_vm_RequestShowPackageManagerSearch);

            //FUNCTION NAME PROMPT
            _vm.Model.RequestsFunctionNamePrompt += _vm_RequestsFunctionNamePrompt;

            _vm.RequestClose += new EventHandler(_vm_RequestClose);
            _vm.RequestSaveImage += new ImageSaveEventHandler(_vm_RequestSaveImage);

            dynSettings.Controller.RequestsCrashPrompt += new DynamoController.CrashPromptHandler(Controller_RequestsCrashPrompt);

            DynamoSelection.Instance.Selection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Selection_CollectionChanged);

            _vm.RequestUserSaveWorkflow += new WorkspaceSaveEventHandler(_vm_RequestUserSaveWorkflow);

            dynSettings.Controller.ClipBoard.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ClipBoard_CollectionChanged);
        }
        
        private PackageManagerPublishView _pubPkgView;
        void _vm_RequestRequestPackageManagerPublish(PublishPackageViewModel model)
        {
            if (_pubPkgView == null)
            {
                _pubPkgView = new PackageManagerPublishView(model);
                _pubPkgView.Closed += (sender, args) => _pubPkgView = null;
                _pubPkgView.Show();
               
                if (_pubPkgView.IsLoaded && this.IsLoaded) _pubPkgView.Owner = this;
            }

            _pubPkgView.Focus();
        }

        private PackageManagerSearchView _searchPkgsView;
        private PackageManagerSearchViewModel _pkgSearchVM;
        void _vm_RequestShowPackageManagerSearch(object s, EventArgs e)
        {
            if (_pkgSearchVM == null)
            {
                _pkgSearchVM = new PackageManagerSearchViewModel(dynSettings.PackageManagerClient);
            }

            if (_searchPkgsView == null)
            {
                _searchPkgsView = new PackageManagerSearchView(_pkgSearchVM);
                _searchPkgsView.Closed += (sender, args) => _searchPkgsView = null;
                _searchPkgsView.Show();

                 if (_searchPkgsView.IsLoaded && this.IsLoaded) _searchPkgsView.Owner = this;
            }
            
            _searchPkgsView.Focus();
            _pkgSearchVM.RefreshAndSearchAsync();
        }

        private InstalledPackagesView _installedPkgsView;
        void _vm_RequestShowInstalledPackages(object s, EventArgs e)
        {
            if (_installedPkgsView == null)
            {
                _installedPkgsView = new InstalledPackagesView();
                _installedPkgsView.Closed += (sender, args) => _installedPkgsView = null;
                _installedPkgsView.Show();

                if (_installedPkgsView.IsLoaded && this.IsLoaded)  _installedPkgsView.Owner = this;
            }
            _installedPkgsView.Focus();
        }

        void ClipBoard_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _vm.CopyCommand.RaiseCanExecuteChanged();
            _vm.PasteCommand.RaiseCanExecuteChanged();
        }

        void _vm_RequestUserSaveWorkflow(object sender, WorkspaceSaveEventArgs e)
        {
            var dialogText = "";
            if (e.Workspace is FuncWorkspace)
            {
                dialogText = "You have unsaved changes to custom node workspace: \"" + e.Workspace.Name +
                             "\"\n\n Would you like to save your changes?";
            }
            else // homeworkspace
            {
                if (string.IsNullOrEmpty(e.Workspace.FilePath))
                {
                    dialogText = "You have unsaved changes to the Home workspace." +
                                 "\n\n Would you like to save your changes?";
                }
                else
                {
                    dialogText = "You have unsaved changes to " + Path.GetFileName(e.Workspace.FilePath) +
                    "\n\n Would you like to save your changes?";
                }
            }

            var buttons = e.AllowCancel ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo;
            var result = System.Windows.MessageBox.Show(dialogText, "Confirmation", buttons, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _vm.ShowSaveDialogIfNeededAndSave(e.Workspace);
                e.Success = true;
            }
            else if (result == MessageBoxResult.Cancel)
            {
                //return false;
                e.Success = false;
            }
            else
            {
                e.Success = true;
            }
        }

        void Selection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _vm.CopyCommand.RaiseCanExecuteChanged();
            _vm.PasteCommand.RaiseCanExecuteChanged();
        }

        void Controller_RequestsCrashPrompt(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var prompt = new CrashPrompt(e.Exception.Message + "\n\n" + e.Exception.StackTrace);
            prompt.ShowDialog();
        }

        //void PackageManagerClient_RequestSetLoginState(object sender, LoginStateEventArgs e)
        //{
        //    PackageManagerLoginState.Text = e.Text;
        //    PackageManagerLoginButton.IsEnabled = e.Enabled;
        //}

        void _vm_RequestSaveImage(object sender, ImageSaveEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Path))
            {
                //var bench = dynSettings.Bench;

                //if (bench == null)
                //{
                //    DynamoLogger.Instance.Log("Cannot export bench as image without UI.  No image wil be exported.");
                //    return;
                //}

                var control = WPF.FindChild<DragCanvas>(this, null);

                double width = 1;
                double height = 1;

                // connectors are most often within the bounding box of the nodes and notes

                foreach (NodeModel n in dynSettings.Controller.DynamoModel.CurrentWorkspace.Nodes)
                {
                    width = Math.Max(n.X + n.Width, width);
                    height = Math.Max(n.Y + n.Height, height);
                }

                foreach (NoteModel n in dynSettings.Controller.DynamoModel.CurrentWorkspace.Notes)
                {
                    width = Math.Max(n.X + n.Width, width);
                    height = Math.Max(n.Y + n.Height, height);
                }

                var rtb = new RenderTargetBitmap( Math.Max(1, (int)width),
                                                  Math.Max(1, (int)height), 
                                                  96, 
                                                  96,
                                                  System.Windows.Media.PixelFormats.Default);

                rtb.Render(control);

                //endcode as PNG
                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

                try
                {
                    using (var stm = File.Create(e.Path))
                    {
                        pngEncoder.Save(stm);
                    }
                }
                catch
                {
                    DynamoLogger.Instance.Log("Failed to save the Workspace an image.");
                }
            }
        }

        void _vm_RequestClose(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the request for the presentation of the function name prompt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _vm_RequestsFunctionNamePrompt(object sender, FunctionNamePromptEventArgs e)
        {
            ShowNewFunctionDialog(e);
        }

        /// <summary>
        /// Presents the function name dialogue. Returns true if the user enters
        /// a function name and category.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public void ShowNewFunctionDialog(FunctionNamePromptEventArgs e)
        {
            string error = "";

            do
            {
                //var dialog = new FunctionNamePrompt(dynSettings.Controller.SearchViewModel.Categories, error);
                var dialog = new FunctionNamePrompt(dynSettings.Controller.SearchViewModel.Categories)
                {
                    nameBox = { Text = e.Name },
                    categoryBox = { Text = e.Category },
                    DescriptionInput = { Text = e.Description }
                };

                if (dialog.ShowDialog() != true)
                {
                    e.Success = false;
                    return;
                }

                if (String.IsNullOrEmpty(dialog.Text))
                {
                    error = "You must supply a name.";
                    MessageBox.Show(error, "Custom Node Property Error", MessageBoxButton.OK,
                                                   MessageBoxImage.Error);
                }
                else if (e.Name != dialog.Text && dynSettings.Controller.CustomNodeManager.Contains(dialog.Text))
                {
                    error = "A custom node with the given name already exists.";
                    MessageBox.Show(error, "Custom Node Property Error", MessageBoxButton.OK,
                                                   MessageBoxImage.Error);
                }
                else if (e.Name != dialog.Text && dynSettings.Controller.BuiltInTypesByNickname.ContainsKey(dialog.Text))
                {
                    error = "A built-in node with the given name already exists.";
                    MessageBox.Show(error, "Custom Node Property Error", MessageBoxButton.OK,
                                                   MessageBoxImage.Error);
                }
                else if (dialog.Category.Equals(""))
                {
                    error = "You must enter a new category or choose one from the existing categories.";
                    MessageBox.Show(error, "Custom Node Property Error", MessageBoxButton.OK,
                                                   MessageBoxImage.Error);
                }
                else
                {
                    error = "";
                }

                e.Name = dialog.Text;
                e.Category = dialog.Category;
                e.Description = dialog.Description;

            } while (!error.Equals(""));

            e.Success = true;
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (_vm.exitInvoked)
                return;

            var res = _vm.AskUserToSaveWorkspacesOrCancel();
            if (!res)
            {
                e.Cancel = true;
                return;
            }

            dynSettings.Controller.ShutDown();
        }

        private void WindowClosed(object sender, EventArgs e)
        {

        }

        private void OverlayCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_vm.IsUILocked)
                return;

            dynNodeView el = draggedNode;

            Point pos = e.GetPosition(overlayCanvas);

            Canvas.SetLeft(el, pos.X - dragOffset.X);
            Canvas.SetTop(el, pos.Y - dragOffset.Y);
        }

        // the key press event is being intercepted before it can get to
        // the active workspace. This code simply grabs the key presses and
        // passes it to thecurrent workspace
        void DynamoView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            int workspace_index = _vm.CurrentWorkspaceIndex;

            WorkspaceViewModel view_model = _vm.Workspaces[workspace_index];

            view_model.WatchEscapeIsDown = true;
        }

        void DynamoView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            int workspace_index = _vm.CurrentWorkspaceIndex;

            WorkspaceViewModel view_model = _vm.Workspaces[workspace_index];

            view_model.WatchEscapeIsDown = false;
        }

        private void Id_butt_OnClick(object sender, RoutedEventArgs e)
        {
            //get the value of the id field 
            //and trigger the command
            string id = id_tb.Text;
            int workspace_index = _vm.CurrentWorkspaceIndex;
            WorkspaceViewModel view_model = _vm.Workspaces[workspace_index];
            if (view_model.FindByIdCommand.CanExecute(id))
                view_model.FindByIdCommand.Execute(id);
        }

        private void WorkspaceTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_vm != null)
            {
                int workspace_index = _vm.CurrentWorkspaceIndex;
                var workspace_vm = _vm.Workspaces[workspace_index];
                workspace_vm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(workspace_vm.Model.X, workspace_vm.Model.Y)));
                workspace_vm.OnZoomChanged(this, new ZoomEventArgs(workspace_vm.Zoom));
            }
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LogScroller.ScrollToBottom();
        }

        /// <summary>
        ///     Setup the "Samples" sub-menu with contents of samples directory.
        /// </summary>
        /// <param name="bench">The bench where the UI will be loaded</param>
        private void LoadSamplesMenu()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string samplesPath = Path.Combine(directory, "samples");

            if (Directory.Exists(samplesPath))
            {
                string[] dirPaths = Directory.GetDirectories(samplesPath);
                string[] filePaths = Directory.GetFiles(samplesPath, "*.dyn");

                // handle top-level files
                if (filePaths.Any())
                {
                    foreach (string path in filePaths)
                    {
                        var item = new MenuItem
                        {
                            Header = Path.GetFileNameWithoutExtension(path),
                            Tag = path
                        };
                        item.Click += OpenSample_Click;
                        SamplesMenu.Items.Add(item);
                    }
                }

                // handle top-level dirs, TODO - factor out to a seperate function, make recusive
                if (dirPaths.Any())
                {
                    foreach (string dirPath in dirPaths)
                    {
                        var dirItem = new MenuItem
                        {
                            Header = Path.GetFileName(dirPath),
                            Tag = Path.GetFileName(dirPath)
                        };

                        filePaths = Directory.GetFiles(dirPath, "*.dyn");
                        if (filePaths.Any())
                        {
                            foreach (string path in filePaths)
                            {
                                var item = new MenuItem
                                {
                                    Header = Path.GetFileNameWithoutExtension(path),
                                    Tag = path
                                };
                                item.Click += OpenSample_Click;
                                dirItem.Items.Add(item);
                            }
                        }
                        SamplesMenu.Items.Add(dirItem);
                    }
                    return;
                }
            }
            //this.fileMenu.Items.Remove(this.samplesMenu);
        }

        /// <summary>
        ///     Callback for opening a sample.
        /// </summary>
        private void OpenSample_Click(object sender, RoutedEventArgs e)
        {
            var path = (string)((MenuItem)sender).Tag;

            if (dynSettings.Controller.DynamoViewModel.IsUILocked)
                dynSettings.Controller.DynamoViewModel.QueueLoad(path);
            else
            {
                if(dynSettings.Controller.DynamoModel.CanGoHome(null))
                    dynSettings.Controller.DynamoModel.Home(null);

                _vm.OpenCommand.Execute(path);
            }
        }
    }
}
