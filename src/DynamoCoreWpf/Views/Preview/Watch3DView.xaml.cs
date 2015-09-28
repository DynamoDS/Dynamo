﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Xml;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Core;
using Dynamo.Core.Threading;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.Wpf.Rendering;
using Dynamo.Wpf.ViewModels;
using Dynamo.Wpf.ViewModels.Watch3D;
using Dynamo.Wpf.Views.Preview;
using DynamoUtilities;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using Model3D = HelixToolkit.Wpf.SharpDX.Model3D;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;
using Point = System.Windows.Point;
using Quaternion = SharpDX.Quaternion;
using TextInfo = HelixToolkit.Wpf.SharpDX.TextInfo;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class Watch3DView : IWatch3DView
    {
        #region private members

        private Point rightMousePoint;

        #endregion

        #region public properties

        public Viewport3DX View
        {
            get { return watch_view; }
        }

        internal HelixWatch3DViewModel ViewModel { get; private set; }

        #endregion

        #region constructors

        public Watch3DView()
        {
            InitializeComponent();
            Loaded += ViewLoadedHandler;
            Unloaded += ViewUnloadedHandler;
        }

        #endregion

        #region event registration

        private void UnregisterEventHandlers()
        {
            UnregisterButtonHandlers();

            CompositionTarget.Rendering -= CompositionTargetRenderingHandler;

            ViewModel.RequestAttachToScene -= ViewModelRequestAttachToSceneHandler;
        }

        private void RegisterButtonHandlers()
        {
            MouseLeftButtonDown += MouseButtonIgnoreHandler;
            MouseLeftButtonUp += MouseButtonIgnoreHandler;
            MouseRightButtonUp += view_MouseRightButtonUp;
            PreviewMouseRightButtonDown += view_PreviewMouseRightButtonDown;
        }

        private void UnregisterButtonHandlers()
        {
            MouseLeftButtonDown -= MouseButtonIgnoreHandler;
            MouseLeftButtonUp -= MouseButtonIgnoreHandler;
            MouseRightButtonUp -= view_MouseRightButtonUp;
            PreviewMouseRightButtonDown -= view_PreviewMouseRightButtonDown;
        }

        #endregion

        #region event handlers

        private void ViewUnloadedHandler(object sender, RoutedEventArgs e)
        {
            UnregisterEventHandlers();
        }

        private void ViewLoadedHandler(object sender, RoutedEventArgs e)
        {
            ViewModel = DataContext as HelixWatch3DViewModel;

            CompositionTarget.Rendering += CompositionTargetRenderingHandler;

            RegisterButtonHandlers();

            if (ViewModel == null)
            {
                return;
            }

            ViewModel.RequestAttachToScene += ViewModelRequestAttachToSceneHandler;
            ViewModel.RequestCreateModels += RequestCreateModelsHandler;
            ViewModel.RequestViewRefresh += RequestViewRefreshHandler;
        }

        void RequestViewRefreshHandler()
        {
            View.InvalidateRender();
        }

        private void RequestCreateModelsHandler(IEnumerable<IRenderPackage> packages)
        {
            if (CheckAccess())
            {
                ViewModel.GenerateViewGeometryFromRenderPackagesAndRequestUpdate(packages);
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => ViewModel.GenerateViewGeometryFromRenderPackagesAndRequestUpdate(packages)));
            }
        }

        private void ViewModelRequestAttachToSceneHandler(Model3D model3D)
        {
            if (model3D is GeometryModel3D)
            {
                if (View != null && View.RenderHost != null && !model3D.IsAttached)
                {
                    model3D.Attach(View.RenderHost);
                }
            }
            else
            {
                //This is for Directional Light. When a watch is attached,
                //Directional light has to be attached one more time.
                if (!model3D.IsAttached && View != null && View.RenderHost != null)
                {
                    model3D.Attach(View.RenderHost);
                }
            }
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            if (xAdjust >= inputGrid.MinWidth)
            {
                Width = xAdjust;
            }

            if (yAdjust >= inputGrid.MinHeight)
            {
                Height = yAdjust;
            }
        }

        private void CompositionTargetRenderingHandler(object sender, EventArgs e)
        {
            var sceneBounds = watch_view.FindBounds();
            ViewModel.UpdateNearClipPlaneForSceneBounds(sceneBounds);

            ViewModel.ComputeFrameUpdate();
        }

        private void OnZoomToFitClickedHandler(object sender, RoutedEventArgs e)
        {
            watch_view.ZoomExtents();
        }

        private void MouseButtonIgnoreHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void view_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rightMousePoint = e.GetPosition(watch3D);
        }

        private void view_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if the mouse has moved, and this is a right click, we assume 
            // rotation. handle the event so we don't show the context menu
            // if the user wants the contextual menu they can click on the
            // node sidebar or top bar
            if (e.GetPosition(watch3D) != rightMousePoint)
            {
                e.Handled = true;
            }
        }

        #endregion

        #region interface methods

        public Ray3D GetClickRay(MouseEventArgs mouseButtonEventArgs)
        {
            var mousePos = mouseButtonEventArgs.GetPosition(this);

            return View.Point2DToRay3D(new Point(mousePos.X, mousePos.Y));
        }

        public void AddGeometryForRenderPackages(IEnumerable<IRenderPackage> packages)
        {
            ViewModel.OnRequestCreateModels(packages);
        }

        public void DeleteGeometryForIdentifier(string identifier, bool requestUpdate = true)
        {
            ViewModel.DeleteGeometryForIdentifier(identifier, requestUpdate);
        }

        #endregion
    }
}
