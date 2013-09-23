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

        private List<Point> GetFramePoints_LibraryItemPreview()
        {
            this.contentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            List<Point> points = new List<Point>();
            points.Add(new Point(this.contentContainer.DesiredSize.Width, 0));
            points.Add(new Point(7, 0));
            points.Add(new Point(7, this.contentContainer.DesiredSize.Height / 2 - 7));
            points.Add(new Point(0, this.contentContainer.DesiredSize.Height / 2));
            points.Add(new Point(7, this.contentContainer.DesiredSize.Height / 2 + 7));
            points.Add(new Point(7, this.contentContainer.DesiredSize.Height));
            points.Add(new Point(this.contentContainer.DesiredSize.Width, this.contentContainer.DesiredSize.Height));
            return points;
        }

        private List<Point> GetFramePoints_NodeTooltip()
        {
            this.contentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            List<Point> points = new List<Point>();
            points.Add(new Point(this.contentContainer.DesiredSize.Width, 0));
            points.Add(new Point(6, 0));
            points.Add(new Point(6, this.contentContainer.DesiredSize.Height / 2 - 6));
            points.Add(new Point(0, this.contentContainer.DesiredSize.Height / 2));
            points.Add(new Point(6, this.contentContainer.DesiredSize.Height / 2 + 6));
            points.Add(new Point(6, this.contentContainer.DesiredSize.Height));
            points.Add(new Point(this.contentContainer.DesiredSize.Width, this.contentContainer.DesiredSize.Height));
            return points;
        }

        private List<Point> GetFramePoints_Error()
        {
            List<Point> points = new List<Point>();
            return points;
        }

        private void UpdateShape()
        {
            if (backgroundPolygon.Points != null && backgroundPolygon.Points.Count > 0)
                backgroundPolygon.Points.Clear();
            List<Point> framePoints = new List<Point>();

            switch (_viewModel.PopupStyle)
            {
                case PopupViewModel.Style.LibraryItemPreview:
                    framePoints = GetFramePoints_LibraryItemPreview();
                    break;
                case PopupViewModel.Style.NodeTooltip:
                    framePoints = GetFramePoints_NodeTooltip();
                    break;
                case PopupViewModel.Style.Error:
                    framePoints = GetFramePoints_Error();
                    break;
                case PopupViewModel.Style.None:
                    break;
            }

            foreach (Point point in framePoints)
                backgroundPolygon.Points.Add(point);
        }

        private void OnTextChange(object sender, TextChangedEventArgs e)
        {
            UpdateShape();

            Thickness margin = _viewModel.Margin;
            margin.Top -= mainGrid.ActualHeight / 2;
            _viewModel.Margin = margin;
        }
    }
}