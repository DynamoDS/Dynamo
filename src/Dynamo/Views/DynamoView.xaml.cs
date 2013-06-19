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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using Dynamo.Utilities;

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
        private dynNodeView draggedNode;
        private DynamoViewModel _vm;
        private Stopwatch _timer;

        public bool ConsoleShowing
        {
            get { return LogScroller.Height > 0; }
        }

        public DynamoView()
        {
            _timer = new Stopwatch();
            _timer.Start();

            InitializeComponent();

            this.Loaded += dynBench_Activated;
            this.LayoutUpdated += new EventHandler(DynamoView_LayoutUpdated);
        }

        void DynamoView_LayoutUpdated(object sender, EventArgs e)
        {
            //Debug.WriteLine("Dynamo view layout updated.");
        }

        void vm_RequestLayoutUpdate(object sender, EventArgs e)
        {
            UpdateLayout();
        }

        private void dynBench_Activated(object sender, EventArgs e)
        {

            this.WorkspaceTabs.SelectedIndex = 0;
            _vm = (DataContext as DynamoViewModel);
            _vm.RequestLayoutUpdate += vm_RequestLayoutUpdate;
            _vm.PostUIActivationCommand.Execute();

            _timer.Stop();
            dynSettings.Controller.DynamoViewModel.Log(string.Format("{0} elapsed for loading Dynamo main window.",
                                                                     _timer.Elapsed));
        }

        private void WindowClosing(object sender, CancelEventArgs  e)
        {
            if (_vm.exitInvoked)
                return;

            var res = _vm.AskUserToSaveWorkspacesOrCancel();
            if (!res)
                e.Cancel = true;
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            _vm.CleanupCommand.Execute();
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

        private void LogScroller_OnSourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            LogScroller.ScrollToEnd();
        }

        // the key press event is being intercepted before it can get to
        // the active workspace. This code simply grabs the key presses and
        // passes it to thecurrent workspace
        void DynamoView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            int workspace_index = _vm.CurrentWorkspaceIndex;

            dynWorkspaceViewModel view_model = _vm.Workspaces[workspace_index];

            view_model.WatchEscapeIsDown = true;
        }

        void DynamoView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            int workspace_index = _vm.CurrentWorkspaceIndex;

            dynWorkspaceViewModel view_model = _vm.Workspaces[workspace_index];

            view_model.WatchEscapeIsDown = false;
        }

        private void Id_butt_OnClick(object sender, RoutedEventArgs e)
        {
            //get the value of the id field 
            //and trigger the command
            string id = id_tb.Text;
            int workspace_index = _vm.CurrentWorkspaceIndex;
            dynWorkspaceViewModel view_model = _vm.Workspaces[workspace_index];
            if (view_model.FindByIdCommand.CanExecute(id))
                view_model.FindByIdCommand.Execute(id);
        }
    }

    public class CancelEvaluationException : Exception
    {
        public bool Force;

        public CancelEvaluationException(bool force)
            : base("Run Cancelled")
        {
            Force = force;
        }
    }


}
