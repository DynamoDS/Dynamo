using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Dynamo.Controls;
using Dynamo.Utilities;
using HelixToolkit.Wpf;
using Binding = System.Windows.Data.Binding;
using ComboBox = System.Windows.Controls.ComboBox;
using DialogResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MenuItem = System.Windows.Controls.MenuItem;
using TextBox = System.Windows.Controls.TextBox;

namespace Dynamo.Nodes
{
    public abstract partial class dynVariableInput : dynNodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            System.Windows.Controls.Button addButton = new dynNodeButton();
            addButton.Content = "+";
            addButton.Width = 20;
            addButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            addButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            System.Windows.Controls.Button subButton = new dynNodeButton();
            subButton.Content = "-";
            subButton.Width = 20;
            subButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            subButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            var wp = new WrapPanel
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            nodeUI.inputGrid.Children.Add(wp);

            addButton.Click += delegate { AddInput(); RegisterAllPorts(); };
            subButton.Click += delegate { RemoveInput(); RegisterAllPorts(); };
        }

    }

    public partial class dynSublists : dynBasicInteractive<string>
    {

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            var tb = new dynTextBox
            {
                Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            tb.OnChangeCommitted += processTextForNewInputs;

            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                //Converter = new StringDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);

            if (Value != "")
                tb.Commit();
        }

    }

    public partial class dynBreakpoint : dynNodeWithOneOutput
    {
        //System.Windows.Controls.Button button;

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            var button = new dynNodeButton();
            button.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            button.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            //inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.Children.Add(button);
            System.Windows.Controls.Grid.SetColumn(button, 0);
            System.Windows.Controls.Grid.SetRow(button, 0);
            button.Content = "Continue";

            Enabled = false;

            button.Click += new RoutedEventHandler(button_Click);

            var bindingVal = new System.Windows.Data.Binding("Enabled")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = this
            };
            button.SetBinding(UIElement.IsEnabledProperty, bindingVal);
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            Deselect();
            Enabled = false;
        }

    }

    public abstract partial class dynBasicInteractive<T> : dynNodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add an edit window option to the 
            //main context window
            var editWindowItem = new System.Windows.Controls.MenuItem
            {
                Header = "Edit...",
                IsCheckable = false
            };

            nodeUI.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += editWindowItem_Click;
        }

        public virtual void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            //override in child classes
        }

    }

    public partial class dynDoubleInput : dynDouble
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            var tb = new dynTextBox
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Top
            };

            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);

            tb.IsNumeric = true;
            tb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay(),
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);

            tb.Text = "0.0";
        }

    }

    public partial class dynAngleInput : dynDouble
    {

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            var tb = new dynTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.IsNumeric = true;
            tb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new RadianToDegreesConverter(),
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);

            tb.Text = "0.0";
        }

    }

    public partial class dynDoubleSliderInput : dynDouble
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a slider control to the input grid of the control
            var tb_slider = new Slider();
            tb_slider.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb_slider.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            tb_slider.MinWidth = 150;

            tb_slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.None;

            tb_slider.PreviewMouseUp += delegate
            {
                dynSettings.ReturnFocusToSearch();
            };

            var mintb = new dynTextBox();
            mintb.Width = double.NaN;

            mintb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            // input value textbox
            var valtb = new dynTextBox();
            valtb.Width = double.NaN;
            valtb.Margin = new Thickness(0, 0, 10, 0);

            var maxtb = new dynTextBox();
            maxtb.Width = double.NaN;

            maxtb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            var sliderGrid = new Grid();
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            sliderGrid.Children.Add(valtb);
            sliderGrid.Children.Add(mintb);
            sliderGrid.Children.Add(tb_slider);
            sliderGrid.Children.Add(maxtb);

            Grid.SetColumn(valtb, 0);
            Grid.SetColumn(mintb, 1);
            Grid.SetColumn(tb_slider, 2);
            Grid.SetColumn(maxtb, 3);
            nodeUI.inputGrid.Children.Add(sliderGrid);

            maxtb.DataContext = this;
            tb_slider.DataContext = this;
            mintb.DataContext = this;
            valtb.DataContext = this;

            // value input
            var inputBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay()
            };
            valtb.SetBinding(dynTextBox.TextProperty, inputBinding);

            // slider value 
            var sliderBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Source = this,
            };
            tb_slider.SetBinding(Slider.ValueProperty, sliderBinding);

            // max value
            var bindingMax = new System.Windows.Data.Binding("Max")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            maxtb.SetBinding(dynTextBox.TextProperty, bindingMax);

            // max slider value
            var bindingMaxSlider = new System.Windows.Data.Binding("Max")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb_slider.SetBinding(Slider.MaximumProperty, bindingMaxSlider);


            // min value
            var bindingMin = new System.Windows.Data.Binding("Min")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            mintb.SetBinding(dynTextBox.TextProperty, bindingMin);

            // min slider value
            var bindingMinSlider = new System.Windows.Data.Binding("Min")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb_slider.SetBinding(Slider.MinimumProperty, bindingMinSlider);
        }
    }

    public partial class dynBoolSelector : dynBool
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            rbTrue = new System.Windows.Controls.RadioButton();
            rbFalse = new System.Windows.Controls.RadioButton();
            rbTrue.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            rbFalse.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            //use a unique name for the button group
            //so other instances of this element don't get confused
            string groupName = Guid.NewGuid().ToString();
            rbTrue.GroupName = groupName;
            rbFalse.GroupName = groupName;

            rbTrue.Content = "1";
            rbTrue.Padding = new Thickness(5, 0, 12, 0);
            rbFalse.Content = "0";
            rbFalse.Padding = new Thickness(5, 0, 0, 0);

            var wp = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center };
            wp.Children.Add(rbTrue);
            wp.Children.Add(rbFalse);
            nodeUI.inputGrid.Children.Add(wp);

            //rbFalse.IsChecked = true;
            rbTrue.Checked += new System.Windows.RoutedEventHandler(rbTrue_Checked);
            rbFalse.Checked += new System.Windows.RoutedEventHandler(rbFalse_Checked);

            rbFalse.DataContext = this;
            rbTrue.DataContext = this;

            var rbTrueBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
            };
            rbTrue.SetBinding(System.Windows.Controls.RadioButton.IsCheckedProperty, rbTrueBinding);

            var rbFalseBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new InverseBoolDisplay()
            };
            rbFalse.SetBinding(System.Windows.Controls.RadioButton.IsCheckedProperty, rbFalseBinding);
        }

    }

    public partial class dynStringInput : dynString
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            base.SetupCustomUIElements(nodeUI);

            //add a text box to the input grid of the control
            var tb = new dynStringTextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 200,
                VerticalAlignment = VerticalAlignment.Top
            };

            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new StringDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
        }

    }

    public partial class dynStringFilename : dynBasicInteractive<string>
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a button to the inputGrid on the dynElement
            var readFileButton = new dynNodeButton();

            //readFileButton.Margin = new System.Windows.Thickness(4);
            readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            readFileButton.Click += new System.Windows.RoutedEventHandler(readFileButton_Click);
            readFileButton.Content = "Browse...";
            readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            var tb = new TextBox();
            if (string.IsNullOrEmpty(Value))
                Value = "No file selected.";

            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;
            tb.TextChanged += delegate { tb.ScrollToHorizontalOffset(double.PositiveInfinity); dynSettings.ReturnFocusToSearch(); };

            StackPanel sp = new StackPanel();
            sp.Children.Add(readFileButton);
            sp.Children.Add(tb);
            nodeUI.inputGrid.Children.Add(sp);

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new FilePathDisplay()
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
        }


        protected virtual void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                CheckFileExists = false
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Value = openDialog.FileName;
            }
        }
 
    }

    public abstract partial class dynDropDrownBase : dynNodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            base.SetupCustomUIElements(nodeUI);

            //add a drop down list to the window
            var combo = new ComboBox
            {
                Width = 300,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            nodeUI.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += new EventHandler(combo_DropDownOpened);
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    RequiresRecalc = true;
            };

            combo.DataContext = this;
            //bind this combo box to the selected item hash

            var bindingVal = new System.Windows.Data.Binding("Items")
            {
                Mode = BindingMode.TwoWay,
                Source = this
            };
            combo.SetBinding(ComboBox.ItemsSourceProperty, bindingVal);

            //bind the selected index to the 
            var indexBinding = new System.Windows.Data.Binding("SelectedIndex")
            {
                Mode = BindingMode.TwoWay,
                Source = this
            };
            combo.SetBinding(ComboBox.SelectedIndexProperty, indexBinding);
        }

    }

    public abstract partial class dynEnum : dynNodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            var comboBox = new ComboBox
            {
                MinWidth = 150,
                Padding = new Thickness(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };

            nodeUI.inputGrid.Children.Add(comboBox);

            Grid.SetColumn(comboBox, 0);
            Grid.SetRow(comboBox, 0);

            comboBox.ItemsSource = this.Items;
            comboBox.SelectedIndex = this.SelectedIndex;

            comboBox.SelectionChanged += delegate
            {
                if (comboBox.SelectedIndex == -1) return;
                this.RequiresRecalc = true;
                this.SelectedIndex = comboBox.SelectedIndex;
            };
        }

    }

    public partial class dynFormula : dynMathBase
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            var tb = new dynTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.IsNumeric = false;
            tb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            tb.DataContext = this;
            var bindingVal = new Binding("Formula")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
        }
    }

    public partial class dynImageFileReader : dynFileReaderBase
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            var image1 = new System.Windows.Controls.Image
            {
                //Width = 320,
                //Height = 240,
                MaxWidth = 400,
                MaxHeight = 400,
                Margin = new Thickness(5),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Name = "image1",
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            //nodeUI.inputGrid.Children.Add(image1);
            nodeUI.grid.Children.Add(image1);
            image1.SetValue(Grid.RowProperty, 2);
            image1.SetValue(Grid.ColumnProperty, 0);
            image1.SetValue(Grid.ColumnSpanProperty, 3);
        }
    }

    public partial class dynFunction : dynBuiltinFunction
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            nodeUI.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(ui_MouseDoubleClick);
        }
    }

    public partial class dynOutput : dynNodeModel
    {
        TextBox tb;

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            tb = new TextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);

            //turn off the border
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);

            tb.DataContext = this;
            var bindingSymbol = new System.Windows.Data.Binding("Symbol")
            {
                Mode = BindingMode.TwoWay,
                Converter = new StringDisplay()
            };
            tb.SetBinding(TextBox.TextProperty, bindingSymbol);

            tb.TextChanged += tb_TextChanged;
        }

        void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Symbol = tb.Text;
        }

    }

    public partial class dynSymbol : dynNodeModel
    {
        TextBox tb;

        void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Symbol = tb.Text;
        }

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a text box to the input grid of the control
            tb = new TextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);

            //turn off the border
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);

            tb.DataContext = this;
            var bindingSymbol = new System.Windows.Data.Binding("Symbol")
            {
                Mode = BindingMode.TwoWay
            };
            tb.SetBinding(TextBox.TextProperty, bindingSymbol);

            tb.TextChanged += new TextChangedEventHandler(tb_TextChanged);

        }

    }

    public partial class dynWatch : dynNodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            var watchTree = new WatchTree();

            //nodeUI.inputGrid.Children.Add(watchTree);
            nodeUI.grid.Children.Add(watchTree);
            watchTree.SetValue(Grid.RowProperty, 2);
            watchTree.SetValue(Grid.ColumnSpanProperty, 3);
            watchTree.Margin = new Thickness(5, 0, 5, 5);
            watchTreeBranch = watchTree.FindResource("Tree") as WatchTreeBranch;
        }

    }

    public partial class dynWatch3D : dynNodeWithOneOutput
    {
        WatchView _watchView;

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
            _watchView.watch_view.DataContext = this;

            RenderOptions.SetEdgeMode(_watchView, EdgeMode.Unspecified);

            Points = new Point3DCollection();
            Lines = new Point3DCollection();

            _points = new PointsVisual3D { Color = Colors.Red, Size = 6 };
            _lines = new LinesVisual3D { Color = Colors.Blue, Thickness = 1 };

            _points.Points = Points;
            _lines.Points = Lines;

            _watchView.watch_view.Children.Add(_lines);
            _watchView.watch_view.Children.Add(_points);

            _watchView.watch_view.Children.Add(new DefaultLights());

            _watchView.Width = 400;
            _watchView.Height = 300;

            System.Windows.Shapes.Rectangle backgroundRect = new System.Windows.Shapes.Rectangle();
            backgroundRect.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            backgroundRect.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            //backgroundRect.RadiusX = 10;
            //backgroundRect.RadiusY = 10;
            backgroundRect.IsHitTestVisible = false;
            BrushConverter bc = new BrushConverter();
            Brush strokeBrush = (Brush)bc.ConvertFrom("#313131");
            backgroundRect.Stroke = strokeBrush;
            backgroundRect.StrokeThickness = 1;
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 216));
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

            Points = null;
            Lines = null;
            _lines.Points = null;
            _points.Points = null;

            Points = new Point3DCollection();
            Lines = new Point3DCollection();
            Meshes = new List<Mesh3D>();

            // a list of all the upstream IDrawable nodes
            List<IDrawable> drawables = new List<IDrawable>();

            GetUpstreamIDrawable(drawables, Inputs);

            foreach (IDrawable d in drawables)
            {
                d.Draw();

                foreach (Point3D p in d.RenderDescription.points)
                {
                    Points.Add(p);
                }

                foreach (Point3D p in d.RenderDescription.lines)
                {
                    Lines.Add(p);
                }

                foreach (Mesh3D mesh in d.RenderDescription.meshes)
                {
                    Meshes.Add(mesh);
                }
            }

            _lines.Points = Lines;
            _points.Points = Points;

            // remove old meshes from the renderer
            foreach (MeshVisual3D mesh in _meshes)
            {
                _watchView.watch_view.Children.Remove(mesh);
            }

            _meshes.Clear();

            foreach (Mesh3D mesh in Meshes)
            {
                MeshVisual3D vismesh = MakeMeshVisual3D(mesh);
                _watchView.watch_view.Children.Add(vismesh);
                _meshes.Add(vismesh);
            }

            _requiresRedraw = false;
            _isRendering = false;
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            _watchView.watch_view.ZoomExtents();
        }

    }

    public partial class dynStringDirectory : dynStringFilename
    {
        protected override void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Value = openDialog.SelectedPath;
            }
        }
    }

    public partial class dynArduino : dynNodeWithOneOutput
    {
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            string[] serialPortNames = System.IO.Ports.SerialPort.GetPortNames();

            foreach (string portName in serialPortNames)
            {

                if (lastComItem != null)
                {
                    lastComItem.IsChecked = false; // uncheck last checked item
                }
                comItem = new System.Windows.Controls.MenuItem();
                comItem.Header = portName;
                comItem.IsCheckable = true;
                comItem.IsChecked = true;
                comItem.Checked += new System.Windows.RoutedEventHandler(comItem_Checked);
                nodeUI.MainContextMenu.Items.Add(comItem);

                port.PortName = portName;
                lastComItem = comItem;
            }
        }
    }

    public class dynTextBox : System.Windows.Controls.TextBox
    {
        public event Action OnChangeCommitted;

        private static Brush clear = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255, 255, 255));
        private static Brush highlighted = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 255, 255, 255));

        public dynTextBox()
        {
            //turn off the border
            Background = clear;
            BorderThickness = new Thickness(1);
            GotFocus += OnGotFocus;
            LostFocus += OnLostFocus;
            LostKeyboardFocus += OnLostFocus;
        }

        private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Background = clear;
        }

        private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Background = highlighted;
        }

        private bool numeric;
        public bool IsNumeric
        {
            get { return numeric; }
            set
            {
                numeric = value;
                if (value && Text.Length > 0)
                {
                    Text = dynSettings.RemoveChars(
                       Text,
                       Text.ToCharArray()
                          .Where(c => !char.IsDigit(c) && c != '-' && c != '.')
                          .Select(c => c.ToString())
                    );
                }
            }
        }

        private bool pending;
        public bool Pending
        {
            get { return pending; }
            set
            {
                if (value)
                {
                    FontStyle = FontStyles.Italic;
                }
                else
                {
                    FontStyle = FontStyles.Normal;
                }
                pending = value;
            }
        }

        public void Commit()
        {
            var expr = GetBindingExpression(TextBox.TextProperty);
            if (expr != null)
                expr.UpdateSource();

            if (OnChangeCommitted != null)
            {
                OnChangeCommitted();
            }
            Pending = false;

            //dynSettings.Bench.mainGrid.Focus();
        }

        new public string Text
        {
            get { return base.Text; }
            set
            {
                //base.Text = value;
                Commit();
            }
        }

        private bool shouldCommit()
        {
            return !dynSettings.Controller.DynamoViewModel.DynamicRunEnabled;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            Pending = true;

            if (IsNumeric)
            {
                var p = CaretIndex;

                //base.Text = dynSettings.RemoveChars(
                //   Text,
                //   Text.ToCharArray()
                //      .Where(c => !char.IsDigit(c) && c != '-' && c != '.')
                //      .Select(c => c.ToString())
                //);

                CaretIndex = p;
            }
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                dynSettings.ReturnFocusToSearch();
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            Commit();
        }
    }

    public class dynStringTextBox : dynTextBox
    {

        public dynStringTextBox()
        {
            Commit();
            Pending = false;
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            //if (e.Key == Key.Return || e.Key == Key.Enter)
            //{
            //    dynSettings.ReturnFocusToSearch();
            //}
        }

    }

    public abstract partial class dynString : dynBasicInteractive<string>
    {
        public override void editWindowItem_Click(object sender, RoutedEventArgs e)
        {

            var editWindow = new dynEditWindow {DataContext = this};

            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new StringDisplay(),
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            editWindow.editText.SetBinding(TextBox.TextProperty, bindingVal);

            if (editWindow.ShowDialog() != true)
            {
                return;
            }
        }
    }
}
