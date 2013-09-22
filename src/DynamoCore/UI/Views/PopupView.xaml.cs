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
using PopupViewModel = Dynamo.ViewModels.PopupViewModel;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for PreviewPopup.xaml
    /// </summary>
    public partial class PopupView : UserControl
    {
        private PopupViewModel _viewModel;

        public PopupView(PopupViewModel viewmodel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewmodel;
        }

        public void UpdatePopupWindow()
        {
            _viewModel.UpdatePopupCommand.Execute(null);
        }

        public void FadeInPopupWindow()
        {
            _viewModel.FadeInCommand.Execute(null);
        }

        public void FadeOutPopupWindow()
        {
            _viewModel.FadeOutCommand.Execute(null);
        }

        //#region old
        //#region Class Properties

        //private DoubleAnimation fadeOutAnimation = new DoubleAnimation();
        //DoubleAnimation fadeInAnimation = new DoubleAnimation();

        //#endregion

        //#region Public Methods

        //public void UpdatePopupWindow(Point connectingPoint, string text)
        //{
        //    RefreshContent(text);
        //    UpdateShape();
        //    UpdatePosition(connectingPoint);
        //}

        //public void FadeOutPopupWindow()
        //{
        //    this.mainGrid.BeginAnimation(OpacityProperty, fadeOutAnimation);
        //}

        //public void FadeInPopupWindow()
        //{
        //    this.popupWindow.IsOpen = true;
        //    this.mainGrid.BeginAnimation(OpacityProperty, fadeInAnimation);
        //}

        //#endregion

        //#region Private Helper Methods

        //private void RefreshContent(string text)
        //{
        //    this.contentContainer.Children.Clear();
        //    TextBox content = GetStyledTextBox(text);
        //    this.contentContainer.Children.Add(content);
        //}

        //private void UpdatePosition(Point connectingPoint)
        //{
        //    switch (this.popupStyle)
        //    {
        //        case PopupStyle.LibraryItemPreview:
        //            this.contentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //            this.popupWindow.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
        //            this.popupWindow.VerticalOffset = connectingPoint.Y - (this.contentContainer.DesiredSize.Height / 2);
        //            break;
        //        case PopupStyle.NodeTooltip:
        //            this.contentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //            this.popupWindow.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
        //            this.popupWindow.VerticalOffset = connectingPoint.Y - (this.contentContainer.DesiredSize.Height / 2);
        //            break;
        //        case PopupStyle.Error:
        //            break;
        //        case PopupStyle.None:
        //            break;
        //    }
        //}

        //private List<Point> GetFramePoints_LibraryItemPreview()
        //{
        //    this.contentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //    List<Point> points = new List<Point>();
        //    points.Add(new Point(this.contentContainer.DesiredSize.Width, 0));
        //    points.Add(new Point(7, 0));
        //    points.Add(new Point(7, this.contentContainer.DesiredSize.Height / 2 - 7));
        //    points.Add(new Point(0, this.contentContainer.DesiredSize.Height / 2));
        //    points.Add(new Point(7, this.contentContainer.DesiredSize.Height / 2 + 7));
        //    points.Add(new Point(7, this.contentContainer.DesiredSize.Height));
        //    points.Add(new Point(this.contentContainer.DesiredSize.Width, this.contentContainer.DesiredSize.Height));
        //    return points;
        //}

        //private List<Point> GetFramePoints_NodeTooltip()
        //{
        //    this.contentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //    List<Point> points = new List<Point>();
        //    points.Add(new Point(this.contentContainer.DesiredSize.Width, 0));
        //    points.Add(new Point(6, 0));
        //    points.Add(new Point(6, this.contentContainer.DesiredSize.Height / 2 - 6));
        //    points.Add(new Point(0, this.contentContainer.DesiredSize.Height / 2));
        //    points.Add(new Point(6, this.contentContainer.DesiredSize.Height / 2 + 6));
        //    points.Add(new Point(6, this.contentContainer.DesiredSize.Height));
        //    points.Add(new Point(this.contentContainer.DesiredSize.Width, this.contentContainer.DesiredSize.Height));
        //    return points;
        //}

        //private List<Point> GetFramePoints_Error()
        //{
        //    List<Point> points = new List<Point>();
        //    return points;
        //}

        //private void UpdateShape()
        //{
        //    this.backgroundPolygon.Points.Clear();
        //    List<Point> framePoints = new List<Point>();

        //    switch (this.popupStyle)
        //    {
        //        case PopupStyle.LibraryItemPreview:
        //            framePoints = GetFramePoints_LibraryItemPreview();
        //            break;
        //        case PopupStyle.NodeTooltip:
        //            framePoints = GetFramePoints_NodeTooltip();
        //            break;
        //        case PopupStyle.Error:
        //            framePoints = GetFramePoints_Error();
        //            break;
        //        case PopupStyle.None:
        //            break;
        //    }

        //    foreach (Point point in framePoints)
        //        backgroundPolygon.Points.Add(point);
        //}

        //private void UpdateBackgroundStyle(PopupViewModel.PopupStyle style)
        //{
        //    this.popupStyle = style;
        //    switch (this.popupStyle)
        //    {
        //        case PopupStyle.LibraryItemPreview:
        //            SetPopupBackgroundStyle_LibraryItemPreview();
        //            break;
        //        case PopupStyle.NodeTooltip:
        //            SetPopupBackgroundStyle_NodeTooltip();
        //            break;
        //        case PopupStyle.Error:
        //            SetPopupBackgroundStyle_Error();
        //            break;
        //        case PopupStyle.None:
        //            throw new ArgumentException("PopupWindow didn't have a style (456B24E0F400)");
        //    }
        //}

        //private void SetPopupBackgroundStyle_LibraryItemPreview()
        //{
        //    this.backgroundPolygon.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        //    this.backgroundPolygon.StrokeThickness = 1;
        //    this.backgroundPolygon.Stroke = new SolidColorBrush(Color.FromRgb(10, 93, 30));
        //    this.contentContainer.MaxWidth = 400;
        //}

        //private void SetPopupBackgroundStyle_NodeTooltip()
        //{
        //    this.backgroundPolygon.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        //    this.backgroundPolygon.StrokeThickness = 1;
        //    this.backgroundPolygon.Stroke = new SolidColorBrush(Color.FromRgb(165, 209, 226));
        //    this.contentContainer.MaxWidth = 200;
        //}

        //private void SetPopupBackgroundStyle_Error()
        //{

        //}

        //private TextBox GetStyledTextBox(string text)
        //{
        //    switch (this.popupStyle)
        //    {
        //        case PopupStyle.LibraryItemPreview:
        //            return GetTextBox_LibraryItemPreviewStyle(text);
        //        case PopupStyle.NodeTooltip:
        //            return GetTextBox_NodeTooltipStyle(text);
        //    }
        //    return null;
        //}

        //private TextBox GetTextBox_LibraryItemPreviewStyle(string text)
        //{
        //    TextBox textbox = new TextBox();
        //    textbox.TextWrapping = TextWrapping.Wrap;
        //    textbox.Text = text;
        //    textbox.IsReadOnly = true;
        //    textbox.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
        //    textbox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
        //    textbox.BorderThickness = new Thickness(0);
        //    textbox.Background = Brushes.Transparent;
        //    textbox.FontSize = 13;
        //    textbox.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
        //    return textbox;
        //}

        //private TextBox GetTextBox_NodeTooltipStyle(string text)
        //{
        //    TextBox textbox = new TextBox();
        //    textbox.TextWrapping = TextWrapping.Wrap;
        //    textbox.Text = text;
        //    textbox.IsReadOnly = true;
        //    textbox.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
        //    textbox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
        //    textbox.BorderThickness = new Thickness(0);
        //    textbox.Background = Brushes.Transparent;
        //    textbox.FontSize = 12;
        //    textbox.FontWeight = FontWeights.Light;
        //    textbox.Foreground = new SolidColorBrush(Color.FromRgb(98, 140, 153));
        //    return textbox;
        //}

        //private void SetupFadeSetting()
        //{
        //    fadeOutAnimation.Completed += fadeOutAnimation_Completed;
        //    fadeOutAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
        //    fadeOutAnimation.To = 0;

        //    fadeInAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
        //    fadeInAnimation.To = 0.96;
        //    fadeInAnimation.Completed += fadeInAnimation_Completed;
        //}

        //private void fadeOutAnimation_Completed(object sender, EventArgs e)
        //{
        //}

        //private void fadeInAnimation_Completed(object sender, EventArgs e)
        //{
        //}

        //private void PopupWindow_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    FadeInPopupWindow();
        //}

        //private void PopupWindow_Mouseleave(object sender, MouseEventArgs e)
        //{
        //    FadeOutPopupWindow();
        //}

        //#endregion
        //#endregion

    }
}