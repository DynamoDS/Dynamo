using Dynamo.Search.SearchElements;
using Dynamo.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Dynamo.Wpf.Utilities
{
    public class LibraryDragAndDrop
    {
        /// <summary>
        /// Position of mouse, when user clicks on button.
        /// </summary>
        public Point StartPosition;

        /// <summary>
        /// Indicates whether item is dragging or not, so that there won't be more than one DoDragDrop event.
        /// </summary>
        private bool IsDragging;

        public void MouseMove(FrameworkElement sender, Point currentPosition, NodeSearchElementViewModel node)
        {
            // If item was dragged enough, then fire DoDragDrop. 
            // Otherwise it means user click on item and there is no need to fire DoDragDrop.
            if ((System.Math.Abs(currentPosition.X - StartPosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                System.Math.Abs(currentPosition.Y - StartPosition.Y) > SystemParameters.MinimumVerticalDragDistance) &&
                !IsDragging)
            {
                StartDrag(sender, node);
            }

        }

        private void StartDrag(FrameworkElement sender, NodeSearchElementViewModel node)
        {
            IsDragging = true;
            DragDrop.DoDragDrop(sender, new DragDropNodeSearchElementInfo(node.Model), DragDropEffects.Copy);
            IsDragging = false;
        }
    }
}
