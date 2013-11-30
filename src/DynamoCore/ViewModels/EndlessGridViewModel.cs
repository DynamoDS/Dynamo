using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows;
using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{

    /// <summary>
    /// In a nutshell, EndlessGrid is the grid lines in canvas. It is one of the key features of 
    /// infinite canvas.
    /// 
    /// (a) It is to catch the mouse events that happened in the DragCanvas and subsequently bubble
    ///     them up the event tree to be handled.
    /// 
    /// (b) EndlessGrid follows the viewport by resetting its location everytime the canvas is being 
    ///     panned a certain distance. This creates the impression that the grid lines are
    ///     infinitely extended but in actual fact it is just being resetted back to its previous
    ///     location seamlessly. This is achieve by offsetting per GridSize and not pixel.
    ///     
    /// With these two properties, it supports mouse click outside region of DragCanvas. It also
    /// rely on DragCanvas Property of ClipToBound to be false. Allowing object to be visible
    /// even when placed outside DragCanvas.
    ///     
    /// </summary>
    partial class EndlessGridViewModel : ViewModelBase
    {
        private ObservableCollection<Line> gridLines;
        public ObservableCollection<Line> GridLines
        {
            get { return gridLines; }
            set { gridLines = value; RaisePropertyChanged("GridLines"); }
        }

        private double left;
        public double Left
        {
            get { return left; }
            set { left = value; RaisePropertyChanged("Left"); }
        }

        private double top;
        public double Top
        {
            get { return top; }
            set { top = value; RaisePropertyChanged("Top"); }
        }

        public double ZIndex
        {
            get { return 0; }
        }

        private Transform transform;
        public Transform Transform
        {
            get
            {
                if (transform == null)
                    transform = new TranslateTransform();
                return transform;
            }
            set
            {
                transform = value;
            }

        }

        public double width;
        public double Width
        {
            get { return width; }
            set { width = value; RaisePropertyChanged("Width"); }
        }

        private double height;
        public double Height
        {
            get { return height; }
            set { height = value; RaisePropertyChanged("Height"); }
        }

        public double WorkspaceX
        {
            get { return workspaceVM.Model.X; }
        }

        public double WorkspaceY
        {
            get { return workspaceVM.Model.Y; }
        }

        public double WorkspaceWidth
        {
            get { return dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth; }
        }

        public double WorkspaceHeight
        {
            get { return dynSettings.Controller.DynamoViewModel.WorkspaceActualHeight; }
        }

        public double WorkspaceZoom
        {
            get { return workspaceVM._model.Zoom; }
        }

        public Color gridLineColor
        {
            get { return Configurations.GridLineColor; }
        }

        public bool FullscreenWatchShowing
        {
            get { return dynSettings.Controller.DynamoViewModel.FullscreenWatchShowing; }
        }


        #region Private
        private WorkspaceViewModel workspaceVM;

        /// <summary>
        /// Extra width or height for EndlessGrid to provide the illusion of infinite canvas grid line
        /// </summary>
        private double requiredSpareGridSize;

        /// <summary>
        /// GridSpacing that is relative to current workspace zoom
        /// </summary>
        private double gridSpacingScaled;

        #endregion

        public EndlessGridViewModel(WorkspaceViewModel workspaceVM)
        {
            this.workspaceVM = workspaceVM;
        }

        /// <summary>
        /// When EndlessGridView is loaded, it will trigger this method to start working
        /// </summary>
        internal void InitializeOnce()
        {
            // Subscribing to properties changes, keeping up to date
            this.workspaceVM.Model.PropertyChanged += WorkspaceModel_PropertyChanged;
            dynSettings.Controller.DynamoViewModel.PropertyChanged += DynamoViewModel_PropertyChanged;

            // Render EndlessGrid for the first time
            RecalculateLeft();
            RecalculateTop();
            RecalculateSize();
        }

        void DynamoViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "WorkspaceActualSize":
                    RecalculateSize();
                    break;
                case "FullscreenWatchShowing":
                    RaisePropertyChanged("FullscreenWatchShowing");
                    break;
            }
        }

        void WorkspaceModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "X":
                    RecalculateLeft();
                    break;

                case "Y":
                    RecalculateTop();
                    break;

                case "Zoom":
                    RecalculateZoom();
                    break;
            }
        }

        protected void RecalculateLeft()
        {
            Left = -requiredSpareGridSize - ((int)(WorkspaceX / gridSpacingScaled)) * Configurations.GridSpacing;
        }

        protected void RecalculateTop()
        {
            Top = -requiredSpareGridSize - ((int)(WorkspaceY / gridSpacingScaled)) * Configurations.GridSpacing;
        }

        protected void RecalculateZoom()
        {
            gridSpacingScaled = Configurations.GridSpacing * WorkspaceZoom;
        }

        protected void RecalculateSize()
        {
            RecalculateZoom();

            // Calculate the required spare grid size for panning certain distance
            requiredSpareGridSize = (int)Math.Ceiling(Configurations.GridSpacing * 2 / WorkspaceModel.ZOOM_MINIMUM);

            this.Width = this.WorkspaceWidth / WorkspaceModel.ZOOM_MINIMUM + requiredSpareGridSize * 2;
            this.Height = this.WorkspaceHeight / WorkspaceModel.ZOOM_MINIMUM + requiredSpareGridSize * 2;

            RecreateGridLines();
        }

        #region Helper Methods
        /// <summary>
        /// Remove existing grid line and redraw it based on the width and height.
        /// Output grid lines will be stored in GridLines
        /// </summary>
        private void RecreateGridLines()
        {
            ObservableCollection<Line> collection = new ObservableCollection<Line>();
            
            // Draw Vertical Grid Lines
            for (double i = 0; i < this.Width; i += Configurations.GridSpacing)
            {
                var xLine = new Line();
                xLine.Stroke = new SolidColorBrush(gridLineColor);
                xLine.StrokeThickness = Configurations.GridThickness;
                xLine.X1 = i;
                xLine.Y1 = 0;
                xLine.X2 = i;
                xLine.Y2 = this.Height;
                xLine.HorizontalAlignment = HorizontalAlignment.Left;
                xLine.VerticalAlignment = VerticalAlignment.Center;
                collection.Add(xLine);
            }

            // Draw Horizontal Grid Lines
            for (double i = 0; i < this.Height; i += Configurations.GridSpacing)
            {
                var yLine = new Line();
                yLine.Stroke = new SolidColorBrush(gridLineColor);
                yLine.StrokeThickness = Configurations.GridThickness;
                yLine.X1 = 0;
                yLine.Y1 = i;
                yLine.X2 = this.Width;
                yLine.Y2 = i;
                yLine.HorizontalAlignment = HorizontalAlignment.Left;
                yLine.VerticalAlignment = VerticalAlignment.Center;
                collection.Add(yLine);
            }

            GridLines = collection;
        }
        #endregion
    }
}
