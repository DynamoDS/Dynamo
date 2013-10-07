//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Watch 3D")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Shows a dynamic preview of geometry.")]
    [AlsoKnownAs("Dynamo.Nodes.dyn3DPreview", "Dynamo.Nodes.3DPreview")]
    public class Watch3D : NodeWithOneOutput
    {
        private bool _requiresRedraw = false;
        private bool _isRendering = false;
        WatchView _watchView;

        public DelegateCommand GetBranchVisualizationCommand { get; set; }

        public event EventHandler WatchResultsReadyToVisualize;

        public WatchView View
        {
            get { return _watchView; }
        }

        public Watch3D()
        {
            InPortData.Add(new PortData("", "Incoming geometry objects.", typeof(object)));
            OutPortData.Add(new PortData("", "Watch contents, passed through", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            GetBranchVisualizationCommand = new DelegateCommand(GetBranchVisualization, CanGetBranchVisualization);
        }

        public void GetBranchVisualization(object parameters)
        {
            var rd = dynSettings.Controller.VisualizationManager.RenderUpstream(this);
            OnWatchResultsReadyToVisualize(this, new VisualizationEventArgs(rd));
        }

        public bool CanGetBranchVisualization(object parameter)
        {
            return true;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            _requiresRedraw = true;

            return input;
        }

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            MenuItem mi = new MenuItem();
            mi.Header = "Zoom to Fit";
            mi.Click += new RoutedEventHandler(mi_Click);

            nodeUI.MainContextMenu.Items.Add(mi);

            //take out the left and right margins and make this so it's not so wide
            //NodeUI.inputGrid.Margin = new Thickness(10, 10, 10, 10);

            //add a 3D viewport to the input grid
            //http://helixtoolkit.codeplex.com/wikipage?title=HelixViewport3D&referringTitle=Documentation
            _watchView = new WatchView();
            _watchView.DataContext = this;

            _watchView.Width = 400;
            _watchView.Height = 300;

            System.Windows.Shapes.Rectangle backgroundRect = new System.Windows.Shapes.Rectangle();
            backgroundRect.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            backgroundRect.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            backgroundRect.IsHitTestVisible = false;
            var bc = new BrushConverter();
            var strokeBrush = (Brush)bc.ConvertFrom("#313131");
            backgroundRect.Stroke = strokeBrush;
            backgroundRect.StrokeThickness = 1;
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 216));
            backgroundRect.Fill = backgroundBrush;

            nodeUI.grid.Children.Add(backgroundRect);
            nodeUI.grid.Children.Add(_watchView);
            backgroundRect.SetValue(Grid.RowProperty, 2);
            backgroundRect.SetValue(Grid.ColumnSpanProperty, 3);
            _watchView.SetValue(Grid.RowProperty, 2);
            _watchView.SetValue(Grid.ColumnSpanProperty, 3);
            _watchView.Margin = new Thickness(5, 0, 5, 5);
            backgroundRect.Margin = new Thickness(5, 0, 5, 5);
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_isRendering)
                return;

            if (!_requiresRedraw)
                return;

            _isRendering = true;
            _requiresRedraw = false;
            _isRendering = false;
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            _watchView.watch_view.ZoomExtents();
        }

        public void OnWatchResultsReadyToVisualize(object sender, VisualizationEventArgs e)
        {
            if (WatchResultsReadyToVisualize != null)
                WatchResultsReadyToVisualize(sender, e);
        }
    }
}
