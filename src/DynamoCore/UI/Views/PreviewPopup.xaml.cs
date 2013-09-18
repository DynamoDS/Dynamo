using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for PreviewPopup.xaml
    /// </summary>
    public partial class PreviewPopup : UserControl
    {
        public enum PopupStyle
        {
            LibraryItemPreview,
            NodeToolTip,
            Error,
            None
        }

        #region Public Methods

        public PreviewPopup(PopupStyle style)
        {
            InitializeComponent();
            UpdateStyle(style);
        }

        public void UpdatePopupWindow(Point connectingPoint, string text)
        {
            RefreshContent(text);
            UpdateShape();
            UpdatePosition(connectingPoint);
        }

        public void FadeOutPopupWindow()
        {
            DoubleAnimation fadeOutAnimation = new DoubleAnimation();
            fadeOutAnimation.Completed += fadeOutAnimation_Completed;
            fadeOutAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            fadeOutAnimation.From = 0.96;
            fadeOutAnimation.To = 0;
            this.mainGrid.BeginAnimation(OpacityProperty, fadeOutAnimation);
        }

        public void FaceInPopupWindow()
        {
            this.popupWindow.IsOpen = true;
            DoubleAnimation fadeInAnimation = new DoubleAnimation();
            fadeInAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            fadeInAnimation.From = 0;
            fadeInAnimation.To = 0.96;
            this.mainGrid.BeginAnimation(OpacityProperty, fadeInAnimation);
        }

        #endregion

        #region Class Properties

        public PopupStyle Style { get; private set; }

        #endregion

        #region Private Helper Methods

        private void RefreshContent(string text)
        {
            this.contentContainer.Children.Clear();
            TextBox content = GetStyledTextBox(text);
            this.contentContainer.Children.Add(content);
        }

        private void UpdatePosition(Point connectingPoint)
        {
            this.contentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            this.popupWindow.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
            this.popupWindow.VerticalOffset = connectingPoint.Y - (this.contentContainer.DesiredSize.Height / 2);
        }

        private void UpdateShape()
        {
            this.backgroundPolygon.Points.Clear();
            this.contentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Point topRightConner = new Point(this.contentContainer.DesiredSize.Width, 0);
            Point topLeftConner = new Point(7, 0);
            Point bottomLeftConner = new Point(7, this.contentContainer.DesiredSize.Height);
            Point bottomRightConner = new Point(this.contentContainer.DesiredSize.Width, this.contentContainer.DesiredSize.Height);
            Point triagleTop = new Point(7, this.contentContainer.DesiredSize.Height / 2 - 3.5);
            Point triagleBottom = new Point(7, this.contentContainer.DesiredSize.Height / 2 + 3.5);
            Point triagleLeft = new Point(0, this.contentContainer.DesiredSize.Height / 2);

            backgroundPolygon.Points.Add(topRightConner);
            backgroundPolygon.Points.Add(topLeftConner);
            backgroundPolygon.Points.Add(triagleTop);
            backgroundPolygon.Points.Add(triagleLeft);
            backgroundPolygon.Points.Add(triagleBottom);
            backgroundPolygon.Points.Add(bottomLeftConner);
            backgroundPolygon.Points.Add(bottomRightConner);
        }

        private void UpdateStyle(PopupStyle style)
        {
            this.Style = style;
            switch (style)
            {
                case PopupStyle.LibraryItemPreview:
                    SetPopupStyle_LibraryItemPreview();
                    break;
                case PopupStyle.NodeToolTip:
                    SetPopupShapeStyle_NodeTooltip();
                    break;
                case PopupStyle.Error:
                    SetPopupShapeStyle_Error();
                    break;
                case PopupStyle.None:
                    throw new ArgumentException("PopupWindow didn't have a style (456B24E0F400)");
            }
        }

        private void SetPopupStyle_LibraryItemPreview()
        {
            this.backgroundPolygon.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            this.backgroundPolygon.StrokeThickness = 1;
            this.backgroundPolygon.Stroke = new SolidColorBrush(Color.FromRgb(10, 93, 30));
        }

        private void SetPopupShapeStyle_NodeTooltip()
        {

        }

        private void SetPopupShapeStyle_Error()
        {

        }

        private TextBox GetStyledTextBox(string text)
        {
            TextBox textbox = new TextBox();
            textbox.Text = text;
            textbox.IsReadOnly = true;
            textbox.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            textbox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            textbox.BorderThickness = new Thickness(0);
            textbox.Background = Brushes.Transparent;
            textbox.FontSize = 13;
            textbox.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            return textbox;
        }

        private void fadeOutAnimation_Completed(object sender, EventArgs e)
        {
            //this.popupWindow.IsOpen = false;
        }

        #endregion
    }
}