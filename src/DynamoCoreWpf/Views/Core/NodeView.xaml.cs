using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Controls;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using Dynamo.Wpf.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using DynCmd = Dynamo.Models.DynamoModel;
using Label = System.Windows.Controls.Label;
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

        //Todo add message to mark this as deprecated or ContentGrid?  Currently only one item references ContentGrid.  Most use inputGrid

        [Obsolete("This method is deprecated and will be removed in a future version of Dynamo, use the ContentGrid")]
        public Grid inputGrid = null;

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
                ExpansionBay.Children.Add(previewControl);
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

        //View items referenced in the constructor and other internal methods to NodeView
        private Border nameBackground;
        private TextBlock NameBlock;
        private TextBox EditableNameBox;
        private Rectangle nodeIcon;
        private Rectangle nodeBackground;
        private ItemsControl outputPortControl;
        private Button optionsButton;

        //View items referenced outside of NodeView internal to DynamoCoreWPF previously from xaml
        internal Border nodeBorder;
        internal ItemsControl inputPortControl; //for testing
        internal Border customNodeBorder0; //for testing
        internal Grid zoomGlyphsGrid; //for testing
        internal Rectangle nodeColorOverlayZoomOut; //for testing
        internal Grid centralGrid = null;

        //View items referenced outside of NodeView internal to DynamoCoreWPF previously from xaml but now loaded on demand.
        private Canvas expansionBay;
        internal Canvas ExpansionBay
        {
            get
            {
                if(expansionBay == null)
                {
                    expansionBay = new Canvas()
                    {
                        Name = "ExpansionBay",
                        Margin = new Thickness(0, 4, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Background = Brushes.Blue
                    };

                    Grid.SetRow(expansionBay, 5);
                    Grid.SetColumnSpan(expansionBay, 3);

                    grid.Children.Add(expansionBay);
                }

                return expansionBay;
                        
            }
        }

        //View items referenced outside of NodeView as previously from xaml outside of DynamoCoreWPF
        public ContextMenu MainContextMenu = new ContextMenu();
        public Grid grid;
        public Grid PresentationGrid = null;

        //Static resources mostly from DynamoModern themes but some from DynamoColorsAndBrushes.xaml

        //Brushes
        private static SolidColorBrush primaryCharcoal100 = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal100Brush"] as SolidColorBrush;
        private static SolidColorBrush primaryCharcoal200 = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal200Brush"] as SolidColorBrush;
        private static SolidColorBrush blue300 = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["Blue300Brush"] as SolidColorBrush;
        private static SolidColorBrush darkBlue200 = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["DarkBlue200Brush"] as SolidColorBrush;
        private static SolidColorBrush nodeDismissedWarningsGlyphForeground = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeDismissedWarningsGlyphForeground"] as SolidColorBrush;
        private static SolidColorBrush nodeDismissedWarningsGlyphBackground = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeDismissedWarningsGlyphBackground"] as SolidColorBrush;
        private static SolidColorBrush midGrey = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["MidGreyBrush"] as SolidColorBrush;
        private static SolidColorBrush darkerGreyBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["DarkerGreyBrush"] as SolidColorBrush;
        private static SolidColorBrush darkMidGreyBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["DarkMidGreyBrush"] as SolidColorBrush;
        private static SolidColorBrush nodeContextMenuBackgroundHighlight = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeContextMenuBackgroundHighlight"] as SolidColorBrush;
        private static SolidColorBrush nodeContextMenuSeparatorColor = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeContextMenuSeparatorColor"] as SolidColorBrush;
        private static SolidColorBrush nodeOptionsButtonBackground = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeOptionsButtonBackground"] as SolidColorBrush;
        private static SolidColorBrush nodeHoverColor = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal300Brush"] as SolidColorBrush;
        private static SolidColorBrush nodeTransientOverlayColor = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeTransientOverlayColor"] as SolidColorBrush;

        // Converters
        private static InverseBooleanToVisibilityCollapsedConverter inverseBooleanToVisibilityCollapsedConverter = new InverseBooleanToVisibilityCollapsedConverter();
        private static InverseBoolToVisibilityConverter inverseBooleanToVisibilityConverter = new InverseBoolToVisibilityConverter();
        private static BoolToVisibilityCollapsedConverter boolToVisibilityCollapsedConverter = new BoolToVisibilityCollapsedConverter();
        private static BoolToVisibilityConverter booleanToVisibilityConverter = new BoolToVisibilityConverter();
        private static EmptyToVisibilityCollapsedConverter emptyToVisibilityCollapsedConverter = new EmptyToVisibilityCollapsedConverter();
        private static ZoomToVisibilityCollapsedConverter zoomToVisibilityCollapsedConverter = new ZoomToVisibilityCollapsedConverter();
        private static IValueConverter sZoomFadeControl = SharedDictionaryManager.DynamoModernDictionary["SZoomFadeControl"] as IValueConverter;
        private static IValueConverter sZoomFadeInControl = SharedDictionaryManager.DynamoModernDictionary["SZoomFadeInControl"] as IValueConverter;
        private static IValueConverter sZoomFadeOutPreview = SharedDictionaryManager.DynamoModernDictionary["SZoomFadeOutPreview"] as IValueConverter;
        private static IValueConverter sZoomFadeInPreview = SharedDictionaryManager.DynamoModernDictionary["SZoomFadeInPreview"] as IValueConverter;
        private static ConditionalPackageTextConverter conditionalPackageTextConverter = new ConditionalPackageTextConverter();

        // Font
        private static FontFamily artifactElementReg = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;

        // Images
        private static readonly BitmapImage frozenImageSource = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/NodeStates/frozen-64px.png"));
        private static readonly BitmapImage transientImageSource = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/NodeStates/transient-64px.png"));
        private static readonly BitmapImage hiddenEyeImageSource = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/hidden.png"));
        private static readonly BitmapImage nodeButtonDotsSelected = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/more-vertical_selected_16px.png"));
        private static readonly BitmapImage nodeButtonDots = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/more-vertical.png"));
        private static ImageBrush defaultNodeIcon = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/default-node-icon.png")))
        {
            Stretch = Stretch.UniformToFill
        };
        
        private static Style nodeButtonStyle = GetNodeButtonStyle();
        private static Style codeBlockNodeItemControlStyle = GetCodeBlockPortItemControlStyle();
        internal static readonly Style DynamoToolTipTopStyle = GetDynamoToolTipTopStyle();
        private static Binding sZoomFadeControlStyleBinding = GetFadeToOpacityStyleBinding(sZoomFadeControl);
        private static Binding sZoomFadeInControlStyleBinding = GetFadeToOpacityStyleBinding(sZoomFadeInControl);
        private static Binding sZoomFadeInPreviewStyleBinding = GetFadeToOpacityStyleBinding(sZoomFadeInPreview);
        private static Binding sZoomFadeOutPreviewStyleBinding = GetFadeToOpacityStyleBinding(sZoomFadeOutPreview);
        // Initiate context menu style as static resource.
        private static ContextMenu nodeContextMenu = GetNodeContextMenu();

        #region constructors
        static NodeView()
        {
            //Set bitmap scaling mode to low quality for default node icon.
            RenderOptions.SetBitmapScalingMode(defaultNodeIcon, BitmapScalingMode.LowQuality);

            //Freeze the static resource to reduce memory overhead
            frozenImageSource.Freeze();
            transientImageSource.Freeze();
            hiddenEyeImageSource.Freeze();
            nodeButtonDotsSelected.Freeze();
            nodeButtonDots.Freeze();
            defaultNodeIcon.Freeze();
            primaryCharcoal100.Freeze();
            primaryCharcoal200.Freeze();
            blue300.Freeze();
            nodeDismissedWarningsGlyphBackground.Freeze();
            nodeDismissedWarningsGlyphForeground.Freeze();
            midGrey.Freeze();
            darkerGreyBrush.Freeze();
            darkMidGreyBrush.Freeze();
            nodeContextMenuBackgroundHighlight.Freeze();
            nodeContextMenuSeparatorColor.Freeze();
            nodeOptionsButtonBackground.Freeze();
            nodeHoverColor.Freeze();
        }

        public NodeView()
        {
            InitializeComponent();

            this.grid = new Grid()
            {
                Name = "grid",
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            grid.SetBinding(Grid.VisibilityProperty, new Binding("IsCollapsed") { Converter = inverseBooleanToVisibilityCollapsedConverter });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(8) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(46) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto, MinHeight = 24 });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star), MinWidth = 10 });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            grid.ContextMenu = new ContextMenu();

            nodeBackground = new Rectangle()
            {
                Name = "nodeBackground",
                Fill = darkerGreyBrush
            };

            Grid.SetRow(nodeBackground, 2);
            Grid.SetRowSpan(nodeBackground, 3);
            Grid.SetColumnSpan(nodeBackground, 3);
            Canvas.SetZIndex(nodeBackground, 1);

            #region Node Header

            // Node Body Background 
            nameBackground = new Border()
            {
                Name = "nameBackground",
                CornerRadius = new CornerRadius(8, 8, 0, 0),
                Background = darkMidGreyBrush,
                IsHitTestVisible = true,
            };

            Grid.SetRow(nameBackground, 1);
            Grid.SetColumnSpan(nameBackground, 3);
            Canvas.SetZIndex(nameBackground, 2);

            nameBackground.MouseDown += NameBlock_OnMouseDown;
            ToolTipService.SetShowDuration(nameBackground, 60000);

            // Create DynamoToolTip
            DynamoToolTip dynamoToolTip = new DynamoToolTip
            {
                AttachmentSide = DynamoToolTip.Side.Top,
                OverridesDefaultStyle = true,
                HasDropShadow = false,
                Style = DynamoToolTipTopStyle
            };

            // Create consolidated TextBlock for tooltip
            TextBlock consolidatedTooltipTextBlock = new TextBlock
            {
                MaxWidth = 320,
                Margin = new Thickness(10),
                FontFamily = artifactElementReg,
                FontWeight = FontWeights.Medium,
                TextWrapping = TextWrapping.Wrap
            };

            // Build the tooltip text using individual runs
            consolidatedTooltipTextBlock.Inlines.Add(new Run { Text = Dynamo.Wpf.Properties.Resources.NodeTooltipOriginalName });

            var runOriginalName = new Run();
            runOriginalName.SetBinding(Run.TextProperty, new Binding("OriginalName") { Mode = BindingMode.OneWay });
            consolidatedTooltipTextBlock.Inlines.Add(runOriginalName);

            // Add conditional package section using a MultiBinding
            var runPackageSection = new Run { FontWeight = FontWeights.Light };
            var packageMultiBinding = new MultiBinding();
            packageMultiBinding.Bindings.Add(new Binding("IsCustomFunction") { Mode = BindingMode.OneWay });
            packageMultiBinding.Bindings.Add(new Binding("PackageName") { Mode = BindingMode.OneWay });
            packageMultiBinding.Converter = conditionalPackageTextConverter;
            runPackageSection.SetBinding(Run.TextProperty, packageMultiBinding);
            consolidatedTooltipTextBlock.Inlines.Add(runPackageSection);

            // Add line break before description
            consolidatedTooltipTextBlock.Inlines.Add(new Run { Text = "\x0a\x0a" });

            // Add "Description:" text and bound Description
            consolidatedTooltipTextBlock.Inlines.Add(new Run { Text = Dynamo.Wpf.Properties.Resources.NodeTooltipDescription });

            var runDescription = new Run();
            runDescription.SetBinding(Run.TextProperty, new Binding("Description") { Mode = BindingMode.OneWay });
            consolidatedTooltipTextBlock.Inlines.Add(runDescription);

            // Set consolidated TextBlock as content of DynamoToolTip
            dynamoToolTip.Content = consolidatedTooltipTextBlock;
            nameBackground.ToolTip = dynamoToolTip;

            var nodeHeaderContent = new DockPanel()
            {
                Name = "nodeHeaderContent",
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(6),
            };

            Grid.SetRow(nodeHeaderContent, 1);
            Grid.SetColumnSpan(nodeHeaderContent, 3);
            Canvas.SetZIndex(nodeHeaderContent, 3);

            nodeIcon = new Rectangle()
            {
                Name = "nodeIcon",
                Width = 34,
                Height = 34,
            };

            nodeHeaderContent.Children.Add(nodeIcon);

            this.NameBlock = new TextBlock()
            {
                Name = "NameBlock",
                Margin = new Thickness(6, 3, 6, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                FontWeight = FontWeights.Medium,
                Foreground = primaryCharcoal200,
                Background = null,
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                FontFamily = artifactElementReg,
            };

            NameBlock.SetBinding(TextBlock.TextProperty, new Binding("Name")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            NameBlock.SetBinding(TextBlock.StyleProperty, sZoomFadeControlStyleBinding);

            nodeHeaderContent.Children.Add(NameBlock);

            this.EditableNameBox = new TextBox()
            {
                Name = "EditableNameBox",
                Margin = new Thickness(6, 3, 6, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                FontWeight = FontWeights.Medium,
                Foreground = primaryCharcoal200,
                SelectionBrush = blue300,
                SelectionOpacity = 0.2,
                IsHitTestVisible = true,
                BorderThickness = new Thickness(0),
                TextAlignment = TextAlignment.Center,
                Visibility = Visibility.Collapsed,
                FontFamily = artifactElementReg,
                Background = null
            };

            EditableNameBox.LostFocus += EditableNameBox_OnLostFocus;
            EditableNameBox.KeyDown += EditableNameBox_KeyDown;
            EditableNameBox.SetBinding(TextBox.TextProperty, new Binding("Name")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            nodeHeaderContent.Children.Add(EditableNameBox);

            var renameIndicator = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Background = Brushes.Transparent,
            };

            Panel.SetZIndex(renameIndicator, 5);
            renameIndicator.SetValue(DockPanel.DockProperty, Dock.Right);

            // Create and configure the Ellipse
            Ellipse nodeRenamedBlueDot = new Ellipse
            {
                Name = "nodeRenamedBlueDot",
                Width = 8,
                Height = 8,
                Margin = new Thickness(0, 2, 6, 0),
                Fill = blue300,
                Visibility = Visibility.Hidden // Default visibility; will be updated by binding
            };

            nodeRenamedBlueDot.SetBinding(UIElement.VisibilityProperty, new Binding("IsRenamed")
            {
                Converter = booleanToVisibilityConverter
            });

            // Add the Ellipse to the Grid's children
            renameIndicator.Children.Add(nodeRenamedBlueDot);
            nodeHeaderContent.Children.Add(renameIndicator);

            // Create and configure the ToolTip
            DynamoToolTip dynamoRenameToolTip = new DynamoToolTip
            {
                AttachmentSide = DynamoToolTip.Side.Top,
                OverridesDefaultStyle = true,
                HasDropShadow = false,
                Style = DynamoToolTipTopStyle
            };

            TextBlock toolTipTextBlock = new TextBlock
            {
                Padding = new Thickness(10),
                FontFamily = artifactElementReg,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
            };

            toolTipTextBlock.SetBinding(TextBlock.TextProperty, new Binding("OriginalName")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                StringFormat = Wpf.Properties.Resources.NodeTooltipRenamed
            });

            dynamoRenameToolTip.Content = toolTipTextBlock;
            nodeRenamedBlueDot.ToolTip = dynamoRenameToolTip;

            #endregion

            #region InPorts and OutPorts

            inputPortControl = new ItemsControl()
            {
                Name = "inputPortControl",
                Margin = new Thickness(-25, 3, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            inputPortControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("InPorts"));
            Grid.SetRow(inputPortControl, 2);
            Grid.SetColumn(inputPortControl, 0);
            Canvas.SetZIndex(inputPortControl, 6);

            outputPortControl = new ItemsControl()
            {
                Name = "outputPortControl",
                Margin = new Thickness(0, 3, -24, 5),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            };

            outputPortControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("OutPorts"));
            Grid.SetRow(outputPortControl, 2);
            Grid.SetColumn(outputPortControl, 2);
            Canvas.SetZIndex(outputPortControl, 4);

            #endregion

            #region Glyph StackPanel

            var GlyphStackPanel = new StackPanel()
            {
                Name = "GlyphStackPanel",
                Margin = new System.Windows.Thickness(0, 0, 2, 2),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Orientation = Orientation.Horizontal,
            };
            GlyphStackPanel.SetBinding(StackPanel.StyleProperty, sZoomFadeControlStyleBinding);

            var experimentalIcon = new FontAwesome5.ImageAwesome()
            {
                Name = "experimentalIcon",
                Icon = FontAwesome5.EFontAwesomeIcon.Solid_Flask,
                Width = 16,
                Height = 16,
                Foreground = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["Blue400Brush"] as SolidColorBrush,
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
                Source = frozenImageSource
            };

            RenderOptions.SetBitmapScalingMode(FrozenImage, BitmapScalingMode.LowQuality);

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
                Source = transientImageSource
            };

            RenderOptions.SetBitmapScalingMode(TransientImage, BitmapScalingMode.LowQuality);

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
                Source = hiddenEyeImageSource
            };

            RenderOptions.SetBitmapScalingMode(HiddenEyeImage, BitmapScalingMode.LowQuality);

            HiddenEyeImage.SetBinding(Grid.VisibilityProperty, new Binding("IsVisible")
            {
                Converter = inverseBooleanToVisibilityCollapsedConverter,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            var LacingIconGlyph = new Label()
            {
                Name = "LacingIconGlyph",
                Margin = new Thickness(0, 1, 2, -1),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = artifactElementReg,
                FontSize = 10,
                Foreground = nodeDismissedWarningsGlyphBackground
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

            var alertsGlyph = new Grid
            {
                Name = "AlertsGlyph",
                Height = 16,
                MinWidth = 16,
                Margin = new Thickness(0, 0, 3, 0)
            };

            // Create the Border
            Border border = new Border
            {
                Background = nodeDismissedWarningsGlyphBackground,
                CornerRadius = new CornerRadius(8)
            };

            // Create the Label
            Label label = new Label
            {
                Padding = new Thickness(3, 2, 3, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontFamily = artifactElementReg,
                FontSize = 10,
                Foreground = Brushes.Black
            };

            // Create the binding for NumberOfDismissedAlerts
            label.SetBinding(ContentControl.ContentProperty, new Binding("NumberOfDismissedAlerts")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            // Add the Label to the Border
            border.Child = label;
            // Add the Border to the Grid
            alertsGlyph.Children.Add(border);

            // Create the style for the Grid
            Style gridStyle = new Style(typeof(Grid));
            gridStyle.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible));
            gridStyle.Setters.Add(new Setter(FrameworkElement.MinWidthProperty, 16.0));
            gridStyle.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(0, 0, 3, 0)));

            // Create the DataTrigger
            DataTrigger dataTrigger = new DataTrigger
            {
                Binding = new Binding("NumberOfDismissedAlerts"),
                Value = 0
            };
            dataTrigger.Setters.Add(new Setter(FrameworkElement.MinWidthProperty, 0.0));
            dataTrigger.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(0)));
            dataTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));

            // Add the DataTrigger to the style
            gridStyle.Triggers.Add(dataTrigger);

            // Apply the style to the Grid
            alertsGlyph.Style = gridStyle;


            //Todo Can this be adjusted with margin on the stack panel instead?
            //Spacer for embedded resize thumb(visible only on resizable nodes)
            var spacerBorder = new Border
            {   
                Width = 16,
                Height = 16,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                IsHitTestVisible = false
            };

            spacerBorder.SetBinding(UIElement.VisibilityProperty, new Binding("NodeModel.IsResizable")
            {
                Converter = boolToVisibilityCollapsedConverter,
                Mode = BindingMode.OneWay
            });

            //Todo Does this need to be a button
            // Button to open node context menu from lower right corner
            // Create the Button
            optionsButton = new Button
            {
                Name = "OptionsButton"
            };

            // Set the Click event handler
            optionsButton.Click += DisplayNodeContextMenu;

            // Create the ToolTip
            ToolTip toolTip = new ToolTip
            {
                Content = Wpf.Properties.Resources.ContextMenu
            };
            optionsButton.ToolTip = toolTip;
            optionsButton.Style = nodeButtonStyle;

            Grid.SetRow(GlyphStackPanel, 3);
            Grid.SetColumnSpan(GlyphStackPanel, 3);
            Canvas.SetZIndex(GlyphStackPanel, 4);

            GlyphStackPanel.Children.Add(experimentalIcon);
            GlyphStackPanel.Children.Add(FrozenImage);
            GlyphStackPanel.Children.Add(TransientImage);
            GlyphStackPanel.Children.Add(HiddenEyeImage);
            GlyphStackPanel.Children.Add(LacingIconGlyph);
            GlyphStackPanel.Children.Add(alertsGlyph);
            GlyphStackPanel.Children.Add(spacerBorder);
            GlyphStackPanel.Children.Add(optionsButton);

            #endregion

            #region Node Borders and Overlays

            // Standard Border
            this.nodeBorder = new Border()
            {
                Name = "nodeBorder",
                CornerRadius = new CornerRadius(8, 8, 0, 0),
                Margin = new Thickness(-1),
                BorderThickness = new Thickness(1),
                BorderBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["WorkspaceBackgroundHomeBrush"] as SolidColorBrush,
                IsHitTestVisible = true,
                SnapsToDevicePixels = true
            };

            Grid.SetRow(nodeBorder, 1);
            Grid.SetRowSpan(nodeBorder, 4);
            Grid.SetColumnSpan(nodeBorder, 3);
            Canvas.SetZIndex(nodeBorder, 5);

            // Selected Border
            var selectionBorder = new Border()
            {
                Name = "selectionBorder",
                CornerRadius = new CornerRadius(10, 10, 0, 0),
                Margin = new System.Windows.Thickness(-3),
                BorderThickness = new System.Windows.Thickness(4),
                BorderBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["Blue300Brush"] as SolidColorBrush,
                IsHitTestVisible = false
            };

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

            //If a note is dragged over this group
            //this border is activated, indicating that the node can be dropped into the group.
            //The visibility of this is controlled by the NodeViewModel property 'NodeHoveringState'
            //which is set in the StateMachine.
            var nodeHoveringStateBorder = new Border
            {
                Name = "nodeHoveringStateBorder",
                Margin = new Thickness(-3),
                Background = Brushes.Transparent,
                IsHitTestVisible = false,
                CornerRadius = new CornerRadius(10, 10, 0, 0),
                BorderBrush = nodeHoverColor,
                BorderThickness = new Thickness(6),
            };

            // Set Grid and Canvas properties
            Grid.SetRow(nodeHoveringStateBorder, 1);
            Grid.SetRowSpan(nodeHoveringStateBorder, 4);
            Grid.SetColumnSpan(nodeHoveringStateBorder, 3);
            Canvas.SetZIndex(nodeHoveringStateBorder, 41);

            // Set up the Visibility binding
            nodeHoveringStateBorder.SetBinding(Border.VisibilityProperty, new Binding("NodeHoveringState")
            {
                Converter = new BooleanToVisibilityConverter(),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            // Node color overlay when zoomed In for Frozen state
            var nodeColorOverlayZoomIn = new Rectangle
            {
                Name = "nodeColorOverlayZoomIn",
                Margin = new Thickness(-8),
                Fill = darkBlue200,
                IsHitTestVisible = false,
                Opacity = 0.5,
            };
            Grid.SetRow(nodeColorOverlayZoomIn, 1);
            Grid.SetRowSpan(nodeColorOverlayZoomIn, 4);
            Grid.SetColumnSpan(nodeColorOverlayZoomIn, 3);
            Canvas.SetZIndex(nodeColorOverlayZoomIn, 6);

            // Visibility binding
            nodeColorOverlayZoomIn.SetBinding(UIElement.VisibilityProperty, new Binding("IsFrozen")
            {
                Converter = boolToVisibilityCollapsedConverter,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            nodeColorOverlayZoomIn.SetBinding(Rectangle.StyleProperty, sZoomFadeOutPreviewStyleBinding);

            // Node color overlay when zoomed In for Transient state
            var nodeTransientColorOverlayZoomIn = new Rectangle
            {
                Name = "nodeTransientColorOverlayZoomIn",
                Margin = new Thickness(-8),
                Fill = nodeTransientOverlayColor,
                IsHitTestVisible = false,
            };
            Grid.SetRow(nodeTransientColorOverlayZoomIn, 1);
            Grid.SetRowSpan(nodeTransientColorOverlayZoomIn, 4);
            Grid.SetColumnSpan(nodeTransientColorOverlayZoomIn, 3);
            Canvas.SetZIndex(nodeTransientColorOverlayZoomIn, 6);

            // Visibility binding
            nodeTransientColorOverlayZoomIn.SetBinding(UIElement.VisibilityProperty, new Binding("IsTransient")
            {
                Converter = boolToVisibilityCollapsedConverter,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            nodeTransientColorOverlayZoomIn.SetBinding(Rectangle.StyleProperty, sZoomFadeOutPreviewStyleBinding);

            #endregion

            #region Zoomed out overlays and glyphs

            // nodeColorOverlayZoomOut
            nodeColorOverlayZoomOut = new Rectangle
            {
                Name = "nodeColorOverlayZoomOut",
                Margin = new Thickness(-8),
                IsHitTestVisible = false,
                Opacity = 0.5
            };
            Grid.SetRow(nodeColorOverlayZoomOut, 1);
            Grid.SetRowSpan(nodeColorOverlayZoomOut, 4);
            Grid.SetColumnSpan(nodeColorOverlayZoomOut, 3);
            Canvas.SetZIndex(nodeColorOverlayZoomOut, 6);

            // Background binding
            nodeColorOverlayZoomOut.SetBinding(Rectangle.FillProperty, new Binding("NodeOverlayColor")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            nodeColorOverlayZoomOut.SetBinding(Rectangle.StyleProperty, sZoomFadeInPreviewStyleBinding);


            // Visibility binding
            nodeColorOverlayZoomOut.SetBinding(UIElement.VisibilityProperty, new Binding("DataContext.Zoom")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(WorkspaceView), 1),
                Converter = zoomToVisibilityCollapsedConverter
            });

            // Create the main Grid
            zoomGlyphsGrid = new Grid
            {
                Name = "zoomGlyphsGrid",
                MinWidth = 48,
                Margin = new Thickness(0, 5, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = false,
            };
            Grid.SetRow(zoomGlyphsGrid, 0);
            Grid.SetRowSpan(zoomGlyphsGrid, 4);
            Grid.SetColumn(zoomGlyphsGrid, 0);
            Grid.SetColumnSpan(zoomGlyphsGrid, 3);
            Canvas.SetZIndex(zoomGlyphsGrid, 7);

            // Visibility binding
            zoomGlyphsGrid.SetBinding(UIElement.VisibilityProperty, new Binding("DataContext.Zoom")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(WorkspaceView), 1),
                Converter = zoomToVisibilityCollapsedConverter
            });
            zoomGlyphsGrid.SetBinding(Grid.StyleProperty, sZoomFadeInControlStyleBinding);

            // StackPanel
            var stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // UniformGrid (ZoomGlyphRowZero)
            var zoomGlyphRowZero = new UniformGrid
            {
                Name = "ZoomGlyphRowZero",
                Margin = new Thickness(0, 10, 0, 10),
                Columns = 1,
                Rows = 1,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            Grid.SetRow(zoomGlyphRowZero, 0);

            // UniformGrid Visibility binding
            zoomGlyphRowZero.SetBinding(UIElement.VisibilityProperty, new Binding("ImgGlyphThreeSource")
            {
                Converter = emptyToVisibilityCollapsedConverter,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            // Image in UniformGrid
            var zoomStateImgOne = new Image
            {
                Name = "ZoomStateImgOne",
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Width = 64
            };
            zoomStateImgOne.SetBinding(Image.SourceProperty, new Binding("ImgGlyphThreeSource")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                TargetNullValue = null
            });
            RenderOptions.SetBitmapScalingMode(zoomStateImgOne, BitmapScalingMode.LowQuality);

            zoomGlyphRowZero.Children.Add(zoomStateImgOne);

            // Grid (ZoomGlyphRowOne)
            var zoomGlyphRowOne = new Grid { Name = "ZoomGlyphRowOne" };

            zoomGlyphRowOne.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            zoomGlyphRowOne.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            // Image in column 0
            var zoomStateImgTwo = new Image
            {
                Name = "ZoomStateImgTwo",
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 10, 5, 0),
                Width = 64
            };
            Grid.SetColumn(zoomStateImgTwo, 0);

            zoomStateImgTwo.SetBinding(Image.SourceProperty, new Binding("ImgGlyphOneSource")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                TargetNullValue = null
            });
            zoomStateImgTwo.SetBinding(UIElement.VisibilityProperty, new Binding("ImgGlyphOneSource")
            {
                Converter = emptyToVisibilityCollapsedConverter,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            RenderOptions.SetBitmapScalingMode(zoomStateImgTwo, BitmapScalingMode.LowQuality);

            // Image in column 1
            var zoomStateImgThree = new Image
            {
                Name = "ZoomStateImgThree",
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 10, 0, 0),
                Width = 64
            };
            Grid.SetColumn(zoomStateImgThree, 1);

            zoomStateImgThree.SetBinding(Image.SourceProperty, new Binding("ImgGlyphTwoSource")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                TargetNullValue = null
            });
            zoomStateImgThree.SetBinding(UIElement.VisibilityProperty, new Binding("ImgGlyphTwoSource")
            {
                Converter = emptyToVisibilityCollapsedConverter,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            RenderOptions.SetBitmapScalingMode(zoomStateImgThree, BitmapScalingMode.LowQuality);

            zoomGlyphRowOne.Children.Add(zoomStateImgTwo);
            zoomGlyphRowOne.Children.Add(zoomStateImgThree);

            // Add children to StackPanel
            stackPanel.Children.Add(zoomGlyphRowZero);
            stackPanel.Children.Add(zoomGlyphRowOne);

            // Add StackPanel to main Grid
            zoomGlyphsGrid.Children.Add(stackPanel);

            #endregion

            // Warning Bar: Displays when node is in Info/Warning/Error state
            // Create the Rectangle
            Rectangle warningBar = new Rectangle
            {
                Name = "warningBar",
                Height = 12,
            };

            // Set Grid.Row, Grid.Column, and Grid.ColumnSpan
            Grid.SetRow(warningBar, 4);
            Grid.SetColumn(warningBar, 0);
            Grid.SetColumnSpan(warningBar, 3);

            // Set Canvas.ZIndex
            Canvas.SetZIndex(warningBar, 1);

            // Create and set the binding for Fill
            warningBar.SetBinding(Rectangle.FillProperty, new Binding("WarningBarColor")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            // Create and set the binding for Visibility
            warningBar.SetBinding(Rectangle.VisibilityProperty, new Binding("NodeWarningBarVisible")
            {
                Converter = boolToVisibilityCollapsedConverter,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });


            PresentationGrid = new Grid()
            {
                Name = "PresentationGrid",
                Margin = new Thickness(6, 6, 6, -3),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Visibility = Visibility.Collapsed
            };

            Grid.SetRow(PresentationGrid, 2);
            Grid.SetColumn(PresentationGrid, 1);
            Canvas.SetZIndex(PresentationGrid, 3);

            centralGrid = new Grid()
            {
                Name = "centralGrid",
                Margin = new Thickness(6, 6, 6, 3),
                VerticalAlignment = VerticalAlignment.Top
            };

            centralGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            centralGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            centralGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(centralGrid, 2);
            Grid.SetColumn(centralGrid, 1);
            Canvas.SetZIndex(centralGrid, 4);

            inputGrid = new Grid()
            {
                Name = "inputGrid",
                MinHeight = Configuration.Configurations.PortHeightInPixels,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            Canvas.SetZIndex(inputGrid, 5);
            inputGrid.SetBinding(Grid.IsEnabledProperty, new Binding("IsInteractionEnabled"));

            centralGrid.Children.Add(inputGrid);


            //TODO DebugAST Canvas.  Do we need this?

            grid.Children.Add(nodeBackground);
            grid.Children.Add(nameBackground);
            grid.Children.Add(nodeHeaderContent);
            grid.Children.Add(inputPortControl);
            grid.Children.Add(outputPortControl);
            grid.Children.Add(GlyphStackPanel);
            grid.Children.Add(nodeBorder);
            grid.Children.Add(selectionBorder);
            grid.Children.Add(nodeColorOverlayZoomIn);
            grid.Children.Add(nodeTransientColorOverlayZoomIn);
            grid.Children.Add(nodeColorOverlayZoomOut);
            grid.Children.Add(zoomGlyphsGrid);
            grid.Children.Add(nodeHoveringStateBorder);
            grid.Children.Add(warningBar);
            grid.Children.Add(PresentationGrid);
            grid.Children.Add(centralGrid);

            this.Content = grid;

            Loaded += OnNodeViewLoaded;
            Unloaded += OnNodeViewUnloaded;
            nodeBackground.Loaded += NodeViewReady;

            nodeBorder.SizeChanged += OnSizeChanged;
            DataContextChanged += OnDataContextChanged;

            this.SetBinding(UserControl.VisibilityProperty, new Binding("IsHidden")
            {
                Converter = inverseBooleanToVisibilityConverter,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            Panel.SetZIndex(this, 1);
        }
        #endregion

        #region Styles methods
        private static Style GetNodeButtonStyle()
        {
            // Create the Style
            Style buttonStyle = new Style(typeof(Button));

            // Create the ControlTemplate
            ControlTemplate controlTemplate = new ControlTemplate(typeof(Button));
            FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));

            // Create the Border
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "DotsBackgroundBorder";
            borderFactory.SetValue(Border.WidthProperty, 24.0);
            borderFactory.SetValue(Border.HeightProperty, 24.0);
            borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(2));

            // Create the Image
            FrameworkElementFactory imageFactory = new FrameworkElementFactory(typeof(Image));
            imageFactory.Name = "DotsImage";
            imageFactory.SetValue(Image.WidthProperty, 16.0);
            imageFactory.SetValue(Image.HeightProperty, 16.0);
            imageFactory.SetValue(Image.MarginProperty, new Thickness(1.5, 0, 0, 0));
            imageFactory.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            imageFactory.SetValue(Image.VerticalAlignmentProperty, VerticalAlignment.Center);
            imageFactory.SetValue(Image.StretchProperty, Stretch.UniformToFill);

            // Add Border and Image to Grid
            gridFactory.AppendChild(borderFactory);
            gridFactory.AppendChild(imageFactory);

            controlTemplate.VisualTree = gridFactory;

            // Create the Triggers
            Trigger isMouseOverTrueTrigger = new Trigger
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true
            };
            isMouseOverTrueTrigger.Setters.Add(new Setter(Border.BackgroundProperty, nodeOptionsButtonBackground, "DotsBackgroundBorder"));
            isMouseOverTrueTrigger.Setters.Add(new Setter(Image.SourceProperty, nodeButtonDotsSelected, "DotsImage"));

            Trigger isMouseOverFalseTrigger = new Trigger
            {
                Property = UIElement.IsMouseOverProperty,
                Value = false
            };
            isMouseOverFalseTrigger.Setters.Add(new Setter(Border.BackgroundProperty, Brushes.Transparent, "DotsBackgroundBorder"));
            isMouseOverFalseTrigger.Setters.Add(new Setter(Image.SourceProperty, new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/more-vertical.png")), "DotsImage"));

            controlTemplate.Triggers.Add(isMouseOverTrueTrigger);
            controlTemplate.Triggers.Add(isMouseOverFalseTrigger);

            // Set the ControlTemplate in the Style
            buttonStyle.Setters.Add(new Setter(Control.TemplateProperty, controlTemplate));
            return buttonStyle;
        }

        private static Style GetDynamoToolTipTopStyle()
        {
            var infoBubbleEdgeNormalBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["UnSelectedLayoutForeground"] as SolidColorBrush;
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

        private static Style GetCodeBlockPortItemControlStyle()
        {
            Style inOutPortControlStyle = new Style(typeof(ItemsControl));

            // Create the ItemsPanelTemplate
            ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate();

            // Create the InOutPortPanel (assuming dynui:InOutPortPanel is defined in the project)
            FrameworkElementFactory inOutPortPanelFactory = new FrameworkElementFactory(typeof(InOutPortPanel));
            inOutPortPanelFactory.SetValue(InOutPortPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            inOutPortPanelFactory.SetValue(InOutPortPanel.VerticalAlignmentProperty, VerticalAlignment.Stretch);

            // Set the visual tree for the ItemsPanelTemplate
            itemsPanelTemplate.VisualTree = inOutPortPanelFactory;

            // Create the Setter for ItemsPanel
            Setter itemsPanelSetter = new Setter(ItemsControl.ItemsPanelProperty, itemsPanelTemplate);

            // Add the setter to the style
            inOutPortControlStyle.Setters.Add(itemsPanelSetter);

            return inOutPortControlStyle;
        }

        private static Binding GetFadeToOpacityStyleBinding(IValueConverter conv)
        {
            return new Binding("DataContext.NodeCountOptimizationEnabled")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(WorkspaceView), 1),
                Converter = conv
            };
        }
        #endregion

        private void DelayPreviewControlAction()
        {
            if (!IsMouseOver) return;

            TryShowPreviewBubbles();
        }

        private void InitialTryShowPreviewBubble()
        {
            // Always set old ZIndex to the last value, even if mouse is not over the node.
            oldZIndex = NodeViewModel.StaticZIndex;

            viewModel.WorkspaceViewModel.DelayNodePreviewControl.Debounce(300, DelayPreviewControlAction);
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

            nameBackground.MouseDown -= NameBlock_OnMouseDown;
            EditableNameBox.LostFocus -= EditableNameBox_OnLostFocus;
            EditableNameBox.KeyDown -= EditableNameBox_KeyDown;
            optionsButton.Click -= DisplayNodeContextMenu;
            nodeBorder.SizeChanged -= OnSizeChanged;
            nodeBackground.Loaded -= NodeViewReady;

            if (previewControl != null)
            {
                previewControl.StateChanged -= OnPreviewControlStateChanged;
                previewControl.MouseEnter -= OnPreviewControlMouseEnter;
                previewControl.MouseLeave -= OnPreviewControlMouseLeave;
                ExpansionBay.Children.Remove(previewControl);
                previewControl = null;
            }
            DataContextChanged -= OnDataContextChanged;
            Loaded -= OnNodeViewLoaded;
            Unloaded -= OnNodeViewUnloaded;
        }

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
                nodeIcon.Fill = defaultNodeIcon;
            }
            else
            {
                var icon = new ImageBrush(ViewModel.ImageSource)
                {
                    Stretch = Stretch.UniformToFill
                };
                icon.Freeze();
                nodeIcon.Fill = icon;
            }

            //Add the adjusted Style for CodeBlockNodeModel to add overrides for Measure / Layout
            if(ViewModel.NodeModel is CodeBlockNodeModel)
            {
                outputPortControl.Margin = new Thickness(0, 12, -24, 0);
                outputPortControl.Style = codeBlockNodeItemControlStyle;
            }

            //Add view items for custom nodes
            if (ViewModel.IsCustomFunction)
            {
                SetCustomNodeVisuals();
            }

            if (!ViewModel.PreferredSize.HasValue) return;

            var size = ViewModel.PreferredSize.Value;
            nodeBorder.Width = size.Width;
            nodeBorder.Height = size.Height;
            nodeBorder.RenderSize = size;
        }
    
        private void SetCustomNodeVisuals()
        {
            customNodeBorder0 = new Border()
            {
                Name = "customNodeBorder0",
                Height = 8,
                Margin = new Thickness(16, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Bottom,
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                Background = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["LightGreyBrush"] as SolidColorBrush
            };

            Grid.SetRow(customNodeBorder0, 0);
            Grid.SetColumnSpan(customNodeBorder0, 3);
            Canvas.SetZIndex(customNodeBorder0, 0);

            var customNodeBorder1 = new Border()
            {
                Name = "customNodeBorder1",
                Height = 4,
                Margin = new Thickness(8, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Bottom,
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                Background = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["DarkMidGreyBrush"] as SolidColorBrush
            };

            Grid.SetRow(customNodeBorder1, 0);
            Grid.SetColumnSpan(customNodeBorder1, 3);
            Canvas.SetZIndex(customNodeBorder1, 0);

            // Create the Canvas
            var customFunctionCanvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            // Set Grid.Row and Canvas.ZIndex
            Grid.SetRow(customFunctionCanvas, 3);
            Canvas.SetZIndex(customFunctionCanvas, 51);

            // Set up the Visibility binding
            var visibilityBinding = new Binding("IsCustomFunction")
            {
                Converter = new BoolToVisibilityCollapsedConverter()
            };
            customFunctionCanvas.SetBinding(Canvas.VisibilityProperty, visibilityBinding);

            // Create the Polygon
            var polygon = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(0, -15),
                    new Point(15, 0),
                    new Point(0, 0)
                },
                Fill = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal100Brush"] as SolidColorBrush
            };

            // Add the Polygon to the Canvas
            customFunctionCanvas.Children.Add(polygon);

            grid.Children.Add(customNodeBorder0);
            grid.Children.Add(customNodeBorder1);
            grid.Children.Add(customFunctionCanvas);
        }

        private void OnNodeViewLoaded(object sender, RoutedEventArgs e)
        {
            // We no longer cache the DataContext (NodeViewModel) here because 
            // OnNodeViewLoaded gets called at a much later time and we need the 
            // ViewModel to be valid earlier (e.g. OnSizeChanged is called before
            // OnNodeViewLoaded, and it needs ViewModel for size computation).

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
                if (DynCmd.IsTestMode)
                {
                    Dispatcher.BeginInvoke(new Action(TryShowPreviewBubbles), DispatcherPriority.Loaded);
                }
                else
                {
                    InitialTryShowPreviewBubble();
                }
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

        private Point PointToLocal(double x, double y, UIElement target)
        {
            Point positionFromScreen = target.PointToScreen(new Point(x, y));
            PresentationSource source = PresentationSource.FromVisual(target);
            Point targetPoints = source.CompositionTarget.TransformFromDevice.Transform(positionFromScreen);
            return targetPoints;
        }

        private void ViewModel_RequestAutoCompletePopupPlacementTarget(Window window, PortModel portModel, double spacing)
        {
            if (portModel.PortType == PortType.Input)
            {
                var portView = inputPortControl.ItemContainerGenerator.ContainerFromIndex(portModel.Index) as FrameworkElement;
                window.Top = PointToLocal(0, 0, portView).Y;
                window.Left = PointToLocal(0, 0, this).X - window.Width - spacing;
            }
            else
            {
                var portView = outputPortControl.ItemContainerGenerator.ContainerFromIndex(portModel.Index) as FrameworkElement;
                window.Top = PointToLocal(0, 0, portView).Y;
                window.Left = PointToLocal(ActualWidth, 0, this).X + spacing;
            }
        }

        private void ViewModel_RequestClusterAutoCompletePopupPlacementTarget(Window window, double spacing)
        {
            Point targetPoints = PointToLocal(0, ActualHeight, this);
            window.Left = Math.Clamp(targetPoints.X,
                    SystemParameters.VirtualScreenLeft,
                    SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - window.Width);
            window.Top = Math.Clamp(targetPoints.Y + spacing,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - window.Height);
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
            if (DynCmd.IsTestMode)
            {
                Dispatcher.BeginInvoke(new Action(TryShowPreviewBubbles), DispatcherPriority.Loaded);
            }
            else
            {
                InitialTryShowPreviewBubble();
            }
        }

        private void TryShowPreviewBubbles()
        {
            nodeWasClicked = false;

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
            // Or node is frozen
            // Or we are panning the view.
            return !ViewModel.DynamoViewModel.ShowPreviewBubbles ||
                ViewModel.WorkspaceViewModel.IsConnecting ||
                ViewModel.WorkspaceViewModel.IsSelecting || !previewEnabled ||
                !ViewModel.IsPreviewInsetVisible || ViewModel.IsFrozen ||
                ViewModel.WorkspaceViewModel.IsPanning;
        }

        private void OnNodeViewMouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.ZIndex = oldZIndex;
            viewModel.WorkspaceViewModel.DelayNodePreviewControl?.Cancel();

            // The preview hasn't been instantiated yet, we should stop here 
            if (previewControl == null) return;

            // Watch nodes doesn't have Preview so we should avoid to use any method/property in PreviewControl class due that Preview is created automatically
            if (ViewModel.NodeModel != null && ViewModel.NodeModel is CoreNodeModels.Watch) return;

            // If mouse in over node/preview control or preview control is pined, we can not hide preview control.
            // check the field and not the property because that will trigger the instantiation
            if (IsMouseOver || previewControl?.IsMouseOver == true ||
                previewControl?.StaysOpen == true || IsMouseInsidePreview(e) ||
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
            if (MainContextMenu != null && MainContextMenu.Items.Count > 0 && NodeViewCustomizationMenuItems.Count == 0)
            {
                foreach (var obj in MainContextMenu.Items)
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
        }

        #region Context Menu related methods

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
                if (grid.ContextMenu != null)
                {
                    grid.ContextMenu.Visibility = Visibility.Collapsed;
                }
                return;
            }


            Guid nodeGuid = ViewModel.NodeModel.GUID;
            ViewModel.WorkspaceViewModel.HideAllPopupCommand.Execute(sender);
            ViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.SelectModelCommand(nodeGuid, Keyboard.Modifiers.AsDynamoType()));

            StashNodeViewCustomizationMenuItems();

            // Clearing any existing items in the node's ContextMenu.
            MainContextMenu.Items.Clear();
            nodeContextMenu.Items.Clear();
            NodeContextMenuBuilder.Build(nodeContextMenu, viewModel, NodeViewCustomizationMenuItems);

            MainContextMenu = nodeContextMenu;
            grid.ContextMenu = MainContextMenu;
            grid.ContextMenu.Visibility = Visibility.Visible;
            MainContextMenu.DataContext = viewModel;
            MainContextMenu.PlacementTarget = grid;
            MainContextMenu.Closed += MainContextMenu_OnClosed;
            MainContextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void MainContextMenu_OnClosed(object sender, RoutedEventArgs e)
        {
            MainContextMenu.Closed -= MainContextMenu_OnClosed;
            MainContextMenu.Items.Clear();
            e.Handled = true;
        }

        private static Style GetContextMenuStyle()
        {
            var contextMenuStyle = new Style(typeof(ContextMenu));
            contextMenuStyle.Setters.Add(new Setter(ContextMenu.PlacementProperty, PlacementMode.MousePoint));
            contextMenuStyle.Setters.Add(new Setter(ContextMenu.ForegroundProperty, primaryCharcoal100));
            contextMenuStyle.Setters.Add(new Setter(ContextMenu.FontSizeProperty, 13.0));
            contextMenuStyle.Setters.Add(new Setter(ContextMenu.FontFamilyProperty, artifactElementReg));
            contextMenuStyle.Setters.Add(new Setter(ContextMenu.FontWeightProperty, FontWeights.Medium));
            contextMenuStyle.Setters.Add(new Setter(ContextMenu.SnapsToDevicePixelsProperty, true));
            contextMenuStyle.Setters.Add(new Setter(ContextMenu.OverridesDefaultStyleProperty, true));

            var contextMenuTemplate = new ControlTemplate(typeof(ContextMenu));
            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "Border";
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(ContextMenu.BackgroundProperty));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0));

            var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
            stackPanel.SetValue(StackPanel.MarginProperty, new Thickness(0, 10, 0, 0));
            stackPanel.SetValue(StackPanel.ClipToBoundsProperty, true);
            stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
            stackPanel.SetValue(StackPanel.IsItemsHostProperty, true);

            border.AppendChild(stackPanel);
            contextMenuTemplate.VisualTree = border;
            contextMenuStyle.Setters.Add(new Setter(ContextMenu.TemplateProperty, contextMenuTemplate));

            return contextMenuStyle;
        }

        private static ContextMenu GetNodeContextMenu()
        {
            var mainContextMenu = new ContextMenu
            {
                Name = "MainContextMenu",
                Background = midGrey,
                Style = GetContextMenuStyle(),
            };

            var menuItemStyle = new Style(typeof(MenuItem));
            menuItemStyle.Setters.Add(new Setter(MenuItem.IsCheckedProperty, new DynamicResourceExtension("IsChecked")));
            menuItemStyle.Setters.Add(new Setter(MenuItem.HeightProperty, 30.0));
            menuItemStyle.Setters.Add(new Setter(MenuItem.WidthProperty, 240.0));
            menuItemStyle.Setters.Add(new Setter(MenuItem.PaddingProperty, new Thickness(20, 0, 20, 0)));

            var menuItemTemplate = new ControlTemplate(typeof(MenuItem));
            var dockPanel = new FrameworkElementFactory(typeof(DockPanel));
            dockPanel.Name = "dockPanel";
            dockPanel.SetValue(DockPanel.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            dockPanel.SetValue(DockPanel.BackgroundProperty, Brushes.Transparent);
            dockPanel.SetValue(DockPanel.SnapsToDevicePixelsProperty, true);

            var checkBox = new FrameworkElementFactory(typeof(Label));
            checkBox.Name = "checkBox";
            checkBox.SetValue(Label.MarginProperty, new Thickness(2, 0, -20, 0));
            checkBox.SetValue(Label.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            checkBox.SetValue(Label.VerticalAlignmentProperty, VerticalAlignment.Center);
            checkBox.SetValue(Label.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
            checkBox.SetValue(Label.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            checkBox.SetValue(Label.ContentProperty, "✓");
            checkBox.SetValue(Label.FontSizeProperty, 9.0);
            checkBox.SetValue(Label.ForegroundProperty, Brushes.White);
            checkBox.SetValue(Label.VisibilityProperty, Visibility.Collapsed);
            checkBox.SetValue(DockPanel.DockProperty, Dock.Left);

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.Name = "ContentPresenter";
            contentPresenter.SetValue(ContentPresenter.MarginProperty, new TemplateBindingExtension(MenuItem.PaddingProperty));
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.ContentSourceProperty, "Header");
            contentPresenter.SetValue(DockPanel.DockProperty, Dock.Left);
            contentPresenter.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
            contentPresenter.SetValue(ContentPresenter.SnapsToDevicePixelsProperty, new TemplateBindingExtension(MenuItem.SnapsToDevicePixelsProperty));

            //var contentPresenterTextBlockStyle = new Style(typeof(TextBlock));
            //contentPresenterTextBlockStyle.Setters.Add(new Setter(TextBlock.MaxWidthProperty, 200.0));
            //contentPresenterTextBlockStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
            //contentPresenter.Resources.Add(typeof(TextBlock), contentPresenterTextBlockStyle);

            var dismissedAlertsBadge = new FrameworkElementFactory(typeof(Border));
            dismissedAlertsBadge.Name = "dismissedAlertsBadge";
            dismissedAlertsBadge.SetValue(Border.HeightProperty, 15.0);
            dismissedAlertsBadge.SetValue(Border.MinWidthProperty, 15.0);
            dismissedAlertsBadge.SetValue(Border.MarginProperty, new Thickness(-15, 0, 0, 1));
            dismissedAlertsBadge.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            dismissedAlertsBadge.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Center);
            dismissedAlertsBadge.SetValue(Border.BackgroundProperty, nodeDismissedWarningsGlyphBackground);
            dismissedAlertsBadge.SetValue(Border.CornerRadiusProperty, new CornerRadius(7.5));
            dismissedAlertsBadge.SetValue(DockPanel.DockProperty, Dock.Left);
            dismissedAlertsBadge.SetValue(Border.VisibilityProperty, Visibility.Hidden);

            var dismissedAlertsBadgeLabel = new FrameworkElementFactory(typeof(Label));
            dismissedAlertsBadgeLabel.SetValue(Label.PaddingProperty, new Thickness(2, 2, 2, 0));
            dismissedAlertsBadgeLabel.SetValue(Label.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            dismissedAlertsBadgeLabel.SetValue(Label.VerticalAlignmentProperty, VerticalAlignment.Center);
            dismissedAlertsBadgeLabel.SetValue(Label.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
            dismissedAlertsBadgeLabel.SetValue(Label.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            dismissedAlertsBadgeLabel.SetValue(Label.ContentProperty, new Binding("NumberOfDismissedAlerts") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            dismissedAlertsBadgeLabel.SetValue(Label.FontFamilyProperty, artifactElementReg);
            dismissedAlertsBadgeLabel.SetValue(Label.FontSizeProperty, 9.0);
            dismissedAlertsBadgeLabel.SetValue(Label.ForegroundProperty, nodeDismissedWarningsGlyphForeground);

            dismissedAlertsBadge.AppendChild(dismissedAlertsBadgeLabel);

            var subMenuArrow = new FrameworkElementFactory(typeof(Label));
            subMenuArrow.Name = "subMenuArrow";
            subMenuArrow.SetValue(Label.MarginProperty, new Thickness(0, 0, 20, 7));
            subMenuArrow.SetValue(Label.PaddingProperty, new Thickness(0));
            subMenuArrow.SetValue(Label.VerticalAlignmentProperty, VerticalAlignment.Center);
            subMenuArrow.SetValue(Label.ContentProperty, ">");
            subMenuArrow.SetValue(DockPanel.DockProperty, Dock.Right);
            subMenuArrow.SetValue(Label.FontFamilyProperty, artifactElementReg);
            subMenuArrow.SetValue(Label.FontSizeProperty, 13.0);
            subMenuArrow.SetValue(Label.ForegroundProperty, blue300);

            var subMenuArrowTransform = new ScaleTransform { ScaleX = 1, ScaleY = 1.5 };
            subMenuArrow.SetValue(Label.RenderTransformProperty, subMenuArrowTransform);

            var subMenuArrowStyle = new Style(typeof(Label));
            subMenuArrowStyle.Setters.Add(new Setter(Label.VisibilityProperty, Visibility.Hidden));
            var subMenuArrowDataTrigger = new DataTrigger
            {
                Binding = new Binding("HasItems") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MenuItem), 1) },
                Value = true
            };
            subMenuArrowDataTrigger.Setters.Add(new Setter(Label.VisibilityProperty, Visibility.Visible));
            subMenuArrowStyle.Triggers.Add(subMenuArrowDataTrigger);
            subMenuArrow.SetValue(Label.StyleProperty, subMenuArrowStyle);

            var inputGestureText = new FrameworkElementFactory(typeof(TextBlock));
            inputGestureText.Name = "InputGestureText";
            inputGestureText.SetValue(TextBlock.MarginProperty, new Thickness(0, 2, 2, 2));
            inputGestureText.SetValue(DockPanel.DockProperty, Dock.Right);
            inputGestureText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            inputGestureText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            inputGestureText.SetValue(TextBlock.FontFamilyProperty, artifactElementReg);
            inputGestureText.SetValue(TextBlock.FontSizeProperty, 13.0);
            inputGestureText.SetValue(TextBlock.TextProperty, new TemplateBindingExtension(MenuItem.InputGestureTextProperty));

            dockPanel.AppendChild(checkBox);
            dockPanel.AppendChild(contentPresenter);
            dockPanel.AppendChild(dismissedAlertsBadge);
            dockPanel.AppendChild(subMenuArrow);
            dockPanel.AppendChild(inputGestureText);

            var partPopup = new FrameworkElementFactory(typeof(Popup));
            partPopup.Name = "PART_Popup";
            partPopup.SetValue(Popup.AllowsTransparencyProperty, true);
            partPopup.SetValue(Popup.FocusableProperty, false);
            partPopup.SetValue(Popup.HorizontalOffsetProperty, 0.0);
            partPopup.SetValue(Popup.IsOpenProperty, new Binding("IsSubmenuOpen") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            partPopup.SetValue(Popup.PlacementProperty, PlacementMode.Right);
            partPopup.SetValue(Popup.VerticalOffsetProperty, -2.0);

            var popupBorder = new FrameworkElementFactory(typeof(Border));
            popupBorder.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(MenuItem.BackgroundProperty));
            popupBorder.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            popupBorder.SetValue(Border.BorderThicknessProperty, new Thickness(0));

            var subMenuScrollViewer = new FrameworkElementFactory(typeof(ScrollViewer));
            subMenuScrollViewer.Name = "SubMenuScrollViewer";
            subMenuScrollViewer.SetValue(ScrollViewer.CanContentScrollProperty, true);
            subMenuScrollViewer.SetValue(ScrollViewer.StyleProperty, new DynamicResourceExtension(new ComponentResourceKey(typeof(FrameworkElement), "MenuScrollViewer")));

            var itemsPresenter = new FrameworkElementFactory(typeof(ItemsPresenter));
            itemsPresenter.Name = "ItemsPresenter";
            itemsPresenter.SetValue(Grid.IsSharedSizeScopeProperty, true);
            itemsPresenter.SetValue(KeyboardNavigation.DirectionalNavigationProperty, KeyboardNavigationMode.Cycle);
            itemsPresenter.SetValue(KeyboardNavigation.TabNavigationProperty, KeyboardNavigationMode.Cycle);
            itemsPresenter.SetValue(ItemsPresenter.SnapsToDevicePixelsProperty, new TemplateBindingExtension(MenuItem.SnapsToDevicePixelsProperty));

            subMenuScrollViewer.AppendChild(itemsPresenter);
            popupBorder.AppendChild(subMenuScrollViewer);
            partPopup.AppendChild(popupBorder);
            dockPanel.AppendChild(partPopup);

            // Trigger for IsEnabled property
            var isEnabledTrigger = new Trigger
            {
                Property = UIElement.IsEnabledProperty,
                Value = false
            };
            isEnabledTrigger.Setters.Add(new Setter(TextBlock.OpacityProperty, 0.5, "ContentPresenter"));

            // Trigger for IsMouseOver property (true)
            var isMouseOverTrueTrigger = new Trigger
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true
            };
            isMouseOverTrueTrigger.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.White, "ContentPresenter"));
            isMouseOverTrueTrigger.Setters.Add(new Setter(DockPanel.BackgroundProperty, nodeContextMenuBackgroundHighlight, "dockPanel"));

            // Trigger for IsMouseOver property (false)
            var isMouseOverFalseTrigger = new Trigger
            {
                Property = UIElement.IsMouseOverProperty,
                Value = false
            };
            isMouseOverFalseTrigger.Setters.Add(new Setter(DockPanel.BackgroundProperty, midGrey, "dockPanel"));

            // DataTrigger for Content property
            var dataTrigger = new DataTrigger
            {
                Binding = new Binding("Content") { ElementName = "ContentPresenter" },
                Value = Dynamo.Wpf.Properties.Resources.NodeInformationalStateDismissedAlerts
            };
            dataTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible, "dismissedAlertsBadge"));

            // Trigger for IsChecked property
            var isCheckedTrigger = new Trigger
            {
                Property = MenuItem.IsCheckedProperty,
                Value = true
            };
            isCheckedTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible, "checkBox"));

            menuItemTemplate.VisualTree = dockPanel;

            // Add the triggers to the ControlTemplate
            menuItemTemplate.Triggers.Add(isEnabledTrigger);
            menuItemTemplate.Triggers.Add(isMouseOverTrueTrigger);
            menuItemTemplate.Triggers.Add(isMouseOverFalseTrigger);
            menuItemTemplate.Triggers.Add(dataTrigger);
            menuItemTemplate.Triggers.Add(isCheckedTrigger);

            menuItemStyle.Setters.Add(new Setter(MenuItem.TemplateProperty, menuItemTemplate));
           
            var separatorStyle = new Style(typeof(Separator));
            separatorStyle.Setters.Add(new Setter(Control.OverridesDefaultStyleProperty, true));

            // Define the ControlTemplate for the Separator
            var separatorTemplate = new ControlTemplate(typeof(Separator));
            var separatorBorder = new FrameworkElementFactory(typeof(Border));
            separatorBorder.SetValue(Border.HeightProperty, 1.0);
            separatorBorder.SetValue(Border.MarginProperty, new Thickness(20, 8, 20, 8));
            separatorBorder.SetValue(Border.BackgroundProperty, nodeContextMenuSeparatorColor);
            separatorTemplate.VisualTree = separatorBorder;

            // Add the ControlTemplate to the style
            separatorStyle.Setters.Add(new Setter(Control.TemplateProperty, separatorTemplate));

            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Add(typeof(MenuItem), menuItemStyle);
            resourceDictionary.Add(MenuItem.SeparatorStyleKey, separatorStyle);

            mainContextMenu.Resources = resourceDictionary;

            return mainContextMenu;

            //// Define the ContextMenu for the Grid
            //var gridContextMenu = new ContextMenu();
            //gridContextMenu.Name = "GridContextMenu";
            //gridContextMenu.Items.Add(mainContextMenu);
        }

        #endregion
    }
}
