using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Dynamo.Wpf.UI
{
    /// <summary>
    /// Adorner to visualize dragged content
    /// </summary>
    class DraggedAdorner: Adorner
    {
        // single visual child of the adorner
        private readonly ContentPresenter contentPresenter;
        private readonly AdornerLayer adornerLayer;
        private double left;
        private double top;

        /// <summary>
        /// Initializes a new instance of the DraggedAdorner class.
        /// </summary>
        /// <param name="dragDropData">Dragged content data</param>
        /// <param name="dragDropTemplate">Template to visualize dragged content</param>
        /// <param name="adornedElement">The element to bind the adorner to</param>
        /// <param name="layer">AdornerLayer element to attach the adorner</param>
        public DraggedAdorner(object dragDropData, DataTemplate dragDropTemplate, 
            UIElement adornedElement, AdornerLayer layer)
            : base(adornedElement)
        {
            adornerLayer = layer;
            contentPresenter = new ContentPresenter
            {
                Content = dragDropData,
                ContentTemplate = dragDropTemplate,
                Opacity = 0.7
            };

            adornerLayer.Add(this);
        }

        /// <summary>
        /// Set position of the dragged data
        /// </summary>
        /// <param name="left">X coordinate</param>
        /// <param name="top">Y coordianate</param>
        /// <param name="bounds">Visible rectangle of dragged content</param>
        public void SetPosition(double left, double top, Rect bounds)
        {
            this.left = left;
            this.top = top;
            contentPresenter.Clip = new RectangleGeometry(bounds);
            if (adornerLayer != null)
            {
                adornerLayer.Update(AdornedElement);
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            contentPresenter.Measure(constraint);
            return contentPresenter.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return contentPresenter;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(left, top));

            return result;
        }

        public void Detach()
        {
            adornerLayer.Remove(this);
        }
    }
}
