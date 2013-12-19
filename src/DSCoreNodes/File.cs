using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBox = System.Windows.Controls.TextBox;

namespace DSCoreNodes
{
    public class File
    {
        [Browsable(false)]
        public abstract class FileSystemBrowser : NodeModel
        {
            protected FileSystemBrowser(string tip)
            {
                OutPortData.Add(new PortData("", tip));
                RegisterAllPorts();

                Value = "";
            }

            private string _value;
            public string Value
            {
                get { return _value; }
                set
                {
                    if (value != null && (_value == null || !_value.Equals(value)))
                    {
                        _value = value;
                        RequiresRecalc = true;
                        RaisePropertyChanged("Value");
                    }
                }
            }

            public override void SetupCustomUIElements(dynNodeView view)
            {
                //add a button to the inputGrid on the dynElement
                var readFileButton = new NodeButton
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top
                };

                readFileButton.Click += readFileButton_Click;
                readFileButton.Content = "Browse...";
                readFileButton.HorizontalAlignment = HorizontalAlignment.Stretch;
                readFileButton.VerticalAlignment = VerticalAlignment.Center;

                var tb = new TextBox();
                if (string.IsNullOrEmpty(Value))
                    Value = "No file selected.";

                tb.HorizontalAlignment = HorizontalAlignment.Stretch;
                tb.VerticalAlignment = VerticalAlignment.Center;
                var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                tb.Background = backgroundBrush;
                tb.BorderThickness = new Thickness(0);
                tb.IsReadOnly = true;
                tb.IsReadOnlyCaretVisible = false;
                tb.TextChanged += delegate { tb.ScrollToHorizontalOffset(double.PositiveInfinity); dynSettings.ReturnFocusToSearch(); };

                var sp = new StackPanel();
                sp.Children.Add(readFileButton);
                sp.Children.Add(tb);
                view.inputGrid.Children.Add(sp);

                tb.DataContext = this;
                var bindingVal = new System.Windows.Data.Binding("Value")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new FilePathDisplayConverter()
                };
                tb.SetBinding(TextBox.TextProperty, bindingVal);
            }

            protected abstract void readFileButton_Click(object sender, RoutedEventArgs e);

            public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstFactory.BuildStringNode(Value))
                };
            }

            protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
            {
                nodeElement.InnerText = Value;
            }

            protected override void LoadNode(XmlNode nodeElement)
            {
                Value = nodeElement.InnerText;
            }
        }

        public class Filename : FileSystemBrowser
        {
            public Filename() : base("Filename") { }

            protected override void readFileButton_Click(object sender, RoutedEventArgs e)
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

        public class Directory : FileSystemBrowser
        {
            public Directory() : base("Directory") { }

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
    }
}
