using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Dynamo.Controls;
using System.Windows.Threading;

namespace Dynamo.Controls
{
   /// <summary>
   /// A Canvas which manages dragging of the UIElements it contains.  
   /// </summary>
   public class DragCanvas : Canvas
   {
      #region Data

      // Stores a reference to the UIElement currently being dragged by the user.
      public List<UIElement> elementsBeingDragged = new List<UIElement>();
      public List<OffsetData> offsets = new List<OffsetData>();

      // Keeps track of where the mouse cursor was when a drag operation began.		
      private Point origCursorLocation;

      // True if a drag operation is underway, else false.
      public bool isDragInProgress;

      //true if user is making a connection between elements
      public bool isConnecting;

      //true if we're ignoring clicks
      public bool ignoreClick;

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

      /// <summary>
      /// Returns the UIElement currently being dragged, or null.
      /// </summary>
      /// <remarks>
      /// Note to inheritors: This property exposes a protected 
      /// setter which should be used to modify the drag element.
      /// </remarks>
      //public UIElement ElementBeingDragged
      //{
      //   get
      //   {
      //      if (!this.AllowDragging)
      //         return null;
      //      else
      //         return this.elementBeingDragged;
      //   }
      //   protected set
      //   {
      //      if (this.elementBeingDragged != null)
      //         this.elementBeingDragged.ReleaseMouseCapture();

      //      if (!this.AllowDragging)
      //         this.elementBeingDragged = null;
      //      else
      //      {
      //         if (DragCanvas.GetCanBeDragged(value))
      //         {
      //            this.elementBeingDragged = value;
      //            this.elementBeingDragged.CaptureMouse();
      //         }
      //         else
      //            this.elementBeingDragged = null;
      //      }
      //   }
      //}

      public List<UIElement> ElementsBeingDragged
      {
          get { return elementsBeingDragged; }
          set
          {
              elementsBeingDragged = value;
          }
      }
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

         if (!isConnecting)
         {
            base.OnMouseLeftButtonDown(e);

            this.isDragInProgress = false;

            // Cache the mouse cursor location.
            this.origCursorLocation = e.GetPosition(this);

            if (this.elementsBeingDragged.Count == 0)
                return;

            foreach (UIElement el in elementsBeingDragged)
            {

                // Get the element's offsets from the four sides of the Canvas.
                double left = Canvas.GetLeft(el);
                double right = Canvas.GetRight(el);
                double top = Canvas.GetTop(el);
                double bottom = Canvas.GetBottom(el);

                // Calculate the offset deltas and determine for which sides
                // of the Canvas to adjust the offsets.
                bool modLeft = false;
                bool modTop = false;
                double hOffset = ResolveOffset(left, right, out modLeft);
                double vOffset = ResolveOffset(top, bottom, out modTop);
                OffsetData os = new OffsetData(hOffset, vOffset, modLeft, modTop);

                offsets.Add(os);

            }

            e.Handled = true;

            this.isDragInProgress = true;
         }
      }

      #endregion // OnPreviewMouseLeftButtonDown

      #region OnPreviewMouseMove

      protected override void OnPreviewMouseMove(MouseEventArgs e)
      {
         base.OnPreviewMouseMove(e);

         if (this.elementsBeingDragged.Count == 0 || !this.isDragInProgress)
             return;

         // Get the position of the mouse cursor, relative to the Canvas.
         Point cursorLocation = e.GetPosition(this);

         #region Calculate Offsets

         int count = 0;
         foreach (UIElement el in this.elementsBeingDragged)
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

             Debug.WriteLine(string.Format("New h:{0} v:{1}", od.NewHorizontalOffset, od.NewVerticalOffset));
             count++;
         }

         #endregion // Calculate Offsets

         if (!this.AllowDragOutOfView)
         {
            #region Verify Drag Element Location

             count = 0;
             foreach (UIElement el in this.elementsBeingDragged)
             {
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
                 foreach (UIElement el in this.elementsBeingDragged)
                 {
                     OffsetData od = offsets[count];
 
                     if (od.ModifyLeftOffset)
                         Canvas.SetLeft(el, od.NewHorizontalOffset);
                     else
                         Canvas.SetRight(el, od.NewHorizontalOffset);

                     if (od.ModifyTopOffset)
                         Canvas.SetTop(el, od.NewVerticalOffset);
                     else
                         Canvas.SetBottom(el, od.NewVerticalOffset);

                     count++;
                 }
               }
            ),DispatcherPriority.Render, null );
         #endregion // Move Drag Element

      }

      #endregion // OnPreviewMouseMove

      #region OnHostPreviewMouseUp

      protected override void OnMouseUp(MouseButtonEventArgs e)
      {
         base.OnMouseUp(e);

         // Reset the field whether the left or right mouse button was 
         // released, in case a context menu was opened on the drag element.
         //this.ElementBeingDragged = null;

         this.isDragInProgress = false;
      }

      public void ClearDragElements()
      {
          this.elementsBeingDragged.Clear();
          this.offsets.Clear();
      }
      #endregion // OnHostPreviewMouseUp

      #endregion // Host Event Handlers

      #region Private Helpers

      #region CalculateDragElementRect

      /// <summary>
      /// Returns a Rect which describes the bounds of the element being dragged.
      /// </summary>
      private Rect CalculateDragElementRect(UIElement el, double newHorizOffset, double newVertOffset, bool modLeftOffset, bool modTopOffset)
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

         Size elemSize = el.RenderSize;

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

       public OffsetData(double hOffset, double vOffset, bool modifyLeftOffset, bool modifyTopOffset)
       {
           this.OriginalHorizontalOffset = hOffset;
           this.OriginalVerticalOffset = vOffset;
           this.ModifyLeftOffset = modifyLeftOffset;
           this.ModifyTopOffset = modifyTopOffset;
       }
   }
}