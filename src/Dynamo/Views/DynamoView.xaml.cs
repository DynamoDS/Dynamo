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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Selection;

namespace Dynamo.Controls
{
    /// <summary>
    ///     Interaction logic for DynamoForm.xaml
    /// </summary>
    public partial class DynamoView : Window
    {
        public const int CANVAS_OFFSET_Y = 0;
        public const int CANVAS_OFFSET_X = 0;
        
        internal Dictionary<string, Expander> addMenuCategoryDict
            = new Dictionary<string, Expander>();

        internal Dictionary<string, dynNodeView> addMenuItemsDictNew
            = new Dictionary<string, dynNodeView>();

        private SortedDictionary<string, TypeLoadData> builtinTypes = new SortedDictionary<string, TypeLoadData>();

        private Point dragOffset;
        private dynNodeView draggedElementMenuItem;
        private dynNodeView draggedNode;
        private bool editingName;
        private bool hoveringEditBox;
        //private bool isWindowSelecting;
        //private Point mouseDownPos;
        private DynamoViewModel vm;
        private bool beginNameEditClick;

        public bool UILocked { get; private set; }

        public DynamoView()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(dynBench_Activated);
        }

        void vm_RequestLayoutUpdate(object sender, EventArgs e)
        {
            UpdateLayout();
        }

        void dynBench_Activated(object sender, EventArgs e)
        {

            this.WorkspaceTabs.SelectedIndex = 0;
            vm = (DataContext as DynamoViewModel);
            vm.UILocked += new EventHandler(LockUI);
            vm.UIUnlocked += new EventHandler(UnlockUI);

            vm.RequestLayoutUpdate += new EventHandler(vm_RequestLayoutUpdate);
            //tell the view model to do some port ui-loading 
            vm.PostUIActivationCommand.Execute();
        }

        private void LockUI(object sender, EventArgs e)
        {
            //UILocked = true;
            saveButton.IsEnabled = false;
            clearButton.IsEnabled = false;

            overlayCanvas.IsHitTestVisible = true;
            overlayCanvas.Cursor = Cursors.AppStarting;
            overlayCanvas.ForceCursor = true;

            //MVVM:now handled by the workspace view model
            //WorkBench.Visibility = System.Windows.Visibility.Hidden;
        }

        private void UnlockUI(object sender, EventArgs e)
        {
            //UILocked = false;
            saveButton.IsEnabled = true;
            clearButton.IsEnabled = true;

            overlayCanvas.IsHitTestVisible = false;
            overlayCanvas.Cursor = null;
            overlayCanvas.ForceCursor = false;

            //WorkBench.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        ///     Updates an element and all its ports.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UpdateElement(object sender, MouseButtonEventArgs e)
        {
            var el = sender as dynNodeModel;
            foreach (dynPortModel p in el.InPorts)
            {
                //p.Update();
                Debug.WriteLine("Ports no longer call update....is it still working?");
            }
            //el.OutPorts.ForEach(x => x.Update());
            foreach (dynPortModel p in el.OutPorts)
            {
                //p.Update();
                Debug.WriteLine("Ports no longer call update....is it still working?");
            }
        }

        

        /// <summary>
        ///     Called when the mouse has been moved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnMouseMove(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        ///     Called when a mouse button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void OnMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        ///     Called when a mouse button is released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            vm.CleanupCommand.Execute();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
        }


        private void OverlayCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (UILocked)
                return;

            dynNodeView el = draggedNode;

            Point pos = e.GetPosition(overlayCanvas);

            Canvas.SetLeft(el, pos.X - dragOffset.X);
            Canvas.SetTop(el, pos.Y - dragOffset.Y);
        }

        private void OverlayCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void LogScroller_OnSourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            LogScroller.ScrollToEnd();
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
