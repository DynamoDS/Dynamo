using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    /// <summary>
    /// A Canvas which manages dragging of the UIElements it contains.  
    /// </summary>
    public class DragCanvas : Canvas
    {
        #region Data

        // The owning workspace for this DragCanvas
        public Dynamo.Views.WorkspaceView owningWorkspace = null;

        #endregion // Data

        #region Overrides

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            //Release the mouse capture on left button up.
            //this will allow window selection to continue when mouse accidentally moves beyond the canvas
            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.owningWorkspace == null)
            {
                return;
            }

            // If the focus falls on a node's text box, or a slider's thumb, 
            // this method will be called with "e.NewValue" sets to "true". 
            // In such cases the state machine should be notified, and any 
            // connection that is in progress should be cancelled off.

            object dataContext = this.owningWorkspace.DataContext;
            WorkspaceViewModel wvm = dataContext as WorkspaceViewModel;
            // when there is a connection on a dynamonodebutton, then the connection should not be cancelled
            if (wvm != null && !wvm.CheckActiveConnectorCompatibility(wvm.portViewModel))
            {
                wvm.HandleFocusChanged(this, ((bool)e.NewValue));
            }
            base.OnIsKeyboardFocusWithinChanged(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (owningWorkspace == null)
            {
                return;
            }

            // If we are snapping to a port when the mouse is clicked, the 
            // DragCanvas should not handle the event here (e.Handled should
            // be set to 'false') so that workspace view gets a chance of 
            // handling it.
            //
            // Or if Shift is pressed, Workspace also handles event.
            // Shift modifier is used to show InCanvasSearch.
            if (owningWorkspace.IsSnappedToPort || Keyboard.Modifiers == ModifierKeys.Shift)
            {
                e.Handled = false; // Do not handle it here!
                return;
            }

            object dataContext = this.owningWorkspace.DataContext;
            WorkspaceViewModel wvm = dataContext as WorkspaceViewModel;

            if (wvm.HandleLeftButtonDown(this, e))
            {
                //capture the mouse input even if the mouse is dragged outside the canvas
                this.CaptureMouse();
                base.OnMouseLeftButtonDown(e);
            }
        }

        #endregion // Host Event Handlers
    }
}
