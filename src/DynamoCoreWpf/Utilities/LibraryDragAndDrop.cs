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
        private Point startPosition;
        private NodeSearchElementViewModel nodeViewModel;

        /// <summary>
        /// Indicates whether item is dragging or not, so that there won't be more than one DoDragDrop event.
        /// </summary>
        private bool isDragging;

        internal void HandleMouseDown(Point position, NodeSearchElementViewModel node)
        {
            startPosition = position;
            nodeViewModel = node;
        }

        internal void Clear()
        {
            startPosition = new Point();
            nodeViewModel = null;
        } 

        internal void HandleMouseMove(DependencyObject sender, Point currentPosition)
        {
            if (isDragging || nodeViewModel == null)
                return;

            // If item was dragged enough, then fire DoDragDrop. 
            // Otherwise it means user click on item and there is no need to fire DoDragDrop.
            var deltaX = System.Math.Abs(currentPosition.X - startPosition.X);
            if (deltaX < SystemParameters.MinimumHorizontalDragDistance)
            {
               return;
            }

            var deltaY = System.Math.Abs(currentPosition.Y - startPosition.Y);
            if (deltaY < SystemParameters.MinimumVerticalDragDistance)
            {                
                return;
            }

            StartDrag(sender, nodeViewModel);

        }

        private void StartDrag(DependencyObject sender, NodeSearchElementViewModel node)
        {
            isDragging = true;
            DragDrop.DoDragDrop(sender, new DragDropNodeSearchElementInfo(node.Model), DragDropEffects.Copy);
            // reset when dragging ends
            Clear();
            isDragging = false;
        }
    }
}
