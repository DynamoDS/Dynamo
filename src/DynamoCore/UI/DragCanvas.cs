using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Dynamo.Models;
using Dynamo.Selection;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Dynamo.Controls
{
    /// <summary>
    /// A Canvas which manages dragging of the UIElements it contains.  
    /// </summary>
    public class DragCanvas : Canvas
    {

        private List<DependencyObject> hitResultsList = new List<DependencyObject>();

        #region Data

        // Stores a reference to the UIElement currently being dragged by the user.
        private ObservableCollection<OffsetData> offsets = new ObservableCollection<OffsetData>();

        // Keeps track of where the mouse cursor was when a drag operation began.		
        private Point origCursorLocation;

        private enum DragState
        {
            /// <summary>
            /// This state indicates that there is no on-going drag operation.
            /// </summary>
            None,

            /// <summary>
            /// When a mouse is clicked within a node, the dragState is set to 
            /// Setup, indicating that the DragCanvas is ready for any subsequent
            /// mouse-move event to begin a drag operation. However, the mouse
            /// may be released without getting moved, in which case only 
            /// selection is done (i.e. dragState will be reset back to None).
            /// </summary>
            Setup,

            /// <summary>
            /// There is an on-going drag operation, and all the nodes that 
            /// are getting dragged have already been recorded for undo.
            /// </summary>
            InProgress
        }

        private DragState dragState = DragState.None;

        //true if user is making a connection between elements
        //MVVM: move isConnecting onto the DynamoViewModel
        /*private bool isConnecting = false;
        public bool IsConnecting
        {
            get { return isConnecting; }
            set { isConnecting = value; }
        }*/

        //true if we're ignoring clicks
        public bool ignoreClick;

        // The owning workspace for this DragCanvas
        public Dynamo.Views.dynWorkspaceView owningWorkspace = null;

        #endregion // Data

        #region Attached Properties

        #region CanBeDragged

        public static readonly DependencyProperty CanBeDraggedProperty;

        public static bool GetCanBeDragged(UIElement uiElement)
        {
            if (uiElement == null)
                return false;

            return (bool)uiElement.GetValue(CanBeDraggedProperty);
        }

        public static void SetCanBeDragged(UIElement uiElement, bool value)
        {
            if (uiElement != null)
                uiElement.SetValue(CanBeDraggedProperty, value);
        }

        #endregion // CanBeDragged

        #endregion // Attached Properties

        #region Dependency Properties

        public static readonly DependencyProperty AllowDraggingProperty;
        public static readonly DependencyProperty AllowDragOutOfViewProperty;

        #endregion // Dependency Properties

        #region Static Constructor

        static DragCanvas()
        {
            AllowDraggingProperty = DependencyProperty.Register(
               "AllowDragging",
               typeof(bool),
               typeof(DragCanvas),
               new PropertyMetadata(true));

            AllowDragOutOfViewProperty = DependencyProperty.Register(
               "AllowDragOutOfView",
               typeof(bool),
               typeof(DragCanvas),
               new UIPropertyMetadata(false));

            CanBeDraggedProperty = DependencyProperty.RegisterAttached(
               "CanBeDragged",
               typeof(bool),
               typeof(DragCanvas),
               new UIPropertyMetadata(true));
        }

        #endregion // Static Constructor

        #region Constructor

        /// <summary>
        /// Initializes a new instance of DragCanvas.  UIElements in
        /// the DragCanvas will immediately be draggable by the user.
        /// </summary>
        public DragCanvas()
        {
            DynamoSelection.Instance.Selection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(selection_CollectionChanged);
        }

        /// <summary>
        /// Manages addition and removal of objects from the selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void selection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                throw new Exception("To properly clean the selection, please use RemoveAll() instead.");
            }

            // call the select method on elements added to the collection
            if (e.NewItems != null)
            {
                foreach (ISelectable sel in e.NewItems)
                {
                    var n = sel as ILocatable;
                    
                    if (n == null)
                        continue;
                    //n.Select();

                    //UIElement el = (UIElement)n;

                    //double left = Canvas.GetLeft(el);
                    //double right = Canvas.GetRight(el);
                    //double top = Canvas.GetTop(el);
                    //double bottom = Canvas.GetBottom(el);

                    //MVVM: now storing element coordinates on models
                    double left = n.X;
                    double right = n.X + n.Width;
                    double top = n.Y;
                    double bottom = n.Y + n.Height;

                    // Calculate the offset deltas and determine for which sides
                    // of the Canvas to adjust the offsets.
                    bool modLeft = false;
                    bool modTop = false;
                    double hOffset = ResolveOffset(left, right, out modLeft);
                    double vOffset = ResolveOffset(top, bottom, out modTop);
                    OffsetData os = new OffsetData(hOffset, vOffset, modLeft, modTop, n);
                    offsets.Add(os);
                }
            }

            if (e.OldItems != null)
            {
                // call the deselect method on elements removed from the collection
                foreach (ISelectable n in e.OldItems)
                {
                    //(n as ISelectable).Deselect();

                    // remove the corresponding offsetdata object
                    // for the element being removed
                    List<OffsetData> toRemove = offsets.Where(od => od.Node == n).ToList();

                    foreach (OffsetData od in toRemove)
                    {
                        offsets.Remove(od);
                    }
                }
            }
        }

        #endregion // Constructor

        #region Interface

        #region AllowDragging

        /// <summary>
        /// Gets/sets whether elements in the DragCanvas should be draggable by the user.
        /// The default value is true.  This is a dependency property.
        /// </summary>
        public bool AllowDragging
        {
            get { return (bool)base.GetValue(AllowDraggingProperty); }
            set { base.SetValue(AllowDraggingProperty, value); }
        }

        #endregion // AllowDragging

        #region AllowDragOutOfView

        /// <summary>
        /// Gets/sets whether the user should be able to drag elements in the DragCanvas out of
        /// the viewable area.  The default value is false.  This is a dependency property.
        /// </summary>
        public bool AllowDragOutOfView
        {
            get { return (bool)GetValue(AllowDragOutOfViewProperty); }
            set { SetValue(AllowDragOutOfViewProperty, value); }
        }

        #endregion // AllowDragOutOfView

        #region BringToFront / SendToBack

        /// <summary>
        /// Assigns the element a z-index which will ensure that 
        /// it is in front of every other element in the Canvas.
        /// The z-index of every element whose z-index is between 
        /// the element's old and new z-index will have its z-index 
        /// decremented by one.
        /// </summary>
        /// <param name="targetElement">
        /// The element to be sent to the front of the z-order.
        /// </param>
        public void BringToFront(UIElement element)
        {
            this.UpdateZOrder(element, true);
        }

        /// <summary>
        /// Assigns the element a z-index which will ensure that 
        /// it is behind every other element in the Canvas.
        /// The z-index of every element whose z-index is between 
        /// the element's old and new z-index will have its z-index 
        /// incremented by one.
        /// </summary>
        /// <param name="targetElement">
        /// The element to be sent to the back of the z-order.
        /// </param>
        public void SendToBack(UIElement element)
        {
            this.UpdateZOrder(element, false);
        }

        #endregion // BringToFront / SendToBack

        #region ElementBeingDragged

        #endregion // ElementBeingDragged

        #region FindCanvasChild

        /// <summary>
        /// Walks up the visual tree starting with the specified DependencyObject, 
        /// looking for a UIElement which is a child of the Canvas.  If a suitable 
        /// element is not found, null is returned.  If the 'depObj' object is a 
        /// UIElement in the Canvas's Children collection, it will be returned.
        /// </summary>
        /// <param name="depObj">
        /// A DependencyObject from which the search begins.
        /// </param>
        public UIElement FindCanvasChild(DependencyObject depObj)
        {
            while (depObj != null)
            {
                // If the current object is a UIElement which is a child of the
                // Canvas, exit the loop and return it.
                UIElement elem = depObj as UIElement;
                if (elem != null && base.Children.Contains(elem))
                    break;

                // VisualTreeHelper works with objects of type Visual or Visual3D.
                // If the current object is not derived from Visual or Visual3D,
                // then use the LogicalTreeHelper to find the parent element.
                if (depObj is Visual || depObj is Visual3D)
                    depObj = VisualTreeHelper.GetParent(depObj);
                else
                    depObj = LogicalTreeHelper.GetParent(depObj);
            }
            return depObj as UIElement;
        }

        #endregion // FindCanvasChild

        #endregion // Interface

        #region Overrides

        #region OnMouseLeftButtonDown

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (ignoreClick)
            {
                ignoreClick = false;
                return;
            }

            //we know the data context will have the is connecting property
            //but we don't want an explicit reference to it
            //the datacontext will be the workspace elements collection
            //which after the full UI separation now lives on the view
            //but, the isconnecting property lives on the ViewModel and we
            //store a reference to that on the view
            //var vm = DataContext.GetType().GetProperty("ViewModel").GetValue(DataContext, null);
            if (!(bool)DataContext.GetType().GetProperty("IsConnecting").GetValue(DataContext, null))
            {
                //test if we're hitting the background
                // Retrieve the coordinate of the mouse position.
                Point pt = e.GetPosition(this);
                Debug.WriteLine(string.Format("Hit point x:{0} y:{0}", pt.X, pt.Y));

                if (DynamoSelection.Instance.Selection.Count == 0)
                    return;

                //for every node in the current selection, see whether the mouse
                //coordinates are within its rectangle. if so, start dragging.
                foreach (ISelectable sel in DynamoSelection.Instance.Selection)
                {
                    var el = sel as ILocatable;
                    if(el == null)
                        continue;

                    if (el.Rect.Contains(pt))
                    {
                        Debug.WriteLine(string.Format("Hit selectable {0}.", sel.GetType().ToString()));
                        base.OnMouseLeftButtonDown(e);

                        // Cache the mouse cursor location.
                        this.origCursorLocation = e.GetPosition(this);
                        Debug.WriteLine(string.Format("ResetCursorLocation point x:{0} y:{0}", this.origCursorLocation.X, this.origCursorLocation.Y));

                        this.dragState = DragState.Setup;

                        e.Handled = true;
                        return;
                    }
                }

            }
        }

        #endregion // OnPreviewMouseLeftButtonDown

        #region OnPreviewMouseMove

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (DynamoSelection.Instance.Selection.Count == 0 || (DragState.None == dragState))
                return;

            if (DragState.Setup == dragState)
            {
                dragState = DragState.InProgress; // The drag operation is now started.

                if (null != this.owningWorkspace)
                {
                    // This is where we attempt to store all the models in undo recorder 
                    // before they are modified (i.e. being dragged around the canvas).
                    // Note that we only do this once when the first mouse-move occurs 
                    // after a mouse-down, because mouse-down can potentially be used 
                    // just to select a node (as opposed to moving the selected nodes), in 
                    // which case we don't want any of the nodes to be recorded for undo.
                    // 
                    List<ModelBase> models = DynamoSelection.Instance.Selection.
                        Where((x) => (x is ModelBase)).Cast<ModelBase>().ToList<ModelBase>();

                    WorkspaceModel workspaceModel = owningWorkspace.ViewModel.Model;
                    workspaceModel.RecordModelsForModification(models);
                    DynamoController controller = Dynamo.Utilities.dynSettings.Controller;
                    controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
                    controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
                }
            }

            // Get the position of the mouse cursor, relative to the Canvas.
            Point cursorLocation = e.GetPosition(this);

            #region Calculate Offsets

            int count = 0;
            foreach (ISelectable sel in DynamoSelection.Instance.Selection)
            {
                OffsetData od = offsets[count];

                // Determine the horizontal offset.
                if (od.ModifyLeftOffset)
                    od.NewHorizontalOffset = od.OriginalHorizontalOffset + (cursorLocation.X - this.origCursorLocation.X);
                else
                    od.NewHorizontalOffset = od.OriginalHorizontalOffset - (cursorLocation.X - this.origCursorLocation.X);

                // Determine the vertical offset.
                if (od.ModifyTopOffset)
                    od.NewVerticalOffset = od.OriginalVerticalOffset + (cursorLocation.Y - this.origCursorLocation.Y);
                else
                    od.NewVerticalOffset = od.OriginalVerticalOffset - (cursorLocation.Y - this.origCursorLocation.Y);

                //Debug.WriteLine(string.Format("New h:{0} v:{1}", od.NewHorizontalOffset, od.NewVerticalOffset));
                count++;
            }

            #endregion // Calculate Offsets

            if (!this.AllowDragOutOfView)
            {
                #region Verify Drag Element Location

                count = 0;
                foreach (ISelectable sel in DynamoSelection.Instance.Selection)
                {
                    var el = sel as ILocatable;
                    if (el == null)
                        continue;

                    OffsetData od = offsets[count];

                    // Get the bounding rect of the drag element.
                    Rect elemRect = this.CalculateDragElementRect(el, od.NewHorizontalOffset, od.NewVerticalOffset, od.ModifyLeftOffset, od.ModifyTopOffset);

                    // If the element is being dragged out of the viewable area, 
                    // determine the ideal rect location, so that the element is 
                    // within the edge(s) of the canvas.
                    //
                    bool leftAlign = elemRect.Left < 0;
                    bool rightAlign = elemRect.Right > this.ActualWidth;

                    if (leftAlign)
                        od.NewHorizontalOffset = od.ModifyLeftOffset ? 0 : this.ActualWidth - elemRect.Width;
                    else if (rightAlign)
                        od.NewHorizontalOffset = od.ModifyLeftOffset ? this.ActualWidth - elemRect.Width : 0;

                    bool topAlign = elemRect.Top < 0;
                    bool bottomAlign = elemRect.Bottom > this.ActualHeight;

                    if (topAlign)
                        od.NewVerticalOffset = od.ModifyTopOffset ? 0 : this.ActualHeight - elemRect.Height;
                    else if (bottomAlign)
                        od.NewVerticalOffset = od.ModifyTopOffset ? this.ActualHeight - elemRect.Height : 0;
                    count++;
                }

                #endregion // Verify Drag Element Location
            }

            #region Move Drag Element
            count = 0;
            this.Dispatcher.Invoke(new Action(
                  delegate
                  {
                      foreach (ISelectable sel in DynamoSelection.Instance.Selection)
                      {
                          var el = sel as ILocatable;
                          if (el == null)
                              continue;

                          var od = offsets[count];

                          if (od.ModifyLeftOffset)
                              //Canvas.SetLeft(el, od.NewHorizontalOffset);
                              el.X = od.NewHorizontalOffset;
                          else
                              //Canvas.SetRight(el, od.NewHorizontalOffset);
                              el.X = od.NewHorizontalOffset + el.Width;
                          if (od.ModifyTopOffset)
                              //Canvas.SetTop(el, od.NewVerticalOffset);
                              el.Y = od.NewVerticalOffset;
                          else
                              //Canvas.SetBottom(el, od.NewVerticalOffset);
                              el.Y = od.NewHorizontalOffset + el.Height;
                          count++;
                      }
                  }
               ), DispatcherPriority.Render, null);
            #endregion // Move Drag Element

        }

        #endregion // OnPreviewMouseMove

        #region OnHostPreviewMouseUp

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            this.dragState = DragState.None;

            // recalculate the offsets for all items in
            // the selection. 
            int count = 0;
            foreach (ISelectable sel in DynamoSelection.Instance.Selection)
            {
                var n = sel as ILocatable;
                if (n == null)
                    continue;

                //UIElement el = (UIElement)n;

                //double left = Canvas.GetLeft(el);
                //double right = Canvas.GetRight(el);
                //double top = Canvas.GetTop(el);
                //double bottom = Canvas.GetBottom(el);

                //MVVM: now storing element coordinates on models
                double left = n.X;
                double right = n.X + n.Width;
                double top = n.Y;
                double bottom = n.Y + n.Height;

                // Calculate the offset deltas and determine for which sides
                // of the Canvas to adjust the offsets.
                bool modLeft = false;
                bool modTop = false;
                double hOffset = ResolveOffset(left, right, out modLeft);
                double vOffset = ResolveOffset(top, bottom, out modTop);

                if (offsets.Count > count)
                {
                    OffsetData os = offsets[count];
                    os.ModifyLeftOffset = modLeft;
                    os.ModifyTopOffset = modTop;
                    os.OriginalHorizontalOffset = hOffset;
                    os.OriginalVerticalOffset = vOffset;
                }

                count++;
            }
        }


        #endregion // OnHostPreviewMouseUp

        #endregion // Host Event Handlers

        #region Private Helpers

        #region CalculateDragElementRect

        /// <summary>
        /// Returns a Rect which describes the bounds of the element being dragged.
        /// </summary>
        private Rect CalculateDragElementRect(ILocatable el, double newHorizOffset, double newVertOffset, bool modLeftOffset, bool modTopOffset)
        {

            //if(this.elementsBeingDragged.Count == 0)
            //    throw new InvalidOperationException("ElementBeingDragged is null.");

            //double xMin = 10000000.0;
            //double yMin = 10000000.0;
            //double xMax = -10000000.0;
            //double yMax = -10000000.0;
            //foreach (UIElement el in this.elementsBeingDragged)
            //{
            //    double elX = Canvas.GetLeft(el);
            //    double elY = Canvas.GetTop(el);
            //    xMin = Math.Min(xMin, elX);
            //    yMin = Math.Min(yMin, elY);
            //    xMax = Math.Max(xMax, elX + el.RenderSize.Width);
            //    yMax = Math.Max(yMax, elY + el.RenderSize.Height);
            //}

            //Size elemSize = new Size(xMax - xMin, yMax - yMin);

            //if (this.ElementBeingDragged == null)
            //    throw new InvalidOperationException("ElementBeingDragged is null.");

            //Size elemSize = this.elementsBeingDragged.RenderSize;

            //Size elemSize = el.RenderSize;
            Size elemSize = new Size(el.Width, el.Height);

            double x, y;

            if (modLeftOffset)
                x = newHorizOffset;
            else
                x = this.ActualWidth - newHorizOffset - elemSize.Width;

            if (modTopOffset)
                y = newVertOffset;
            else
                y = this.ActualHeight - newVertOffset - elemSize.Height;

            Point elemLoc = new Point(x, y);

            return new Rect(elemLoc, elemSize);
        }

        #endregion // CalculateDragElementRect

        #region ResolveOffset

        /// <summary>
        /// Determines one component of a UIElement's location 
        /// within a Canvas (either the horizontal or vertical offset).
        /// </summary>
        /// <param name="side1">
        /// The value of an offset relative to a default side of the 
        /// Canvas (i.e. top or left).
        /// </param>
        /// <param name="side2">
        /// The value of the offset relative to the other side of the 
        /// Canvas (i.e. bottom or right).
        /// </param>
        /// <param name="useSide1">
        /// Will be set to true if the returned value should be used 
        /// for the offset from the side represented by the 'side1' 
        /// parameter.  Otherwise, it will be set to false.
        /// </param>
        private static double ResolveOffset(double side1, double side2, out bool useSide1)
        {
            // If the Canvas.Left and Canvas.Right attached properties 
            // are specified for an element, the 'Left' value is honored.
            // The 'Top' value is honored if both Canvas.Top and 
            // Canvas.Bottom are set on the same element.  If one 
            // of those attached properties is not set on an element, 
            // the default value is Double.NaN.
            useSide1 = true;
            double result;
            if (Double.IsNaN(side1))
            {
                if (Double.IsNaN(side2))
                {
                    // Both sides have no value, so set the
                    // first side to a value of zero.
                    result = 0;
                }
                else
                {
                    result = side2;
                    useSide1 = false;
                }
            }
            else
            {
                result = side1;
            }
            return result;
        }

        #endregion // ResolveOffset

        #region UpdateZOrder

        /// <summary>
        /// Helper method used by the BringToFront and SendToBack methods.
        /// </summary>
        /// <param name="element">
        /// The element to bring to the front or send to the back.
        /// </param>
        /// <param name="bringToFront">
        /// Pass true if calling from BringToFront, else false.
        /// </param>
        private void UpdateZOrder(UIElement element, bool bringToFront)
        {
            #region Safety Check

            if (element == null)
                throw new ArgumentNullException("element");

            if (!base.Children.Contains(element))
                throw new ArgumentException("Must be a child element of the Canvas.", "element");

            #endregion // Safety Check

            #region Calculate Z-Indici And Offset

            // Determine the Z-Index for the target UIElement.
            int elementNewZIndex = -1;
            if (bringToFront)
            {
                foreach (UIElement elem in base.Children)
                    if (elem.Visibility != Visibility.Collapsed)
                        ++elementNewZIndex;
            }
            else
            {
                elementNewZIndex = 0;
            }

            // Determine if the other UIElements' Z-Index 
            // should be raised or lowered by one. 
            int offset = (elementNewZIndex == 0) ? +1 : -1;

            int elementCurrentZIndex = Canvas.GetZIndex(element);

            #endregion // Calculate Z-Indici And Offset

            #region Update Z-Indici

            // Update the Z-Index of every UIElement in the Canvas.
            foreach (UIElement childElement in base.Children)
            {
                if (childElement == element)
                    Canvas.SetZIndex(element, elementNewZIndex);
                else
                {
                    int zIndex = Canvas.GetZIndex(childElement);

                    // Only modify the z-index of an element if it is  
                    // in between the target element's old and new z-index.
                    if (bringToFront && elementCurrentZIndex < zIndex ||
                       !bringToFront && zIndex < elementCurrentZIndex)
                    {
                        Canvas.SetZIndex(childElement, zIndex + offset);
                    }
                }
            }

            #endregion // Update Z-Indici
        }

        #endregion // UpdateZOrder

        #endregion // Private Helpers

        /// <summary>
        /// The WorkspaceView (i.e. the owner of this DragCanvas object) calls
        /// this method to mark the current drag operation as being "over". This 
        /// happens when user activates another workspace.
        /// </summary>
        internal void CancelDragOperation()
        {
            this.dragState = DragState.None;
        }

        /// <summary>
        /// Find the user control of type 'testType' by traversing the tree.
        /// </summary>
        /// <returns></returns>
        public UIElement ElementClicked(DependencyObject depObj)  //, Type testType)
        {
            UIElement foundElement = null;

            //walk up the tree to see whether the element is part of a port
            //then get the port's parent object
            while (depObj != null)
            {
                // If the current object is a UIElement which is a child of the
                // Canvas, exit the loop and return it.
                UIElement elem = depObj as UIElement;

                if (elem != null)
                {
                    Type t = elem.GetType();

                    //only hit test against visible elements
                    //we want to avoid elements in other workspaces.
                    if (elem is ISelectable && elem.Visibility == System.Windows.Visibility.Visible)
                    {
                        foundElement = elem;
                        return foundElement;
                    }
                }

                // VisualTreeHelper works with objects of type Visual or Visual3D.
                // If the current object is not derived from Visual or Visual3D,
                // then use the LogicalTreeHelper to find the parent element.
                if (depObj is Visual)
                    depObj = VisualTreeHelper.GetParent(depObj);
                else
                    depObj = LogicalTreeHelper.GetParent(depObj);
            }

            return foundElement;
        }

        // Return the result of the hit test to the callback.
        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            if (!hitResultsList.Contains(result.VisualHit))
            {
                hitResultsList.Add(result.VisualHit);
            }

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }

        static bool HasParentType(Type t, Type testType)
        {
            while (t != typeof(object))
            {
                t = t.BaseType;
                if (t.Equals(testType))
                    return true;
            }
            return false;
        }
    }

    public class OffsetData
    {
        public double OriginalHorizontalOffset
        {
            get;
            set;
        }
        public double OriginalVerticalOffset
        {
            get;
            set;
        }
        public bool ModifyLeftOffset
        {
            get;
            set;
        }
        public bool ModifyTopOffset
        {
            get;
            set;
        }
        public double NewHorizontalOffset
        {
            get;
            set;
        }
        public double NewVerticalOffset
        {
            get;
            set;
        }
        public object Node
        {
            get;
            set;
        }

        public OffsetData(double hOffset, double vOffset, bool modifyLeftOffset, bool modifyTopOffset, object node)
        {
            this.OriginalHorizontalOffset = hOffset;
            this.OriginalVerticalOffset = vOffset;
            this.ModifyLeftOffset = modifyLeftOffset;
            this.ModifyTopOffset = modifyTopOffset;
            this.Node = node;
        }
    }

}