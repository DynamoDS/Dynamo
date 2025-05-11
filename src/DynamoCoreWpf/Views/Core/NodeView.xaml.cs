using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Controls;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using DynCmd = Dynamo.Models.DynamoModel;
using Thickness = System.Windows.Thickness;


namespace Dynamo.Controls
{
    public partial class NodeView : IViewModelView<NodeViewModel>
    {
        public delegate void SetToolTipDelegate(string message);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);
        private NodeViewModel viewModel = null;
        private PreviewControl previewControl = null;
        private const int previewDelay = 1000;

        /// <summary>
        /// If false - hides preview control until it will be explicitly shown.
        /// If true -preview control is shown and hidden on mouse enter/leave events.
        /// </summary>
        private bool previewEnabled = true;

        /// <summary>
        /// Old ZIndex of node. It's set, when mouse leaves node.
        /// </summary>
        private int oldZIndex;

        private bool nodeWasClicked;

        public NodeView TopControl
        {
            get { return topControl; }
        }

        public Grid ContentGrid
        {
            get { return inputGrid; }
        }

        public NodeViewModel ViewModel
        {
            get { return viewModel; }
            private set
            {
                viewModel = value;
                if (viewModel.PreviewPinned)
                {
                    CreatePreview(viewModel);
                }
            }
        }

        private void NodeView_MouseLeave(object sender, MouseEventArgs e)
        {
            if (viewModel != null && viewModel.OnMouseLeave != null)
                viewModel.OnMouseLeave();
        }

        internal PreviewControl PreviewControl
        {
            get
            {
                CreatePreview(ViewModel);

                return previewControl;
            }
        }

        private void CreatePreview(NodeViewModel vm)
        {
            if (previewControl == null)
            {
                previewControl = new PreviewControl(vm);
                previewControl.StateChanged += OnPreviewControlStateChanged;
                previewControl.bubbleTools.MouseEnter += OnPreviewControlMouseEnter;
                previewControl.bubbleTools.MouseLeave += OnPreviewControlMouseLeave;
                expansionBay.Children.Add(previewControl);
            }
        }

        /// <summary>
        /// Returns a boolean value of whether this node view already has its PreviewControl field
        /// constructed (not null), in order to avoid calling the PreviewControl constructor
        /// whenever the accessor property is queried.
        /// </summary>
        internal bool HasPreviewControl
        {
            get
            {
                return previewControl != null;
            }
        }

        #region constructors

        internal Grid grid;
        internal Border nodeBorder;
        internal TextBlock NameBlock;
        internal TextBox EditableNameBox;
        private Canvas _expansionBay;
        internal Canvas expansionBay
        {
            get
            {
                if(_expansionBay == null)
                {
                    _expansionBay = new Canvas()
                    {
                        Name = "expansionBay",
                        Margin = new System.Windows.Thickness(0, 4, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Background = Brushes.Blue
                    };

                    //TODO DebugAST Canvas.Do we need this ?
                    //TODO IsCustomFunction section.Do we need this ?

                    Grid.SetRow(expansionBay, 5);
                    Grid.SetColumnSpan(expansionBay, 3);

                    grid.Children.Add(expansionBay);
                }

                return _expansionBay;
                        
            }
        }
        internal Grid centralGrid;
        internal Rectangle nodeIcon;
        //TODO real property getter for public objects
        public Grid inputGrid;
        public ContextMenu MainContextMenu;
        public Grid PresentationGrid;

        private static ImageBrush defaultIcon = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/default-node-icon.png")))
        {
            Stretch = Stretch.UniformToFill
        };

        private static SolidColorBrush primaryCharcoal200 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCDCDC"));
        private static InverseBooleanToVisibilityCollapsedConverter inverseBooleanToVisibilityCollapsedConverter = new InverseBooleanToVisibilityCollapsedConverter();
        private static BoolToVisibilityCollapsedConverter boolToVisibilityCollapsedConverter = new BoolToVisibilityCollapsedConverter();
        private static BooleanToVisibilityConverter booleanToVisibilityConverter = new BooleanToVisibilityConverter();
        private static FontFamily artifactElementReg = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;

        public NodeView()
        {
            //Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            //Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            //Resources.MergedDictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
            //Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);
            //Resources.MergedDictionaries.Add(SharedDictionaryManager.InPortsDictionary);
            //Resources.MergedDictionaries.Add(SharedDictionaryManager.OutPortsDictionary);

            //Not sure if InitializeComponent() should be called first or later?
            InitializeComponent();

            //TODO See if this can work vs adding it to the DataTemplatesDictionary
            //DataTemplate InPortsDataTemplate = new DataTemplate()
            //{
            //    DataType = typeof(InPortViewModel)
            //};

            //InPortsDataTemplate.VisualTree = new FrameworkElementFactory(typeof(InPorts));
            //Resources.Add(typeof(InPortViewModel), InPortsDataTemplate);

            #region shared objects

            //Maybe these can be static on the view?
            

            #endregion


            this.grid = new Grid()
            {
                Name = "grid",
                HorizontalAlignment = HorizontalAlignment.Left,
                //Height = 189,
                //Width = 234.5
            };

            grid.SetBinding(Grid.VisibilityProperty, new Binding("IsCollapsed") { Converter = inverseBooleanToVisibilityCollapsedConverter });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });  // Todo set this to 0 now and then set it taller if custome node
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(46) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(24) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(12) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); //new GridLength(98.5)
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star), MinWidth = 10 }); //new GridLength(55.5)
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); //new GridLength(80.5)

            //Todo context menu

            var nodeBackground = new System.Windows.Shapes.Rectangle()
            {
                Name = "nodeBackground",
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C3C3C")),  //DarkGreyBrush
            };

            Grid.SetRow(nodeBackground, 2);
            Grid.SetRowSpan(nodeBackground, 3);
            Grid.SetColumnSpan(nodeBackground, 3);
            Canvas.SetZIndex(nodeBackground, 1);

            var nameBackground = new Border()
            {
                Name = "nameBackground",
                CornerRadius = new CornerRadius(8, 8, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#535353")),
                IsHitTestVisible = true,
            };

            Grid.SetRow(nameBackground, 1);
            Grid.SetColumnSpan(nameBackground, 3);
            Canvas.SetZIndex(nameBackground, 2);
            //TODO unhook event handler
            nameBackground.MouseDown += NameBlock_OnMouseDown;
            ToolTipService.SetShowDuration(nameBackground, 60000);

            // Create DynamoToolTip
            DynamoToolTip dynamoToolTip = new DynamoToolTip
            {
                AttachmentSide = DynamoToolTip.Side.Top,
                OverridesDefaultStyle = true,
                HasDropShadow = false,
                Style = GetDynamoToolTipTopStyle()
            };

            // Create outer StackPanel
            StackPanel tooltipStackPanel = new StackPanel
            {
                MaxWidth = 320,
                Margin = new Thickness(10),
                Orientation = Orientation.Vertical
            };

            // Create TextBlocks
            TextBlock textBlock1 = new TextBlock
            {
                FontFamily = artifactElementReg,
                FontWeight = FontWeights.Medium,
                TextWrapping = TextWrapping.Wrap
            };

            textBlock1.Inlines.Add(new Run { Text = Dynamo.Wpf.Properties.Resources.NodeTooltipOriginalName });

            var runOriginalName = new Run();
            runOriginalName.SetBinding(Run.TextProperty, new Binding("OriginalName") { Mode = BindingMode.OneWay });
            textBlock1.Inlines.Add(runOriginalName);


            TextBlock textBlock2 = new TextBlock
            {
                FontFamily = artifactElementReg,
                FontWeight = FontWeights.Light,
                TextWrapping = TextWrapping.Wrap
            };

            textBlock2.SetBinding(UIElement.VisibilityProperty, new Binding("IsCustomFunction") { Converter = boolToVisibilityCollapsedConverter });
            textBlock2.Inlines.Add(new Run { Text = Dynamo.Wpf.Properties.Resources.NodeTooltipOriginalName });

            var runPackageName = new Run();
            runPackageName.SetBinding(Run.TextProperty, new Binding("PackageName") { Mode = BindingMode.OneWay });
            textBlock2.Inlines.Add(runPackageName);


            TextBlock textBlock3 = new TextBlock
            {
                Text = "\x0a"
            };

            TextBlock textBlock4 = new TextBlock
            {
                FontFamily = artifactElementReg,
                FontWeight = FontWeights.Medium,
                TextWrapping = TextWrapping.Wrap
            };
            textBlock4.Inlines.Add(new Run { Text = Dynamo.Wpf.Properties.Resources.NodeTooltipDescription });

            var runDescription = new Run();
            runDescription.SetBinding(Run.TextProperty, new Binding("Description") { Mode = BindingMode.OneWay });
            textBlock4.Inlines.Add(runOriginalName);

            // Add TextBlocks to inner StackPanel
            tooltipStackPanel.Children.Add(textBlock1);
            tooltipStackPanel.Children.Add(textBlock2);
            tooltipStackPanel.Children.Add(textBlock3);
            tooltipStackPanel.Children.Add(textBlock4);

            // Set Grid as content of DynamoToolTip
            dynamoToolTip.Content = tooltipStackPanel;
            nameBackground.ToolTip = dynamoToolTip;

            var nodeHeaderContent = new DockPanel()
            {
                Name = "nodeHeaderContent",
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new System.Windows.Thickness(6),
            };

            Grid.SetRow(nodeHeaderContent, 1);
            Grid.SetColumnSpan(nodeHeaderContent, 3);
            Canvas.SetZIndex(nodeHeaderContent, 3);

            nodeIcon = new System.Windows.Shapes.Rectangle()
            {
                Name = "nodeIcon",
                Width = 34,
                Height = 34,
            };

            nodeHeaderContent.Children.Add(nodeIcon);

            this.NameBlock = new TextBlock()
            {
                Name = "NameBlock",
                Margin = new System.Windows.Thickness(6, 3, 6, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                FontWeight = FontWeights.Medium,
                Foreground = primaryCharcoal200,
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                FontFamily = artifactElementReg
            };

            NameBlock.SetBinding(TextBlock.TextProperty, new Binding("Name")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            nodeHeaderContent.Children.Add(NameBlock);

            this.EditableNameBox = new TextBox()
            {
                Name = "EditableNameBox",
                Margin = new System.Windows.Thickness(6, 3, 6, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                FontWeight = FontWeights.Medium,
                Foreground = primaryCharcoal200,
                SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6ac0e7")),
                SelectionOpacity = 0.2,
                IsHitTestVisible = true,
                BorderThickness = new System.Windows.Thickness(0),
                TextAlignment = TextAlignment.Center,
                Visibility = Visibility.Collapsed,
                FontFamily = artifactElementReg
            };

            //Todo unhook event handlers
            EditableNameBox.LostFocus += EditableNameBox_OnLostFocus;
            EditableNameBox.KeyDown += EditableNameBox_KeyDown;
            EditableNameBox.SetBinding(TextBox.TextProperty, new Binding("Name")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            nodeHeaderContent.Children.Add(EditableNameBox);

            //TODO Add Grid / Ellipse / DynamoToolTip

            var inPortControl = new ItemsControl()
            {
                Name = "inPortControl",
                Margin = new System.Windows.Thickness(-25, 3, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            inPortControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("InPorts"));
            Grid.SetRow(inPortControl, 2);
            Grid.SetColumn(inPortControl, 0);
            Canvas.SetZIndex(inPortControl, 6);

            //TODO Add Output Ports

            //TODO LazyLoad
            //this.centralGrid = new Grid()
            //{
            //    Name = "centralGrid",
            //    Margin = new System.Windows.Thickness(6, 6, 6, 3),
            //    VerticalAlignment = VerticalAlignment.Top
            //};

            //centralGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            //centralGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            //centralGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            //Grid.SetRow(centralGrid, 2);
            //Grid.SetColumn(centralGrid, 1);
            //Canvas.SetZIndex(centralGrid, 4);

            //this.inputGrid = new Grid()
            //{
            //    Name = "inputGrid",
            //    MinHeight = Configuration.Configurations.PortHeightInPixels,
            //    Margin = new System.Windows.Thickness(6, 6, 6, 3)
            //};

            //Canvas.SetZIndex(inputGrid, 5);
            //inputGrid.SetBinding(Grid.IsEnabledProperty, new Binding("IsInteractionEnabled"));

            //centralGrid.Children.Add(inputGrid);

            var GlyphStackPanel = new StackPanel()
            {
                Name = "GlyphStackPanel",
                Margin = new System.Windows.Thickness(0, 0, 2, 2),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                //FlowDirection = FlowDirection.LeftToRight,
                Orientation = Orientation.Horizontal
            };

            var experimentalIcon = new FontAwesome5.ImageAwesome()
            {
                Name = "experimentalIcon",
                Icon = FontAwesome5.EFontAwesomeIcon.Solid_Flask,
                Width = 16,
                Height = 16,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38ABDF")),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                ToolTip = new ToolTip() { Content = Dynamo.Properties.Resources.DocsExperimentalPrefixMessage }
            };

            experimentalIcon.SetBinding(Grid.VisibilityProperty, new Binding("IsExperimental")
            {
                Converter = boolToVisibilityCollapsedConverter,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            var FrozenImage = new Image()
            {
                Name = "FrozenImage",
                Width = 16,
                Height = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Stretch = Stretch.UniformToFill,
                Source = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/NodeStates/frozen-64px.png"))
            };

            FrozenImage.SetBinding(Grid.VisibilityProperty, new Binding("IsFrozen")
            {
                Converter = boolToVisibilityCollapsedConverter,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            var TransientImage = new Image()
            {
                Name = "TransientImage",
                Width = 16,
                Height = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Stretch = Stretch.UniformToFill,
                Source = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/NodeStates/transient-64px.png"))
            };

            TransientImage.SetBinding(Grid.VisibilityProperty, new Binding("IsTransient")
            {
                Converter = boolToVisibilityCollapsedConverter,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            var HiddenEyeImage = new Image()
            {
                Name = "HiddenEyeImage",
                Width = 16,
                Height = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Stretch = Stretch.UniformToFill,
                Source = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/hidden.png"))
            };

            HiddenEyeImage.SetBinding(Grid.VisibilityProperty, new Binding("IsVisible")
            {
                Converter = inverseBooleanToVisibilityCollapsedConverter,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            var LacingIconGlyph = new Label()
            {
                Name = "LacingIconGlyp",
                Margin = new System.Windows.Thickness(0, 1, 2, -1),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = artifactElementReg,
                FontSize = 10,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EBEBEB"))
            };

            var lacingToolTip = new ToolTip();

            lacingToolTip.SetBinding(ContentControl.ContentProperty, new Binding("ArgumentLacing")
            {
                Converter = new LacingToTooltipConverter()
            });

            LacingIconGlyph.ToolTip = lacingToolTip;

            LacingIconGlyph.SetBinding(Label.VisibilityProperty, new Binding("ArgumentLacing")
            {
                Converter = new LacingToVisibilityConverter(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            LacingIconGlyph.SetBinding(Label.ContentProperty, new Binding("ArgumentLacing")
            {
                Converter = new LacingToAbbreviationConverter(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            Grid.SetRow(GlyphStackPanel, 3);
            Grid.SetColumnSpan(GlyphStackPanel, 3);
            Canvas.SetZIndex(GlyphStackPanel, 4);

            GlyphStackPanel.Children.Add(experimentalIcon);
            GlyphStackPanel.Children.Add(FrozenImage);
            GlyphStackPanel.Children.Add(TransientImage);
            GlyphStackPanel.Children.Add(HiddenEyeImage);
            GlyphStackPanel.Children.Add(LacingIconGlyph);
            //TODO Finish GlyphStackPanel

            //TODO Lazy Load
            //this.PresentationGrid = new Grid()
            //{
            //    Name = "PresentationGrid",
            //    Margin = new System.Windows.Thickness(6, 6, 6, -3),
            //    HorizontalAlignment = HorizontalAlignment.Left,
            //    VerticalAlignment = VerticalAlignment.Bottom,
            //    Visibility = Visibility.Collapsed
            //};

            //Grid.SetRow(PresentationGrid, 2);
            //Grid.SetColumn(PresentationGrid, 1);
            //Canvas.SetZIndex(PresentationGrid, 3);

            this.nodeBorder = new Border()
            {
                Name = "nodeBorder",
                CornerRadius = new CornerRadius(8, 8, 0, 0),
                Margin = new Thickness(-1),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9F9F9")),
                IsHitTestVisible = true,
                SnapsToDevicePixels = true
            };

            Grid.SetRow(nodeBorder, 1);
            Grid.SetRowSpan(nodeBorder, 4);
            Grid.SetColumnSpan(nodeBorder, 3);
            Canvas.SetZIndex(nodeBorder, 5);

            var selectionBorder = new Border()
            {
                Name = "selectionBorder",
                CornerRadius = new CornerRadius(10, 10, 0, 0),
                Margin = new System.Windows.Thickness(-3),
                BorderThickness = new System.Windows.Thickness(4),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6AC0E7")),
                IsHitTestVisible = false
            };

            //TODO nodeColorOverlayZoomin Transient Out
            //TODO zoomGlyphsGrid

            selectionBorder.SetBinding(Border.VisibilityProperty, new Binding("IsSelected")
            {
                Converter = booleanToVisibilityConverter,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            Grid.SetRow(selectionBorder, 1);
            Grid.SetRowSpan(selectionBorder, 4);
            Grid.SetColumnSpan(selectionBorder, 3);
            Canvas.SetZIndex(selectionBorder, 6);

            //TODO nodeHoveringStateBorder
            //TODO warningBar

            //TODO Lazy Load
            //this.expansionBay = new Canvas()
            //{
            //    Name = "expansionBay",
            //    Margin = new System.Windows.Thickness(0, 4, 0, 0),
            //    HorizontalAlignment = HorizontalAlignment.Left,
            //    Background = new SolidColorBrush(Colors.Blue)
            //};

            //TODO DebugAST Canvas.  Do we need this?
            //TODO IsCustomFunction section.  Do we need this?

            //Grid.SetRow(expansionBay, 5);
            //Grid.SetColumnSpan(expansionBay, 3);

            grid.Children.Add(nodeBackground);
            grid.Children.Add(nameBackground);
            grid.Children.Add(nodeHeaderContent);
            grid.Children.Add(inPortControl);
            //grid.Children.Add(centralGrid);
            grid.Children.Add(GlyphStackPanel);
            //grid.Children.Add(PresentationGrid);
            grid.Children.Add(nodeBorder);
            grid.Children.Add(selectionBorder);
            //grid.Children.Add(expansionBay);

            this.Content = grid;

            Loaded += OnNodeViewLoaded;
            Unloaded += OnNodeViewUnloaded;
            nodeBackground.Loaded += NodeViewReady;

            nodeBorder.SizeChanged += OnSizeChanged;
            DataContextChanged += OnDataContextChanged;


            Panel.SetZIndex(this, 1);
        }

        private static Style GetDynamoToolTipTopStyle()
        {
            var infoBubbleEdgeNormalBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999"));
            var infoBubbleBackNormalBrush = Brushes.White;

            Style customTooltipStyle = new Style(typeof(DynamoToolTip));

            // Create the ControlTemplate
            ControlTemplate toolTipTemplate = new ControlTemplate(typeof(DynamoToolTip));
            FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));

            // Add RowDefinitions to the Grid
            FrameworkElementFactory rowDef0 = new FrameworkElementFactory(typeof(RowDefinition));
            rowDef0.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Auto));
            gridFactory.AppendChild(rowDef0);

            FrameworkElementFactory rowDef1 = new FrameworkElementFactory(typeof(RowDefinition));
            rowDef1.SetValue(RowDefinition.HeightProperty, new GridLength(6));
            gridFactory.AppendChild(rowDef1);

            FrameworkElementFactory columnDef0 = new FrameworkElementFactory(typeof(ColumnDefinition));
            columnDef0.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Auto));
            gridFactory.AppendChild(columnDef0);

            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Grid.RowProperty, 0);
            borderFactory.SetValue(Border.MarginProperty, new Thickness(0, 0, 0, -1));
            borderFactory.SetValue(Border.BackgroundProperty, infoBubbleBackNormalBrush);
            borderFactory.SetValue(Border.BorderBrushProperty, infoBubbleEdgeNormalBrush);
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(2));
            gridFactory.AppendChild(borderFactory);

            // TODO simplify this shape??
            var polylineSegment = new PolyLineSegment()
            {
                Points = new PointCollection(new List<Point>()
                {
                    new Point(0,0),
                    new Point(5,6),
                    new Point(10,0)
                })
            };

            var tooltipPathFigure = new PathFigure()
            {
                IsClosed = false,
                StartPoint = new Point(0, 0)
            };

            tooltipPathFigure.Segments.Add(polylineSegment);

            var tooltipGeometry = new PathGeometry();
            tooltipGeometry.Figures.Add(tooltipPathFigure);

            FrameworkElementFactory pathFactory = new FrameworkElementFactory(typeof(Path));
            pathFactory.SetValue(Path.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            pathFactory.SetValue(Path.VerticalAlignmentProperty, VerticalAlignment.Center);
            pathFactory.SetValue(Path.FillProperty, infoBubbleBackNormalBrush);
            pathFactory.SetValue(Path.StrokeProperty, infoBubbleEdgeNormalBrush);
            pathFactory.SetValue(Path.StrokeThicknessProperty, 1.0);
            pathFactory.SetValue(Path.DataProperty, tooltipGeometry);
            pathFactory.SetValue(Grid.RowProperty, 1);
            gridFactory.AppendChild(pathFactory);

            FrameworkElementFactory contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.MarginProperty, new Thickness(4));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Top);
            contentPresenterFactory.SetValue(TextBlock.FontSizeProperty, 14.0);
            gridFactory.AppendChild(contentPresenterFactory);

            toolTipTemplate.VisualTree = gridFactory;
            customTooltipStyle.Setters.Add(new Setter(DynamoToolTip.TemplateProperty, toolTipTemplate));

            return customTooltipStyle;
        }


        private void OnNodeViewUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.NodeLogic.DispatchedToUI -= NodeLogic_DispatchedToUI;
            ViewModel.RequestShowNodeHelp -= ViewModel_RequestShowNodeHelp;
            ViewModel.RequestShowNodeRename -= ViewModel_RequestShowNodeRename;
            ViewModel.RequestsSelection -= ViewModel_RequestsSelection;
            ViewModel.RequestClusterAutoCompletePopupPlacementTarget -= ViewModel_RequestClusterAutoCompletePopupPlacementTarget;
            ViewModel.RequestAutoCompletePopupPlacementTarget -= ViewModel_RequestAutoCompletePopupPlacementTarget;
            ViewModel.RequestPortContextMenuPopupPlacementTarget -= ViewModel_RequestPortContextMenuPlacementTarget;
            ViewModel.NodeLogic.PropertyChanged -= NodeLogic_PropertyChanged;
            ViewModel.NodeModel.ConnectorAdded -= NodeModel_ConnectorAdded;
            MouseLeave -= NodeView_MouseLeave;

            if (previewControl != null)
            {
                previewControl.StateChanged -= OnPreviewControlStateChanged;
                previewControl.MouseEnter -= OnPreviewControlMouseEnter;
                previewControl.MouseLeave -= OnPreviewControlMouseLeave;
                expansionBay.Children.Remove(previewControl);
                previewControl = null;
            }
            nodeBorder.SizeChanged -= OnSizeChanged;
        }

        #endregion

        /// <summary>
        /// Called when the size of the node changes. Communicates changes down to the view model 
        /// then to the model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnSizeChanged(object sender, EventArgs eventArgs)
        {
            if (ViewModel == null || ViewModel.PreferredSize.HasValue) return;

            var size = new[] { ActualWidth, nodeBorder.ActualHeight };
            if (ViewModel.SetModelSizeCommand.CanExecute(size))
            {
                ViewModel.SetModelSizeCommand.Execute(size);
            }
        }

        /// <summary>
        /// This event handler is called soon as the NodeViewModel is bound to this 
        /// NodeView, which happens way before OnNodeViewLoaded event is sent. 
        /// There is a known bug in WPF 4.0 where DataContext becomes DisconnectedItem 
        /// when actions such as tab switching happens (that is when the View becomes 
        /// disconnected from the underlying ViewModel/Model that it was bound to). So 
        /// it is more reliable for NodeView to cache the NodeViewModel it is bound 
        /// to when it first becomes available, and refer to the cached value at a later
        /// time.
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If this is the first time NodeView is bound to the NodeViewModel, 
            // cache the DataContext (i.e. NodeViewModel) locally and start 
            // referencing it from this point onwards. Note that this notification 
            // can be sent as a result of DataContext becoming DisconnectedItem too,
            // but ViewModel should not be updated in that case (hence the null-check).
            // 
            if (null != ViewModel) return;

            ViewModel = e.NewValue as NodeViewModel;

            //Set NodeIcon
            if (ViewModel.ImageSource == null)
            {
                nodeIcon.Fill = defaultIcon;
            }
            else
            {
                nodeIcon.Fill = new ImageBrush(ViewModel.ImageSource)
                {
                    Stretch = Stretch.UniformToFill
                };
            }

            if (ViewModel.IsCustomFunction)
            {

                var customNodeBorder0 = new Border()
                {
                    Name = "customNodeBorder0",
                    Height = 8,
                    Margin = new System.Windows.Thickness(0, 16, 0, 16),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    CornerRadius = new CornerRadius(6, 6, 0, 0),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#959595")) //
                };

                Grid.SetRow(customNodeBorder0, 0);
                Grid.SetColumnSpan(customNodeBorder0, 3);
                Canvas.SetZIndex(customNodeBorder0, 0);

                var customNodeBorder1 = new Border()
                {
                    Name = "customNodeBorder1",
                    Height = 4,
                    Margin = new System.Windows.Thickness(0, 8, 0, 8),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    CornerRadius = new CornerRadius(6, 6, 0, 0),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#747474"))
                };

                Grid.SetRow(customNodeBorder0, 0);
                Grid.SetColumnSpan(customNodeBorder0, 3);
                Canvas.SetZIndex(customNodeBorder0, 0);

                grid.Children.Add(customNodeBorder0);
                grid.Children.Add(customNodeBorder1);
            }

            if (!ViewModel.PreferredSize.HasValue) return;

            var size = ViewModel.PreferredSize.Value;
            nodeBorder.Width = size.Width;
            nodeBorder.Height = size.Height;
            nodeBorder.RenderSize = size;
        }

        private void OnNodeViewLoaded(object sender, RoutedEventArgs e)
        {
            // We no longer cache the DataContext (NodeViewModel) here because 
            // OnNodeViewLoaded gets called at a much later time and we need the 
            // ViewModel to be valid earlier (e.g. OnSizeChanged is called before
            // OnNodeViewLoaded, and it needs ViewModel for size computation).
            // 
            // ViewModel = this.DataContext as NodeViewModel;
            ViewModel.NodeLogic.DispatchedToUI += NodeLogic_DispatchedToUI;
            ViewModel.RequestShowNodeHelp += ViewModel_RequestShowNodeHelp;
            ViewModel.RequestShowNodeRename += ViewModel_RequestShowNodeRename;
            ViewModel.RequestsSelection += ViewModel_RequestsSelection;
            ViewModel.RequestClusterAutoCompletePopupPlacementTarget += ViewModel_RequestClusterAutoCompletePopupPlacementTarget;
            ViewModel.RequestAutoCompletePopupPlacementTarget += ViewModel_RequestAutoCompletePopupPlacementTarget;
            ViewModel.RequestPortContextMenuPopupPlacementTarget += ViewModel_RequestPortContextMenuPlacementTarget;
            ViewModel.NodeLogic.PropertyChanged += NodeLogic_PropertyChanged;
            ViewModel.NodeModel.ConnectorAdded += NodeModel_ConnectorAdded;
            MouseLeave += NodeView_MouseLeave;
        }

        private void NodeModel_ConnectorAdded(Graph.Connectors.ConnectorModel obj)
        {
            // If the mouse does not leave the node after the connector is added,
            // try to show the preview bubble without new mouse enter event. 
            if (IsMouseOver)
            {
                Dispatcher.BeginInvoke(new Action(TryShowPreviewBubbles), DispatcherPriority.Loaded);
            }
        }

        void NodeLogic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ArgumentLacing":
                    ViewModel.SetLacingTypeCommand.RaiseCanExecuteChanged();
                    break;

                case "CachedValue":
                    CachedValueChanged();
                    break;

                case "IsSetAsInput":
                    (this.DataContext as NodeViewModel).DynamoViewModel.CurrentSpace.HasUnsavedChanges = true;
                    break;

                case "IsSetAsOutput":
                    (this.DataContext as NodeViewModel).DynamoViewModel.CurrentSpace.HasUnsavedChanges = true;
                    break;
            }
        }

        /// <summary>
        /// Called when the NodeModel's CachedValue property is updated
        /// </summary>
        private void CachedValueChanged()
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                // There is no preview control or the preview control is 
                // currently in transition state (it can come back to handle
                // the new data later on when it is ready).
                // If node is frozen, we shouldn't update cached value.
                // We keep value, that was before freezing. 
                if ((previewControl == null) || ViewModel.IsFrozen)
                {
                    return;
                }

                // Enqueue an update of the preview control once it has completed its 
                // transition
                if (previewControl.IsInTransition)
                {
                    previewControl.RequestForRefresh();
                    return;
                }

                if (previewControl.IsHidden) // The preview control is hidden.
                {
                    previewControl.IsDataBound = false;
                    return;
                }

                previewControl.BindToDataSource();
            }));
        }

        private void ViewModel_RequestAutoCompletePopupPlacementTarget(Popup popup)
        {
            popup.PlacementTarget = this;

            ViewModel.ActualHeight = ActualHeight;
            ViewModel.ActualWidth = ActualWidth;
        }

        private void ViewModel_RequestClusterAutoCompletePopupPlacementTarget(Window window, double spacing)
        {
            Point positionFromScreen = PointToScreen(new Point(0, this.ActualHeight));
            PresentationSource source = PresentationSource.FromVisual(this);
            Point targetPoints = source.CompositionTarget.TransformFromDevice.Transform(positionFromScreen);
                
            window.Left = targetPoints.X;
            window.Top = targetPoints.Y + spacing;
        }

        private void ViewModel_RequestPortContextMenuPlacementTarget(Popup popup)
        {
            popup.PlacementTarget = this;

            ViewModel.ActualHeight = ActualHeight;
            ViewModel.ActualWidth = ActualWidth;
        }

        void ViewModel_RequestsSelection(object sender, EventArgs e)
        {
            if (!ViewModel.NodeLogic.IsSelected)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.ClearSelection();
                }

                DynamoSelection.Instance.Selection.AddUnique(ViewModel.NodeLogic);
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.Selection.Remove(ViewModel.NodeLogic);
                }
            }
        }

        void ViewModel_RequestShowNodeRename(object sender, NodeDialogEventArgs e)
        {
            if (e.Handled) return;

            e.Handled = true;

            var editWindow = new EditWindow(viewModel.DynamoViewModel, false, true)
            {
                DataContext = ViewModel,
                Title = Dynamo.Wpf.Properties.Resources.EditNodeWindowTitle
            };

            editWindow.Owner = Window.GetWindow(this);

            editWindow.BindToProperty(null, new Binding("Name")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = ViewModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
        }

        void ViewModel_RequestShowNodeHelp(object sender, NodeDialogEventArgs e)
        {
            if (e.Handled) return;

            e.Handled = true;

            var nodeAnnotationEventArgs = new OpenNodeAnnotationEventArgs(viewModel.NodeModel, viewModel.DynamoViewModel);
            ViewModel.DynamoViewModel.OpenDocumentationLink(nodeAnnotationEventArgs);
        }

        void NodeLogic_DispatchedToUI(object sender, UIDispatcherEventArgs e)
        {
            Dispatcher.Invoke(e.ActionToDispatch);
        }

        private bool nodeViewReadyCalledOnce = false;
        private void NodeViewReady(object sender, RoutedEventArgs e)
        {
            if (nodeViewReadyCalledOnce) return;

            nodeViewReadyCalledOnce = true;
            ViewModel.DynamoViewModel.OnNodeViewReady(this);
        }

        private Dictionary<UIElement, bool> enabledDict
            = new Dictionary<UIElement, bool>();

        internal void DisableInteraction()
        {
            enabledDict.Clear();

            foreach (UIElement e in inputGrid.Children)
            {
                enabledDict[e] = e.IsEnabled;

                e.IsEnabled = false;
            }

            //set the state using the view model's command
            if (ViewModel.SetStateCommand.CanExecute(ElementState.Dead))
                ViewModel.SetStateCommand.Execute(ElementState.Dead);
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            nodeWasClicked = true;
            BringToFront();
        }

        /// <summary>
        /// If Zindex is more then max value of int, it should be set back to 0 to all elements.
        /// </summary>
        private void PrepareZIndex()
        {
            NodeViewModel.StaticZIndex = Configurations.NodeStartZIndex;

            var parent = TemplatedParent as ContentPresenter;
            if (parent == null) return;

            foreach (var child in parent.ChildrenOfType<NodeView>())
            {
                child.ViewModel.ZIndex = Configurations.NodeStartZIndex;
            }
        }

        private void topControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null || Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control) return;

            var view = WpfUtilities.FindUpVisualTree<DynamoView>(this);
            ViewModel.DynamoViewModel.OnRequestReturnFocusToView();

            view?.mainGrid.Focus();

            Guid nodeGuid = ViewModel.NodeModel.GUID;
            ViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.SelectModelCommand(nodeGuid, Keyboard.Modifiers.AsDynamoType()));

            viewModel.OnSelected(this, EventArgs.Empty);

            if (e.ClickCount == 2)
            {
                if (ViewModel.GotoWorkspaceCommand.CanExecute(null))
                {
                    e.Handled = true;
                    ViewModel.GotoWorkspaceCommand.Execute(null);
                }
            }
        }

        private void NameBlock_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Debug.WriteLine("Name double clicked!");
                // If workspace is zoomed-out, open an Edit Name dialog, otherwise rename inline
                if (viewModel.WorkspaceViewModel.Zoom < Configurations.ZoomDirectEditThreshold)
                {
                    if (ViewModel != null && ViewModel.RenameCommand.CanExecute(null))
                    {
                        ViewModel.RenameCommand.Execute(null);
                    }
                }
                else
                {
                    ChangeNameInline();
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// Edit Node Name directly in the Node Header
        /// </summary>
        private void ChangeNameInline()
        {
            NameBlock.Visibility = Visibility.Collapsed;
            EditableNameBox.Visibility = Visibility.Visible;

            EditableNameBox.Focus();
            if (EditableNameBox.SelectionLength == 0)
                EditableNameBox.SelectAll();
        }

        /// <summary>
        ///  Finalize Inline Rename by hiding the TextBox and showing the TextBlock
        /// </summary>
        private void EndInlineRename()
        {
            NameBlock.Visibility = Visibility.Visible;
            EditableNameBox.Visibility = Visibility.Collapsed;

            ViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.UpdateModelValueCommand(
                    System.Guid.Empty, ViewModel.NodeModel.GUID, nameof(NodeModel.Name), NameBlock.Text));
        }

        private void EditableNameBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            EndInlineRename();
        }

        private void EditableNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
                EndInlineRename();
        }


        #region Preview Control Related Event Handlers

        private void OnNodeViewMouseEnter(object sender, MouseEventArgs e)
        {
            // if the node is located under "Hide preview bubbles" menu item and the item is clicked,
            // ViewModel.DynamoViewModel.ShowPreviewBubbles will be updated AFTER node mouse enter event occurs
            // so, wait while ShowPreviewBubbles binding updates value
            Dispatcher.BeginInvoke(new Action(TryShowPreviewBubbles), DispatcherPriority.Loaded);
        }

        private void TryShowPreviewBubbles()
        {
            nodeWasClicked = false;

            // Always set old ZIndex to the last value, even if mouse is not over the node.
            oldZIndex = NodeViewModel.StaticZIndex;

            // There is no need run further.
            if (IsPreviewDisabled()) return;

            if (PreviewControl.IsInTransition) // In transition state, come back later.
                return;

            if (PreviewControl.IsHidden)
            {
                if (!previewControl.IsDataBound)
                    PreviewControl.BindToDataSource();

                PreviewControl.TransitionToState(PreviewControl.State.Condensed);
            }

            Dispatcher.DelayInvoke(previewDelay, BringToFront);
        }

        private bool IsPreviewDisabled()
        {
            // True if preview bubbles are turned off globally 
            // Or a connector is being created now
            // Or the user is selecting nodes
            // Or preview is disabled for this node
            // Or preview shouldn't be shown for some nodes (e.g. number sliders, watch nodes etc.)
            // Or node is frozen.
            // Or node is transient state.
            return !ViewModel.DynamoViewModel.ShowPreviewBubbles ||
                ViewModel.WorkspaceViewModel.IsConnecting ||
                ViewModel.WorkspaceViewModel.IsSelecting || !previewEnabled ||
                !ViewModel.IsPreviewInsetVisible || ViewModel.IsFrozen || viewModel.IsTransient;
        }

        private void OnNodeViewMouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.ZIndex = oldZIndex;

            //Watch nodes doesn't have Preview so we should avoid to use any method/property in PreviewControl class due that Preview is created automatically
            if (ViewModel.NodeModel != null && ViewModel.NodeModel is CoreNodeModels.Watch) return;

            // If mouse in over node/preview control or preview control is pined, we can not hide preview control.
            if (IsMouseOver || PreviewControl.IsMouseOver || PreviewControl.StaysOpen || IsMouseInsidePreview(e) ||
                (Mouse.Captured is DragCanvas && IsMouseInsideNodeOrPreview(e.GetPosition(this)))) return;

            // If it's expanded, then first condense it.
            if (PreviewControl.IsExpanded)
            {
                PreviewControl.TransitionToState(PreviewControl.State.Condensed);
            }
            // If it's condensed, then try to hide it.
            if (PreviewControl.IsCondensed && Mouse.Captured == null)
            {
                PreviewControl.TransitionToState(PreviewControl.State.Hidden);
            }
        }

        /// <summary>
        /// This event fires right after preview's state has been changed.
        /// This event is necessary, it handles some preview's manipulations, 
        /// that we can't handle in mouse enter/leave events.
        /// E.g. When mouse leaves preview control, it should be first condensed, after that hidden.
        /// </summary>
        /// <param name="sender">PreviewControl</param>
        /// <param name="e">Event arguments</param>
        private void OnPreviewControlStateChanged(object sender, EventArgs e)
        {
            var preview = sender as PreviewControl;
            // If the preview is in a transition, return directly to avoid another
            // transition
            if (preview == null || preview.IsInTransition || DynCmd.IsTestMode)
            {
                return;
            }

            switch (preview.CurrentState)
            {
                case PreviewControl.State.Hidden:
                    {
                        if (IsMouseOver && previewEnabled)
                        {
                            preview.TransitionToState(PreviewControl.State.Condensed);
                        }
                        break;
                    }
                case PreviewControl.State.Condensed:
                    {
                        if (preview.bubbleTools.IsMouseOver || preview.StaysOpen)
                        {
                            preview.TransitionToState(PreviewControl.State.Expanded);
                        }

                        if (!IsMouseOver)
                        {
                            // If mouse is captured by DragCanvas and mouse is still over node, preview should stay open.
                            if (!(Mouse.Captured is DragCanvas && IsMouseInsideNodeOrPreview(Mouse.GetPosition(this))))
                            {
                                preview.TransitionToState(PreviewControl.State.Hidden);
                            }
                        }
                        break;
                    }
                case PreviewControl.State.Expanded:
                    {
                        if (!preview.bubbleTools.IsMouseOver && !preview.StaysOpen)
                        {
                            preview.TransitionToState(PreviewControl.State.Condensed);
                        }
                        break;
                    }
            };
        }

        /// <summary>
        /// Sets ZIndex of node the maximum value.
        /// </summary>
        private void BringToFront()
        {
            if (!IsMouseOver && !PreviewControl.IsMouseOver && !DynCmd.IsTestMode) return;

            if (NodeViewModel.StaticZIndex == int.MaxValue)
            {
                PrepareZIndex();
            }

            var index = ++NodeViewModel.StaticZIndex;

            // increment all Notes to ensure that they are always above any Node
            NoteViewModel.StaticZIndex = index + 1;

            foreach (var note in ViewModel.WorkspaceViewModel.Notes)
            {
                note.ZIndex = NoteViewModel.StaticZIndex;
            }

            oldZIndex = nodeWasClicked ? index : ViewModel.ZIndex;
            ViewModel.ZIndex = index;
        }

        private void OnPreviewControlMouseEnter(object sender, MouseEventArgs e)
        {
            if (PreviewControl.IsCondensed)
            {
                PreviewControl.TransitionToState(PreviewControl.State.Expanded);
            }
        }

        private void OnPreviewControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (!PreviewControl.StaysOpen)
            {
                PreviewControl.TransitionToState(PreviewControl.State.Condensed);
            }
        }

        private void OnNodeViewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured == null) return;

            bool isInside = IsMouseInsideNodeOrPreview(e.GetPosition(this));

            if (!isInside && PreviewControl != null && previewControl.IsCondensed)
            {
                PreviewControl.TransitionToState(PreviewControl.State.Hidden);
            }
        }

        /// <summary>
        /// When Mouse is captured, all mouse events are handled by element, that captured it.
        /// So we can't use MouseLeave/MouseEnter events.
        /// In this case, when we want to ensure, that mouse really left node, we use HitTest.
        /// </summary>
        /// <param name="mousePosition">Correct position of mouse</param>
        /// <returns>bool</returns>
        private bool IsMouseInsideNodeOrPreview(Point mousePosition)
        {
            bool isInside = false;
            VisualTreeHelper.HitTest(
                this,
                d =>
                {
                    if (d == this)
                    {
                        isInside = true;
                    }
                    return HitTestFilterBehavior.Stop;
                },
                ht => HitTestResultBehavior.Stop,
                new PointHitTestParameters(mousePosition));
            return isInside;
        }

        /// <summary>
        /// Checks whether a mouse event occurred at a position matching the preview.
        /// Alternatives attempted:
        /// - PreviewControl.IsMouseOver => This is always false
        /// - HitTest on NodeView => Anomalous region that skips right part of preview if larger than node
        /// - HitTest on Preview => Bounds become irreversible larger than "mouse over area" after preview is expanded
        /// </summary>
        /// <param name="e">A mouse event</param>
        /// <returns>Whether the mouse is over the preview or not</returns>
        private bool IsMouseInsidePreview(MouseEventArgs e)
        {
            var isInside = false;
            if (previewControl != null)
            {
                var bounds = new Rect(0, 0, previewControl.ActualWidth, previewControl.ActualHeight);
                var mousePosition = e.GetPosition(previewControl);
                isInside = bounds.Contains(mousePosition);
            }

            return isInside;
        }

        /// <summary>
        /// Enables/disables preview control. 
        /// </summary>
        internal void TogglePreviewControlAllowance()
        {
            previewEnabled = !previewEnabled;

            //Watch nodes doesn't have Preview so we should avoid to use any method/property in PreviewControl class due that Preview is created automatically
            if (ViewModel.NodeModel != null && ViewModel.NodeModel is CoreNodeModels.Watch) return;

            if (previewEnabled == false && !PreviewControl.StaysOpen)
            {
                if (PreviewControl.IsExpanded)
                {
                    PreviewControl.TransitionToState(PreviewControl.State.Condensed);
                    PreviewControl.TransitionToState(PreviewControl.State.Hidden);
                }
                else if (PreviewControl.IsCondensed)
                {
                    PreviewControl.TransitionToState(PreviewControl.State.Hidden);
                }
            }
        }

        #endregion

        /// <summary>
        /// A dictionary of MenuItems which are added during certain nodes' NodeViewCustomization process.
        /// </summary>
        private OrderedDictionary NodeViewCustomizationMenuItems { get; } = new OrderedDictionary();

        /// <summary>
        /// Saves a persistent list of unique MenuItems that are added by certain nodes during their NodeViewCustomization process.
        /// Because nodes' ContextMenus are loaded lazily, and their MenuItems are disposed on closing,
        /// these custom MenuItems need to be manually re-injected into the context menu whenever it is opened.
        /// </summary>
        private void StashNodeViewCustomizationMenuItems()
        {
            foreach (var obj in grid.ContextMenu.Items)
            {
                if (!(obj is MenuItem menuItem)) continue;

                // We don't stash default MenuItems, such as 'Freeze'.
                if (NodeContextMenuBuilder.NodeContextMenuDefaultItemNames.Contains(menuItem.Header.ToString())) continue;

                // We don't stash the same MenuItem multiple times.
                if (NodeViewCustomizationMenuItems.Contains(menuItem.Header.ToString())) continue;

                // The MenuItem gets stashed.
                NodeViewCustomizationMenuItems.Add(menuItem.Header.ToString(), menuItem);
            }
        }

        /// <summary>
        /// A common method to handle the node Options Button being clicked and
        /// the user right-clicking on the node body to open its ContextMenu.
        /// </summary>
        private void DisplayNodeContextMenu(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.NodeModel?.IsTransient is true ||
                ViewModel?.NodeModel?.HasTransientConnections() is true)
            {
                e.Handled = true;
                return;
            }


            Guid nodeGuid = ViewModel.NodeModel.GUID;
            ViewModel.WorkspaceViewModel.HideAllPopupCommand.Execute(sender);
            ViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.SelectModelCommand(nodeGuid, Keyboard.Modifiers.AsDynamoType()));

            var contextMenu = grid.ContextMenu;

            // Stashing any injected MenuItems from the Node View Customization process.
            if (contextMenu.Items.Count > 0 && NodeViewCustomizationMenuItems.Count < 1)
            {
                StashNodeViewCustomizationMenuItems();
            }

            // Clearing any existing items in the node's ContextMenu.
            contextMenu.Items.Clear();
            NodeContextMenuBuilder.Build(contextMenu, viewModel, NodeViewCustomizationMenuItems);

            contextMenu.DataContext = viewModel;
            contextMenu.IsOpen = true;

            e.Handled = true;
        }

        private void MainContextMenu_OnClosed(object sender, RoutedEventArgs e)
        {
            grid.ContextMenu.Items.Clear();
            e.Handled = true;
        }

    }
}
